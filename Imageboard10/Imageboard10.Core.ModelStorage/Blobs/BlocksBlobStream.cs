using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Imageboard10.Core.Database;
using Imageboard10.ModuleInterface;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    internal sealed class BlocksBlobStream : BlobStreamBase
    {
        private readonly BlobId _blobId;

        public BlocksBlobStream(IEsentInstanceProvider esent, IBlobsModelStore blobStore, IGlobalErrorHandler globalErrorHandler, BlobLockId lockId, long length, BlobId blobId)
            : base(esent, blobStore, globalErrorHandler, lockId)
        {
            Length = length;
            _blobId = blobId;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (count < 1)
            {
                return 0;
            }
            using (var ev = new ManualResetEvent(false))
            {
                int result = 0;
                Exception error = null;
                var session = Esent.GetReadOnlySession();
                using (var cts = new CancellationTokenSource())
                {
                    var token = cts.Token;
                    session.Run(() =>
                    {
                        try
                        {
                            result = DoRead(session, buffer, offset, count, token);
                        }
                        catch (Exception ex)
                        {
                            error = ex;
                        }
                        finally
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            ev.Set();
                        }
                    });
                    if (!ev.WaitOne(TimeSpan.FromSeconds(15)))
                    {
                        cts.Cancel();
                        throw new TimeoutException();
                    }
                    if (error != null)
                    {
                        throw error;
                    }
                    return result;
                }
            }
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (count < 1)
            {
                return 0;
            }
            var session = Esent.GetReadOnlySession();
            int result = 0;
            await session.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                result = DoRead(session, buffer, offset, count, cancellationToken);
            });
            return result;
        }

        private int DoRead(IEsentSession session, byte[] buffer, int offset, int count, CancellationToken token)
        {
            CheckClosed();
            token.ThrowIfCancellationRequested();
            using (session.UseSession())
            {
                var position = Interlocked.CompareExchange(ref _position, 0, 0);
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    int blockSize;
                    using (var table = session.OpenTable(BlobTableInfo.BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        Api.MakeKey(sid, table, _blobId.Id, MakeKeyGrbit.NewKey);
                        if (!Api.TrySeek(sid, table, SeekGrbit.SeekEQ))
                        {
                            throw new BlobNotFoundException(_blobId);
                        }
                        blockSize = Api.RetrieveColumnAsInt32(sid, table,
                                        Api.GetTableColumnid(sid, table,
                                            BlobTableInfo.BlobTableColumns.BlockSize)) ?? 0;
                        if (blockSize < 1024)
                        {
                            throw new BlobException(
                                $"Ошибка в данных в таблице {BlobTableInfo.BlobsTable}. Размер блока < 1024");
                        }
                    }
                    using (var table = session.OpenTable(BlobTableInfo.BlockTable, OpenTableGrbit.ReadOnly))
                    {
                        var firstBlock = (int) (position / blockSize);
                        var lastBlock = (int) ((position + count - 1) / blockSize);
                        int read = count;
                        var colid = Api.GetTableColumnid(sid, table, BlobTableInfo.BlocksTableColumns.Data);
                        int bufferOfs = offset;
                        int firstOfs = (int) (position % blockSize);
                        int toRead = count;
                        for (var blockNum = firstBlock; blockNum <= lastBlock; blockNum++)
                        {
                            token.ThrowIfCancellationRequested();
                            // Прочитали все данные.
                            if (toRead <= 0)
                            {
                                break;
                            }

                            Api.MakeKey(sid, table, _blobId.Id, MakeKeyGrbit.NewKey);
                            Api.MakeKey(sid, table, blockNum, MakeKeyGrbit.None);

                            // Блок не найден. Предположительно предыдущий блок был последним.
                            if (!Api.TrySeek(sid, table, SeekGrbit.SeekEQ))
                            {
                                break;
                            }
                            var data = Api.RetrieveColumn(sid, table, colid);
                            if (data == null)
                            {
                                throw new BlobException(
                                    $"Ошибка в данных в таблице {BlobTableInfo.BlockTable}. Блок {_blobId.Id}:{blockNum} == null");
                            }

                            var toCopy = Math.Min(toRead, data.Length - firstOfs);
                            // Размер блока меньше ожидаемого. Т.е. предположительно последний блок.
                            if (toCopy <= 0)
                            {
                                break;
                            }
                            Array.Copy(data, firstOfs, buffer, bufferOfs, toCopy);
                            toRead -= toCopy;
                            bufferOfs += toCopy;
                            firstOfs = 0;
                            read += toCopy;
                        }
                        return read;
                    }
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckClosed();
            long position = Interlocked.CompareExchange(ref _position, 0, 0);
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;
                case SeekOrigin.End:
                    position = Length - offset;
                    break;
                case SeekOrigin.Current:
                    position = position + offset;
                    break;
            }
            if (position < 0)
            {
                position = 0;
            }
            if (position > Length)
            {
                position = Length;
            }
            Interlocked.Exchange(ref _position, position);
            return position;
        }

        public override long Length { get; }

        private long _position;

        public override long Position
        {
            get => Interlocked.CompareExchange(ref _position, 0, 0);
            set => Seek(value, SeekOrigin.Begin);
        }
    }
}