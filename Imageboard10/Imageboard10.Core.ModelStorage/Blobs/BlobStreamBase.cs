using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Imageboard10.Core.Database;
using Imageboard10.Core.Tasks;
using Imageboard10.ModuleInterface;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Базовый класс потока блоба.
    /// </summary>
    internal abstract class BlobStreamBase : Stream
    {
        /// <summary>
        /// Провайдер ESENT.
        /// </summary>
        protected readonly IEsentInstanceProvider Esent;

        /// <summary>
        /// Хранилище файлов.
        /// </summary>
        protected readonly IBlobsModelStore BlobStore;

        /// <summary>
        /// Обработчик глобальных ошибок.
        /// </summary>
        protected readonly IGlobalErrorHandler GlobalErrorHandler;

        /// <summary>
        /// Идентификатор блокировки.
        /// </summary>
        private readonly BlobLockId? _lockId;

        private int _isClosed;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="esent">Провайдер ESENT.</param>
        /// <param name="blobStore">Хранилище файлов.</param>
        /// <param name="globalErrorHandler">Обработчик глобальных ошибок.</param>
        /// <param name="lockId">Идентификатор блокировки.</param>
        protected BlobStreamBase(IEsentInstanceProvider esent, IBlobsModelStore blobStore, IGlobalErrorHandler globalErrorHandler, BlobLockId? lockId)
        {
            Esent = esent ?? throw new ArgumentNullException(nameof(esent));
            BlobStore = blobStore ?? throw new ArgumentNullException(nameof(blobStore));
            GlobalErrorHandler = globalErrorHandler;
            _lockId = lockId;
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanSeek => true;

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            async Task DoDispose()
            {
                try
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    await BlobStore.UnlockBlob(_lockId.Value);
                }
                catch (Exception ex)
                {
                    GlobalErrorHandler?.SignalError(ex);
                }
            }

            base.Dispose(disposing);
            if (disposing)
            {
                if (_lockId != null)
                {
                    CoreTaskHelper.RunUnawaitedTaskAsync(DoDispose);
                    Interlocked.Exchange(ref _isClosed, 1);
                }
            }
        }

        /// <summary>
        /// Проверить, закрыт ли поток.
        /// </summary>
        protected void CheckClosed()
        {
            if (Interlocked.CompareExchange(ref _isClosed, 0, 0) != 0)
            {
                throw new ObjectDisposedException("BlobStream");
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Нельзя изменять данные в потоке BlobStream");
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException("Нельзя изменять данные в потоке BlobStream");
        }
    }
}