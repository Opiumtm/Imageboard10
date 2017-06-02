using System;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Информация о файле.
    /// </summary>
    public struct BlobInfo
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        public BlobId Id;

        /// <summary>
        /// Имя файла.
        /// </summary>
        public string UniqueName;

        /// <summary>
        /// Категория.
        /// </summary>
        public string Category;

        /// <summary>
        /// Время создания.
        /// </summary>
        public DateTime CreatedTime;

        /// <summary>
        /// Размер.
        /// </summary>
        public long Size;
    }
}