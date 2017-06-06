using System;
using System.IO;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Входной файл.
    /// </summary>
    public struct InputBlob
    {
        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        public string UniqueName;

        /// <summary>
        /// Категория.
        /// </summary>
        public string Category;

        /// <summary>
        /// Данные файла. Режим "только для чтения". Должен быть позиционирован на начало файла.
        /// </summary>
        public Stream BlobStream;

        /// <summary>
        /// Максимальный размер (если данные содержат кроме файла что-то ещё). Если null - чтение до конца файла.
        /// </summary>
        public long? MaxSize;

        /// <summary>
        /// Идентификатор ссылки.
        /// </summary>
        public Guid? ReferenceId;

        /// <summary>
        /// Для юнит-тестов.
        /// </summary>
        public bool RememberTempFile;
    }
}