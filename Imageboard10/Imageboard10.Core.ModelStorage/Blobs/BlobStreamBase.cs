using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        /// Обработчик глобальных ошибок.
        /// </summary>
        protected readonly IGlobalErrorHandler GlobalErrorHandler;


        private int _isClosed;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="globalErrorHandler">Обработчик глобальных ошибок.</param>
        protected BlobStreamBase(IGlobalErrorHandler globalErrorHandler)
        {
            GlobalErrorHandler = globalErrorHandler;
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanSeek => true;

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                Interlocked.Exchange(ref _isClosed, 1);
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