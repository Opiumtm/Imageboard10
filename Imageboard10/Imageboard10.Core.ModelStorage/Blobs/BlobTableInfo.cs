namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Информация о таблице с блобами.
    /// </summary>
    internal static class BlobTableInfo
    {
        /// <summary>
        /// Максимальный размер файла для упрощённого доступа.
        /// </summary>
        public const int MaxInlineSize = 64 * 1024;

        /// <summary>
        /// Размер, начиная с которого данные хранятся в виде файла на диске.
        /// </summary>
        public const long FileStreamSize = 256 * 1024;

        /// <summary>
        /// Макисмальный размер файла. Если размер файла превышает этот - при сохранении файла кидать исключение.
        /// </summary>
        public const long MaxFileSize = 64 * 1024 * 1024;

        /// <summary>
        /// Имя таблицы.
        /// </summary>
        public const string BlobsTableName = "blobs";

        /// <summary>
        /// Имя таблицы.
        /// </summary>
        public const string ReferencesTableName = "blobs_refs";
    }
}