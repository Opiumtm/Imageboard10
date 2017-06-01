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
            if (stream is InlineBlobStream)
            {
                return BlobStreamKind.Inlined;
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
        Inlined,
        /// <summary>
        /// Не является BlobStream.
        /// </summary>
        NotBlobStream
    }
}