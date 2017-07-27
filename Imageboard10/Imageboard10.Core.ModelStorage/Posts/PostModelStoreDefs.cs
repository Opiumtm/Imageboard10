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
        /// Имя таблицы лога доступа.
        /// </summary>
        protected string AccessLogTableName => $"pacclog_{EngineId}";

        /// <summary>
        /// Таблица медиафайлов.
        /// </summary>
        protected string MediaFilesTableName => $"postmedia_{EngineId}";

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
            PostsTable.CreateColumnsAndIndexes(session.Session, tableid);
        }

        /// <summary>
        /// Инициализировать основную таблицу.
        /// </summary>
        /// <param name="session">Сессия.</param>
        /// <param name="tableid">Идентификатор таблицы.</param>
        protected virtual void InitializeAccessLogTable(IEsentSession session, JET_TABLEID tableid)
        {
            AccessLogTable.CreateColumnsAndIndexes(session.Session, tableid);
        }

        /// <summary>
        /// Инициализировать основную таблицу.
        /// </summary>
        /// <param name="session">Сессия.</param>
        /// <param name="tableid">Идентификатор таблицы.</param>
        protected virtual void InitializeMediaFilesTable(IEsentSession session, JET_TABLEID tableid)
        {
            MediaFilesTable.CreateColumnsAndIndexes(session.Session, tableid);
        }

        private PostsTable OpenPostsTable(IEsentSession session, OpenTableGrbit grbit)
        {
            var r = session.OpenTable(TableName, grbit);
            return new PostsTable(r.Session, r.Table);
        }

        private AccessLogTable OpenAccessLogTable(IEsentSession session, OpenTableGrbit grbit)
        {
            var r = session.OpenTable(TableName, grbit);
            return new AccessLogTable(r.Session, r.Table);
        }

        private MediaFilesTable OpenMediaFilesTable(IEsentSession session, OpenTableGrbit grbit)
        {
            var r = session.OpenTable(TableName, grbit);
            return new MediaFilesTable(r.Session, r.Table);
        }
    }
}