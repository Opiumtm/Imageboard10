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
        /// <summary>
        /// Поток данных в памяти.
        /// </summary>
        private readonly ColumnStream _inlinedStream;

        private readonly Transaction _transaction;

        private EsentTable _table;

        private readonly IDisposable _usage;

        private readonly IEsentSession _session;

        /// <summary>
        /// Конструктор. Должен вызываться из потока сессии.
        /// </summary>
        /// <param name="globalErrorHandler">Обработчик глобальных ошибок.</param>
        /// <param name="session">Сессия.</param>
        /// <param name="blobId">Идентификатор блоба.</param>
        public BlocksBlobStream(IGlobalErrorHandler globalErrorHandler, IEsentSession session, BlobId blobId) : base(globalErrorHandler)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            var sid = _session.Session;
            try
            {
                _usage = _session.UseSession();
                _transaction = new Transaction(session.Session);
                try
                {
                    _table = session.OpenTable(BlobTableInfo.BlobsTable, OpenTableGrbit.ReadOnly);
                    try
                    {
                        Api.MakeKey(sid, _table, blobId.Id, MakeKeyGrbit.NewKey);
                        if (!Api.TrySeek(sid, _table, SeekGrbit.SeekEQ))
                        {
                            throw new BlobNotFoundException(blobId);
                        }
                        _inlinedStream = new ColumnStream(sid, _table, Api.GetTableColumnid(sid, _table, BlobTableInfo.BlobTableColumns.Data));
                        Length = _inlinedStream.Length;
                    }
                    catch
                    {
                        _table.Dispose();
                    }
                }
                catch
                {
                    _transaction.Dispose();
                    throw;
                }
            }
            catch
            {
                _usage.Dispose();
                throw;
            }
        }

        public override void Flush()
        {
            // не вызывает ESENT API, поэтому можно вызывать с любого треда
            _inlinedStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var t = ReadAsync(buffer, offset, count, CancellationToken.None);
            if (!t.Wait(TimeSpan.FromSeconds(5)))
            {
                throw new TimeoutException();
            }
            return t.Result;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int result = 0;
            await _session.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                result = _inlinedStream.Read(buffer, offset, count);
            });
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            // не вызывает ESENT API, поэтому можно вызывать с любого треда
            return _inlinedStream.Seek(offset, origin);
        }

        public override long Length { get; }

        public override long Position
        {
            // не вызывает ESENT API, поэтому можно вызывать с любого треда
            get => _inlinedStream.Position;
            // не вызывает ESENT API, поэтому можно вызывать с любого треда
            set => _inlinedStream.Position = value;
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                try
                {
                    _inlinedStream.Dispose();
                }
                finally
                {
                    try
                    {
                        _table.Dispose();
                    }
                    finally
                    {
                        try
                        {
                            _transaction.Dispose();
                        }
                        finally
                        {
                            _usage.Dispose();
                        }
                    }
                }
            }
        }
    }
}