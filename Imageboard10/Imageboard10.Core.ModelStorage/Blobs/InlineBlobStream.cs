using System;
using System.IO;
using Imageboard10.Core.Database;
using Imageboard10.ModuleInterface;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Поток для чтения 
    /// </summary>
    internal sealed class InlineBlobStream : BlobStreamBase
    {
        /// <summary>
        /// Поток данных в памяти.
        /// </summary>
        private readonly MemoryStream _inlinedStream;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="esent">Провайдер ESENT.</param>
        /// <param name="blobStore">Хранилище файлов.</param>
        /// <param name="globalErrorHandler">Обработчик глобальных ошибок.</param>
        /// <param name="data">Данные.</param>
        public InlineBlobStream(IEsentInstanceProvider esent, IBlobsModelStore blobStore, IGlobalErrorHandler globalErrorHandler, byte[] data) : base(esent, blobStore, globalErrorHandler, null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _inlinedStream = new MemoryStream(data);
        }

        public override void Flush()
        {
            _inlinedStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inlinedStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inlinedStream.Seek(offset, origin);
        }

        public override long Length => _inlinedStream.Length;

        public override long Position
        {
            get => _inlinedStream.Position;
            set => _inlinedStream.Position = value;
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _inlinedStream.Dispose();
            }
        }
    }
}