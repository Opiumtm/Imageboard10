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
        /// Имя таблицы.
        /// </summary>
        public const string BlobsTable = "blobs";

        /// <summary>
        /// Столбцы.
        /// </summary>
        public static class BlobTableColumns
        {
            /// <summary>
            /// Идентификатор.
            /// </summary>
            public const string Id = "Id";

            /// <summary>
            /// Имя файла.
            /// </summary>
            public const string Name = "Name";

            /// <summary>
            /// Категория.
            /// </summary>
            public const string Category = "Category";

            /// <summary>
            /// Размер.
            /// </summary>
            public const string Length = "Length";

            /// <summary>
            /// Дата создания.
            /// </summary>
            public const string CreatedDate = "CreatedDate";

            /// <summary>
            /// Встроенные данные.
            /// </summary>
            public const string Data = "Data";
        }

        /// <summary>
        /// Индексы.
        /// </summary>
        public static class BlobTableIndexes
        {
            /// <summary>
            /// Первичный ключ.
            /// </summary>
            public const string Primary = "PK_blobs";

            /// <summary>
            /// Имя файла.
            /// </summary>
            public const string Name = "IX_blobs_Name";

            /// <summary>
            /// Категория.
            /// </summary>
            public const string Category = "IX_blobs_Category";
        }
    }
}