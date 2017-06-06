using System;
using System.IO;
using Imageboard10.Core.Database;
using Imageboard10.ModuleInterface;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Поток для чтения 
    /// </summary>
    internal sealed class InlineBlobStream : InlineBlobStreamBase
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="globalErrorHandler">Обработчик глобальных ошибок.</param>
        /// <param name="data">Данные.</param>
        public InlineBlobStream(IGlobalErrorHandler globalErrorHandler, byte[] data) : base(globalErrorHandler, new MemoryStream(data ?? throw new ArgumentNullException(nameof(data))))
        {
        }
    }
}