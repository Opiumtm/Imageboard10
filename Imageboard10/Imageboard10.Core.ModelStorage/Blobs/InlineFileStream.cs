using System.IO;
using Imageboard10.ModuleInterface;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Поток для чтения из файла.
    /// </summary>
    internal sealed class InlineFileStream : InlineBlobStreamBase
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="globalErrorHandler">Обработчик глобальных ошибок.</param>
        /// <param name="inlinedStream">Данные.</param>
        public InlineFileStream(IGlobalErrorHandler globalErrorHandler, Stream inlinedStream) : base(globalErrorHandler, inlinedStream)
        {
        }
    }
}