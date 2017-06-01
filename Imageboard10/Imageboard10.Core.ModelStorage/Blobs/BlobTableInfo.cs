namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Информация о таблице с блобами.
    /// </summary>
    internal static class BlobTableInfo
    {
        /// <summary>
        /// Максимальный размер блока.
        /// </summary>
        public const int MaxBlockSize = 16 * 1024;

        /// <summary>
        /// Максимальный размер файла для встраивания (в блоках).
        /// </summary>
        public const int MaxInlineSize = 4;

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
            /// Размер блока.
            /// </summary>
            public const string BlockSize = "BlockSize";

            /// <summary>
            /// Дата создания.
            /// </summary>
            public const string CreatedDate = "CreatedDate";

            /// <summary>
            /// Данные файла хранятся в основной таблице.
            /// </summary>
            public const string IsInlined = "IsInlined";

            /// <summary>
            /// Встроенные данные.
            /// </summary>
            public const string InlineData = "InlineData";

            /// <summary>
            /// Идентификатор последней блокировки.
            /// </summary>
            public const string BlockId = "BlockId";

            /// <summary>
            /// Идентификатор последней разблокировки.
            /// </summary>
            public const string UblockId = "UblockId";

            /// <summary>
            /// Максимальное время блокировки.
            /// </summary>
            public const string BlockUntil = "BlockUntil";

            /// <summary>
            /// Успешно сохранено.
            /// </summary>
            public const string Commited = "Commited";
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

            /// <summary>
            /// Завершённые блобы.
            /// </summary>
            public const string Commited = "IX_blobs_Commited";
        }

        /// <summary>
        /// Имя таблицы с блоками.
        /// </summary>
        public const string BlockTable = "blobs_blocks";

        /// <summary>
        /// Столбцы таблицы с блоками.
        /// </summary>
        public static class BlocksTableColumns
        {
            /// <summary>
            /// Идентификатор блоба.
            /// </summary>
            public const string BlobId = "BlobId";

            /// <summary>
            /// Номер блока.
            /// </summary>
            public const string Counter = "Counter";

            /// <summary>
            /// Данные.
            /// </summary>
            public const string Data = "Data";
        }

        /// <summary>
        /// Индексы.
        /// </summary>
        public static class BlocksTableIndexes
        {
            /// <summary>
            /// Первичный ключ.
            /// </summary>
            public const string Primary = "PK_blobs_blocks";

            /// <summary>
            /// Первичный ключ.
            /// </summary>
            public const string BlobId = "IX_blobs_blocks_BlobId";
        }
    }
}