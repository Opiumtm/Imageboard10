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
        /// Имя таблицы.
        /// </summary>
        public const string ReferencesTable = "blobs_refs";

        /// <summary>
        /// Столбцы.
        /// </summary>
        public static class BlobsTableColumns
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

            /// <summary>
            /// Идентификатор ссылки.
            /// </summary>

            public const string ReferenceId = "ReferenceId";

            /// <summary>
            /// Загрузка завершена.
            /// </summary>
            public const string IsCompleted = "IsCompleted";
        }

        /// <summary>
        /// Индексы.
        /// </summary>
        public static class BlobsTableIndexes
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

            /// <summary>
            /// Идентификатор ссылки.
            /// </summary>
            public const string ReferenceId = "IX_blobs_ReferenceId";

            /// <summary>
            /// Идентификатор ссылки.
            /// </summary>
            public const string IsCompleted = "IX_blobs_IsCompleted";
        }

        /// <summary>
        /// Столбцы таблицы ссылок.
        /// </summary>
        public static class ReferencesTableColumns
        {
            /// <summary>
            /// Идентификатор ссылки.
            /// </summary>

            public const string ReferenceId = "ReferenceId";
        }

        /// <summary>
        /// Столбцы таблицы ссылок.
        /// </summary>
        public static class ReferencesTableIndexes
        {
            /// <summary>
            /// Первичный ключ.
            /// </summary>

            public const string Primary = "PK_blobs_refs";
        }
    }
}