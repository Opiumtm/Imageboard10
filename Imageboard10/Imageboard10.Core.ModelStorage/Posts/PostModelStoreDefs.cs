using Imageboard10.Core.Database;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Хранилище постов.
    /// </summary>
    public partial class PostModelStore
    {
        /// <summary>
        /// Идентификатор движка.
        /// </summary>
        protected string EngineId { get; }

        /// <summary>
        /// Имя таблицы.
        /// </summary>
        protected string TableName => $"posts_{EngineId}";

        /// <summary>
        /// Первичный индекс.
        /// </summary>
        protected string TablePkName => $"PK_{TableName}";

        /// <summary>
        /// Имя таблицы лога доступа.
        /// </summary>
        protected string AccessLogTableName => $"pacclog_{EngineId}";

        /// <summary>
        /// Первичный индекс таблицы лога доступа.
        /// </summary>
        protected string AccessLogPkName => $"PK_{AccessLogTableName}";

        /// <summary>
        /// Таблица медиафайлов.
        /// </summary>
        protected string MediaFilesTableName => $"postmedia_{EngineId}";

        /// <summary>
        /// Первичный индекс таблицы медиа файлов.
        /// </summary>
        protected string MediaFilesPkName => $"PK_{MediaFilesTableName}";

        /// <summary>
        /// Имена столбцов.
        /// </summary>
        protected static class ColumnNames
        {
            /// <summary>
            /// Идентификатор сущности.
            /// </summary>
            public const string Id = "Id";

            /// <summary>
            /// Ссылка на родительскую сущность (multi-value).
            /// </summary>
            public const string ParentId = "ParentId";

            /// <summary>
            /// Ссылка на непосредственную родительскую сущность (multi-value).
            /// </summary>
            public const string DirectParentId = "DirectParentId";

            /// <summary>
            /// Тип сущности.
            /// </summary>
            public const string EntityType = "EntityType";

            /// <summary>
            /// Загружены данные кроме основной ссылки.
            /// </summary>
            public const string DataLoaded = "DataLoaded";

            /// <summary>
            /// Состояние загрузки дочерних сущностей.
            /// </summary>
            public const string ChildrenLoadStage = "ChildrenLoadStage";

            /// <summary>
            /// Идентификатор доски.
            /// </summary>
            public const string BoardId = "BoardId";

            /// <summary>
            /// Порядковый номер (в общем случае - идентификатор поста на доске, для страницы доски - номер страницы, для каталога - тип сортировки).
            /// </summary>
            public const string SequenceNumber = "SequenceNumber";

            /// <summary>
            /// Порядковый номер родительской сущности (только для постов).
            /// </summary>
            public const string ParentSequenceNumber = "ParentSequenceNumber";

            /// <summary>
            /// Заголовок.
            /// </summary>
            public const string Subject = "Subject";

            /// <summary>
            /// Превью картинки.
            /// </summary>
            public const string Thumbnail = "Thumbnail";

            /// <summary>
            /// Дата поста.
            /// </summary>
            public const string Date = "Date";

            /// <summary>
            /// Дата поста (в формате строки).
            /// </summary>
            public const string BoardSpecificDate = "BoardSpecificDate";

            /// <summary>
            /// Флаги поста (multi-value).
            /// </summary>
            public const string Flags = "Flags";

            /// <summary>
            /// Тэги (multi-value).
            /// </summary>
            public const string ThreadTags = "ThreadTags";

            /// <summary>
            /// Лайки.
            /// </summary>
            public const string Likes = "Likes";

            /// <summary>
            /// Дизлайки.
            /// </summary>
            public const string Dislikes = "Dislikes";

            /// <summary>
            /// Документ (комментарий).
            /// </summary>
            public const string Document = "Document";

            /// <summary>
            /// Список процитированных постов в этом документе (multi-value).
            /// </summary>
            public const string QuotedPosts = "QuotedPosts";

            /// <summary>
            /// Время загрузки поста.
            /// </summary>
            public const string LoadedTime = "LoadedTime";

            /// <summary>
            /// ETAG.
            /// </summary>
            public const string Etag = "Etag";

            /// <summary>
            /// Имя автора поста.
            /// </summary>
            public const string PosterName = "PosterName";

            /// <summary>
            /// Дополнительные данные в двоичном формате.
            /// </summary>
            public const string OtherDataBinary = "OtherDataBinary";

            /// <summary>
            /// Количества (превью треда).
            /// </summary>
            public const string PreviewCounts = "PreviewCounts";

            /// <summary>
            /// Время последнего апдейта на сервере.
            /// </summary>
            public const string LastServerUpdate = "LastServerUpdate";

            /// <summary>
            /// Количество постов на сервере.
            /// </summary>
            public const string NumberOfPostsOnServer = "NumberOfPostsOnServer";

            /// <summary>
            /// Количество прочитанных постов.
            /// </summary>
            public const string NumberOfReadPosts = "NumberOfReadPosts";

            /// <summary>
            /// Последний пост на сервере.
            /// </summary>
            public const string LastPostLinkOnServer = "LastPostLinkOnServer";

            /// <summary>
            /// Порядковый номер на сервере. Сейчас не пригодно к использованию, но возможно в будущем Makaba API будет изменено в лучшую сторону.
            /// </summary>
            public const string OnServerSequenceCounter = "OnServerSequenceCounter";
        }

        /// <summary>
        /// Индексы.
        /// </summary>
        protected static class Indexes
        {
            /// <summary>
            /// Родительская сущность.
            /// </summary>
            public static readonly IndexDefinition ParentId = new IndexDefinition()
            {
                Fields = new[] {"+" + ColumnNames.ParentId},
                Grbit = CreateIndexGrbit.IndexIgnoreAnyNull
            };

            /// <summary>
            /// Тип сущности и идентификатор.
            /// </summary>
            public static readonly IndexDefinition Type = new IndexDefinition()
            {
                Fields = new[] {"+" + ColumnNames.EntityType},
                Grbit = CreateIndexGrbit.None
            };

            /// <summary>
            /// Тип сущности и идентификатор.
            /// </summary>
            public static readonly IndexDefinition TypeAndId = new IndexDefinition()
            {
                Fields = new[]
                {
                    "+" + ColumnNames.EntityType,
                    "+" + ColumnNames.Id
                },
                Grbit = CreateIndexGrbit.None
            };

            /// <summary>
            /// Состояние загрузки дочерних элементов.
            /// </summary>
            public static readonly IndexDefinition ChildrenLoadStage = new IndexDefinition()
            {
                Fields = new[] {"+" + ColumnNames.ChildrenLoadStage},
                Grbit = CreateIndexGrbit.None
            };

            /// <summary>
            /// Тип сущности и идентификатор поста.
            /// </summary>
            public static readonly IndexDefinition TypeAndPostId = new IndexDefinition()
            {
                Fields = new[]
                {
                    "+" + ColumnNames.EntityType,
                    "+" + ColumnNames.BoardId,
                    "+" + ColumnNames.SequenceNumber
                },
                Grbit = CreateIndexGrbit.None
            };

            /// <summary>
            /// Флаги.
            /// </summary>
            public static readonly IndexDefinition Flags = new IndexDefinition()
            {
                Fields = new[] {"+" + ColumnNames.Flags},
                Grbit = CreateIndexGrbit.None
            };

            /// <summary>
            /// Цитаты.
            /// </summary>
            public static readonly IndexDefinition QuotedPosts = new IndexDefinition()
            {
                Fields = new[]
                {
                    "+" + ColumnNames.DirectParentId,
                    "+" + ColumnNames.QuotedPosts
                },
                Grbit = CreateIndexGrbit.IndexIgnoreAnyNull
            };

            /// <summary>
            /// Ссылка на пост в треде.
            /// </summary>
            public static readonly IndexDefinition InThreadPostLink = new IndexDefinition()
            {
                Fields = new[]
                {
                    "+" + ColumnNames.DirectParentId,
                    "+" + ColumnNames.SequenceNumber
                },
                Grbit = CreateIndexGrbit.IndexIgnoreAnyNull
            };
        }

        /// <summary>
        /// Имена столбцов таблицы лога доступа.
        /// </summary>
        protected static class AccessLogColumnNames
        {
            /// <summary>
            /// Идентификатор.
            /// </summary>
            public const string Id = "Id";

            /// <summary>
            /// Идентификатор сущности.
            /// </summary>
            public const string EntityId = "EntityId";

            /// <summary>
            /// Время доступа.
            /// </summary>
            public const string AccessTime = "AccessTime";
        }

        /// <summary>
        /// Индексы таблицы лога доступа.
        /// </summary>
        protected static class AccessLogIndexes
        {
            /// <summary>
            /// Сущность.
            /// </summary>
            public static readonly IndexDefinition EntityId = new IndexDefinition()
            {
                Fields = new[] {"+" + AccessLogColumnNames.EntityId},
                Grbit = CreateIndexGrbit.None
            };

            /// <summary>
            /// Время доступа.
            /// </summary>
            public static readonly IndexDefinition AccessTime = new IndexDefinition()
            {
                Fields = new[] {"+" + AccessLogColumnNames.AccessTime},
                Grbit = CreateIndexGrbit.None
            };
        }

        /// <summary>
        /// Имена столбцов таблицы медиа файлов.
        /// </summary>
        protected static class MediaFilesColumnNames
        {
            /// <summary>
            /// Идентификатор.
            /// </summary>
            public const string Id = "Id";

            /// <summary>
            /// Ссылки на сущности (multi-value).
            /// </summary>
            public const string EntityReferences = "EntityReferences";

            /// <summary>
            /// Номер последовательности поста. Для составного ключа, определяющего порядок медиа-файлов в каталоге медиа.
            /// </summary>
            public const string EntitySequenceNumber = "PostSequenceNumber";

            /// <summary>
            /// Номер последовательности относительно сущности. Для составного ключа, определяющего порядок медиа-файлов в каталоге медиа.
            /// </summary>
            public const string MediaSequenceNumber = "MediaSequenceNumber";

            /// <summary>
            /// Бинарное представление информации о медиафайле.
            /// </summary>
            public const string MediaData = "MediaData";
        }

        /// <summary>
        /// Индексы таблицы медиа файлов.
        /// </summary>
        protected static class MediaFilesIndexes
        {
            /// <summary>
            /// Сущность.
            /// </summary>
            public static readonly IndexDefinition EntityReferences = new IndexDefinition()
            {
                Fields = new[] {"+" + MediaFilesColumnNames.EntityReferences},
                Grbit = CreateIndexGrbit.None
            };

            /// <summary>
            /// Последовательности.
            /// </summary>
            public static readonly IndexDefinition Sequences = new IndexDefinition()
            {
                Fields = new[]
                {
                    "+" + MediaFilesColumnNames.EntityReferences,
                    "+" + MediaFilesColumnNames.EntitySequenceNumber,
                    "+" + MediaFilesColumnNames.MediaSequenceNumber,
                },
                Grbit = CreateIndexGrbit.None
            };
        }

        /// <summary>
        /// Этап загрузки дочерних сущностей.
        /// </summary>
        protected static class ChildrenLoadStageId
        {
            /// <summary>
            /// Не начато.
            /// </summary>
            public const byte NotStarted = 0;

            /// <summary>
            /// Начато.
            /// </summary>
            public const byte Started = 1;

            /// <summary>
            /// Завершено.
            /// </summary>
            public const byte Completed = 2;
        }

        /// <summary>
        /// Инициализировать основную таблицу.
        /// </summary>
        /// <param name="session">Сессия.</param>
        /// <param name="tableid">Идентификатор таблицы.</param>
        protected virtual void InitializeMainTable(IEsentSession session, JET_TABLEID tableid)
        {
            var sid = session.Session;
            JET_COLUMNID tempcolid;
            Api.JetAddColumn(sid, tableid, ColumnNames.Id, new JET_COLUMNDEF()
            {
                coltyp = VistaColtyp.GUID,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.ParentId, new JET_COLUMNDEF()
            {
                coltyp = VistaColtyp.GUID,
                grbit = ColumndefGrbit.ColumnMultiValued | ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.DirectParentId, new JET_COLUMNDEF()
            {
                coltyp = VistaColtyp.GUID,
                grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.EntityType, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.UnsignedByte,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.DataLoaded, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Bit,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.ChildrenLoadStage, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.UnsignedByte,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.BoardId, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Text,
                grbit = ColumndefGrbit.ColumnNotNULL,
                cbMax = 50,
                cp = JET_CP.Unicode
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.SequenceNumber, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.ParentSequenceNumber, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.Subject, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnTagged,
                cp = JET_CP.Unicode
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.Thumbnail, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongBinary,
                grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.Date, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.DateTime,
                grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.BoardSpecificDate, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnTagged,
                cp = JET_CP.Unicode
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.Flags, new JET_COLUMNDEF()
            {
                coltyp = VistaColtyp.GUID,
                grbit = ColumndefGrbit.ColumnMultiValued | ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.ThreadTags, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnMultiValued | ColumndefGrbit.ColumnTagged,
                cp = JET_CP.Unicode
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.Likes, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.Dislikes, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.Document, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongBinary,
                grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.QuotedPosts, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnMultiValued | ColumndefGrbit.ColumnTagged
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.LoadedTime, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.DateTime,
                grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.Etag, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnTagged,
                cp = JET_CP.Unicode
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.PosterName, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnTagged,
                cp = JET_CP.Unicode
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.OtherDataBinary, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongBinary,
                grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.PreviewCounts, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongBinary,
                grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.LastServerUpdate, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.DateTime,
                grbit = ColumndefGrbit.ColumnTagged
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.NumberOfPostsOnServer, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnTagged
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.NumberOfReadPosts, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnTagged
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.LastPostLinkOnServer, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnTagged,
                cp = JET_CP.Unicode
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.OnServerSequenceCounter, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);

            var pkDef = $"+{ColumnNames.Id}\0\0";
            Api.JetCreateIndex(sid, tableid, TablePkName, CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, pkDef, pkDef.Length, 100);
            CreateIndex(sid, tableid, TableName, nameof(Indexes.Flags), Indexes.Flags);
            CreateIndex(sid, tableid, TableName, nameof(Indexes.ChildrenLoadStage), Indexes.ChildrenLoadStage);
            CreateIndex(sid, tableid, TableName, nameof(Indexes.ParentId), Indexes.ParentId);
            CreateIndex(sid, tableid, TableName, nameof(Indexes.QuotedPosts), Indexes.QuotedPosts);
            CreateIndex(sid, tableid, TableName, nameof(Indexes.Type), Indexes.Type);
            CreateIndex(sid, tableid, TableName, nameof(Indexes.TypeAndId), Indexes.TypeAndId);
            CreateIndex(sid, tableid, TableName, nameof(Indexes.TypeAndPostId), Indexes.TypeAndPostId);
            CreateIndex(sid, tableid, TableName, nameof(Indexes.InThreadPostLink), Indexes.InThreadPostLink);
        }

        /// <summary>
        /// Инициализировать основную таблицу.
        /// </summary>
        /// <param name="session">Сессия.</param>
        /// <param name="tableid">Идентификатор таблицы.</param>
        protected virtual void InitializeAccessLogTable(IEsentSession session, JET_TABLEID tableid)
        {
            var sid = session.Session;
            JET_COLUMNID tempcolid;
            Api.JetAddColumn(sid, tableid, AccessLogColumnNames.Id, new JET_COLUMNDEF()
            {
                coltyp = VistaColtyp.GUID,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, AccessLogColumnNames.EntityId, new JET_COLUMNDEF()
            {
                coltyp = VistaColtyp.GUID,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, AccessLogColumnNames.AccessTime, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.DateTime,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);

            var pkDef = $"+{AccessLogColumnNames.Id}\0\0";
            Api.JetCreateIndex(sid, tableid, AccessLogPkName, CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, pkDef, pkDef.Length, 100);
            CreateIndex(sid, tableid, AccessLogTableName, nameof(AccessLogIndexes.EntityId), AccessLogIndexes.EntityId);
            CreateIndex(sid, tableid, AccessLogTableName, nameof(AccessLogIndexes.AccessTime), AccessLogIndexes.AccessTime);
        }

        /// <summary>
        /// Инициализировать основную таблицу.
        /// </summary>
        /// <param name="session">Сессия.</param>
        /// <param name="tableid">Идентификатор таблицы.</param>
        protected virtual void InitializeMediaFilesTable(IEsentSession session, JET_TABLEID tableid)
        {
            var sid = session.Session;
            JET_COLUMNID tempcolid;
            Api.JetAddColumn(sid, tableid, MediaFilesColumnNames.Id, new JET_COLUMNDEF()
            {
                coltyp = VistaColtyp.GUID,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, MediaFilesColumnNames.EntityReferences, new JET_COLUMNDEF()
            {
                coltyp = VistaColtyp.GUID,
                grbit = ColumndefGrbit.ColumnMultiValued | ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, MediaFilesColumnNames.EntitySequenceNumber, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, MediaFilesColumnNames.MediaSequenceNumber, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, MediaFilesColumnNames.MediaData, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongBinary,
                grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);

            var pkDef = $"+{MediaFilesColumnNames.Id}\0\0";
            Api.JetCreateIndex(sid, tableid, MediaFilesPkName, CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, pkDef, pkDef.Length, 100);
            CreateIndex(sid, tableid, MediaFilesTableName, nameof(MediaFilesIndexes.EntityReferences), MediaFilesIndexes.EntityReferences);
            CreateIndex(sid, tableid, MediaFilesTableName, nameof(MediaFilesIndexes.Sequences), MediaFilesIndexes.Sequences);
        }
    }
}