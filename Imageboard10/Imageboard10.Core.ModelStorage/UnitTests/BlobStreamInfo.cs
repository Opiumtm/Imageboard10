using System.IO;
using Imageboard10.Core.ModelStorage.Blobs;

namespace Imageboard10.Core.ModelStorage.UnitTests
{
    /// <summary>
    /// Информация о BlobStream.
    /// </summary>
    public static class BlobStreamInfo
    {
        /// <summary>
        /// Получить тип потока.
        /// </summary>
        /// <param name="stream">Поток.</param>
        /// <returns>Тип.</returns>
        public static BlobStreamKind GetBlobStreamKind(Stream stream)
        {
            if (stream is BlocksBlobStream)
            {
                return BlobStreamKind.Normal;
            }
            if (stream is InlineFileStream)
            {
                return BlobStreamKind.Filestream;
            }
            if (stream is InlineBlobStream)
            {
                return BlobStreamKind.Memory;
            }
            return BlobStreamKind.NotBlobStream;
        }
    }

    /// <summary>
    /// Тип BlobStream.
    /// </summary>
    public enum BlobStreamKind
    {
        /// <summary>
        /// Нормальный поток.
        /// </summary>
        Normal,
        /// <summary>
        /// Встроенный в основную таблицу.
        /// </summary>
        Memory,
        /// <summary>
        /// Из файловой системы.
        /// </summary>
        Filestream,
        /// <summary>
        /// Не является BlobStream.
        /// </summary>
        NotBlobStream
    }
}