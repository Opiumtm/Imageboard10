using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Models.Boards;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.ModelStorage.Boards.DataContracts;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.ModelStorage.Boards
{
    /// <summary>
    /// Базовая реализация хранилища ссылок на доски.
    /// </summary>
    public class BoardReferenceStore : ModelStorageBase<IBoardReferenceStore>, IBoardReferenceStore
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="engineId">Идентификатор движка.</param>
        public BoardReferenceStore(string engineId)
        {
            EngineId = engineId ?? throw new ArgumentNullException(nameof(engineId));
        }

        /// <summary>
        /// Идентификатор движка.
        /// </summary>
        protected string EngineId { get; }

        /// <summary>
        /// Имя таблицы.
        /// </summary>
        protected string TableName => $"boardref_{EngineId}";

        /// <summary>
        /// Первичный индекс таблицы.
        /// </summary>
        protected string TablePkName => $"PK_{TableName}";

        /// <summary>
        /// Индекс таблицы по категориями.
        /// </summary>
        protected string TableCategoryIndexName => $"IX_{TableName}_Category";

        /// <summary>
        /// Индекс таблицы по взрослым доскам.
        /// </summary>
        protected string TableIsAdultIndexName => $"IX_{TableName}_IsAdult";

        /// <summary>
        /// Индекс таблицы по взрослым доскам + категории.
        /// </summary>
        protected string TableIsAdultAndCategoryIndexName => $"IX_{TableName}_Q1";

        /// <summary>
        /// Создать или обновить таблицы.
        /// </summary>
        protected override async ValueTask<Nothing> CreateOrUpgradeTables()
        {
            await EnsureTable(TableName, 1, InitializeTable, null, true);
            return Nothing.Value;
        }

        /// <summary>
        /// Имена столбцов.
        /// </summary>
        protected static class ColumnNames
        {
            public const string Id = "Id";
            public const string Category = "Category";
            public const string ShortName = "ShortName";
            public const string DisplayName = "DisplayName";
            public const string IsAdult = "IsAdult";
            public const string ExtendedData = "ExtendedData";
            public const string BumpLimit = "BumpLimit";
            public const string DefaultName = "DefaultName";
            public const string Pages = "Pages";
        }

        /// <summary>
        /// Инициализировать таблицу.
        /// </summary>
        /// <param name="session">Сессия.</param>
        /// <param name="tableid">Идентификатор таблицы.</param>
        protected virtual void InitializeTable(IEsentSession session, JET_TABLEID tableid)
        {
            var sid = session.Session;
            JET_COLUMNID tempcolid;
            Api.JetAddColumn(sid, tableid, ColumnNames.Id, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnNotNULL,
                cp = JET_CP.Unicode
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.Category, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnNotNULL,
                cp = JET_CP.Unicode
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.ShortName, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnMaybeNull,
                cp = JET_CP.Unicode
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.DisplayName, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnMaybeNull,
                cp = JET_CP.Unicode
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.IsAdult, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Bit,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.ExtendedData, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongBinary,
                grbit = ColumndefGrbit.ColumnMaybeNull,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.BumpLimit, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnMaybeNull,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.DefaultName, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnMaybeNull,
            }, null, 0, out tempcolid);
            Api.JetAddColumn(sid, tableid, ColumnNames.Pages, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnMaybeNull,
            }, null, 0, out tempcolid);

            var pkDef = $"+{ColumnNames.Id}\0\0";
            var catDef = $"+{ColumnNames.Category}\0\0";
            var adDef = $"+{ColumnNames.IsAdult}\0\0";
            var q1Def = $"+{ColumnNames.IsAdult}\0+{ColumnNames.Category}\0\0";

            Api.JetCreateIndex(sid, tableid, TablePkName, CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, pkDef, pkDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, TableCategoryIndexName, CreateIndexGrbit.None, catDef, catDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, TableIsAdultIndexName, CreateIndexGrbit.None, adDef, adDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, TableIsAdultAndCategoryIndexName, CreateIndexGrbit.None, q1Def, q1Def.Length, 100);
        }

        /// <summary>
        /// Получить идентификатор.
        /// </summary>
        /// <param name="boardReference">Ссылка.</param>
        /// <returns>Идентификатор.</returns>
        protected virtual string GetId(IBoardReference boardReference)
        {
            return boardReference?.ShortName?.ToLowerInvariant() ?? "";
        }

        /// <summary>
        /// Получить идентификатор.
        /// </summary>
        /// <param name="boardLink">Ссылка на борду.</param>
        /// <returns>Идентификатор.</returns>
        protected virtual string GetId(ILink boardLink)
        {
            if (boardLink is BoardLink l)
            {
                return l.Board?.ToLowerInvariant() ?? "";
            }
            return "";
        }

        /// <summary>
        /// Создать ссылку на борду.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Ссылка на борду.</returns>
        protected virtual ILink CreateBoardLink(string id)
        {
            return new BoardLink()
            {
                Engine = EngineId,
                Board = id
            };
        }

        /// <summary>
        /// Выбрать индекс и включить фильтр в зависимости от запроса.
        /// </summary>
        /// <param name="session">Сессия.</param>
        /// <param name="tableid">Идентификатор таблицы.</param>
        /// <param name="query">Запрос.</param>
        /// <returns>Результат позиционирования на первой записи индекса.</returns>
        protected virtual bool SelectIndex(IEsentSession session, JET_TABLEID tableid, BoardReferenceStoreQuery query)
        {
            var sid = session.Session;
            if (query.Category == null && query.IsAdult == null)
            {
                Api.JetSetTableSequential(sid, tableid, SetTableSequentialGrbit.None);
                return Api.TryMoveFirst(sid, tableid);
            }
            if (query.Category == null)
            {
                Api.JetSetCurrentIndex(sid, tableid, TableIsAdultIndexName);
                Api.MakeKey(sid, tableid, query.IsAdult.Value, MakeKeyGrbit.NewKey);
                return Api.TrySeek(sid, tableid, SeekGrbit.SeekEQ);
            }
            if (query.IsAdult == null)
            {
                Api.JetSetCurrentIndex(sid, tableid, TableCategoryIndexName);
                Api.MakeKey(sid, tableid, query.Category, Encoding.Unicode, MakeKeyGrbit.NewKey);
                return Api.TrySeek(sid, tableid, SeekGrbit.SeekEQ);
            }
            Api.JetSetCurrentIndex(sid, tableid, TableIsAdultAndCategoryIndexName);
            Api.MakeKey(sid, tableid, query.IsAdult.Value, MakeKeyGrbit.NewKey);
            Api.MakeKey(sid, tableid, query.Category, Encoding.Unicode, MakeKeyGrbit.None);
            return Api.TrySeek(sid, tableid, SeekGrbit.SeekEQ);
        }

        /// <summary>
        /// Открыть таблицу только для чтения.
        /// </summary>
        /// <param name="session">Сессия.</param>
        /// <returns>Идентификатор таблицы.</returns>
        protected JET_TABLEID OpenTableReadOnly(IEsentSession session)
        {
            var sid = session.Session;
            var dbid = session.Database;
            JET_TABLEID tableid;
            Api.OpenTable(sid, dbid, TableName, OpenTableGrbit.ReadOnly, out tableid);
            return tableid;
        }

        /// <summary>
        /// Получить количество досок.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Количество досок.</returns>
        public IAsyncOperation<int> GetCount(BoardReferenceStoreQuery query)
        {
            return DoGetCount(query).AsAsyncOperation();
        }

        /// <summary>
        /// Получить количество досок.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Количество досок.</returns>
        protected virtual Task<int> DoGetCount(BoardReferenceStoreQuery query)
        {
            return QueryReadonlyThreadUnsafeAsync(session =>
            {
                var sid = session.Session;
                var tableid = OpenTableReadOnly(session);
                try
                {
                    if (!SelectIndex(session, tableid, query))
                    {
                        return 0;
                    }
                    int count;
                    Api.JetIndexRecordCount(sid, tableid, out count, int.MaxValue);
                    return count;
                }
                finally
                {
                    Api.JetCloseTable(sid, tableid);
                }
            });
        }

        /// <summary>
        /// Получить количество категорий.
        /// </summary>
        /// <returns>Количество категорий.</returns>
        public IAsyncOperation<int> GetCategoryCount()
        {
            return DoGetCategoryCount().AsAsyncOperation();
        }

        /// <summary>
        /// Получить количество категорий.
        /// </summary>
        /// <returns>Количество категорий.</returns>
        protected virtual Task<int> DoGetCategoryCount()
        {
            return QueryReadonlyThreadUnsafeAsync(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    var tableid = OpenTableReadOnly(session);
                    try
                    {
                        Api.JetSetCurrentIndex(sid, tableid, TableCategoryIndexName);
                        var colid = Api.GetTableColumnid(sid, tableid, ColumnNames.Category);
                        HashSet<string> categories = new HashSet<string>();
                        if (Api.TryMoveFirst(sid, tableid))
                        {
                            do
                            {
                                var c = Api.RetrieveColumnAsString(sid, tableid, colid, Encoding.Unicode, RetrieveColumnGrbit.RetrieveFromIndex);
                                categories.Add(c);
                            } while (Api.TryMoveNext(sid, tableid));
                        }
                        return categories.Count;
                    }
                    finally
                    {
                        Api.JetCloseTable(sid, tableid);
                    }
                }
            });
        }

        /// <summary>
        /// Получить все ссылки на доски.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Ссылки на доски.</returns>
        public IAsyncOperation<IList<ILink>> GetBoardLiks(BoardReferenceStoreQuery query)
        {
            return DoGetBoardLinks(query).AsAsyncOperation();
        }

        /// <summary>
        /// Получить все ссылки на доски.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Ссылки на доски.</returns>
        protected virtual Task<IList<ILink>> DoGetBoardLinks(BoardReferenceStoreQuery query)
        {
            return QueryReadonlyThreadUnsafeAsync(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    var tableid = OpenTableReadOnly(session);
                    try
                    {
                        var idSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var colid = Api.GetTableColumnid(sid, tableid, ColumnNames.Id);
                        if (SelectIndex(session, tableid, query))
                        {
                            do
                            {
                                idSet.Add(Api.RetrieveColumnAsString(sid, tableid, colid, Encoding.Unicode));
                            } while (Api.TryMoveNext(sid, tableid));
                        }
                        IList<ILink> result = idSet.Select(CreateBoardLink).ToList();
                        return result;
                    }
                    finally
                    {
                        Api.JetCloseTable(sid, tableid);
                    }
                }
            });
        }

        /// <summary>
        /// Получить все категории.
        /// </summary>
        /// <returns>Все категории.</returns>
        public IAsyncOperation<IList<string>> GetAllCategories()
        {
            return DoGetAllCategories().AsAsyncOperation();
        }

        /// <summary>
        /// Получить все категории.
        /// </summary>
        /// <returns>Все категории.</returns>
        protected virtual Task<IList<string>> DoGetAllCategories()
        {
            return QueryReadonlyThreadUnsafeAsync(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    var tableid = OpenTableReadOnly(session);
                    try
                    {
                        Api.JetSetCurrentIndex(sid, tableid, TableCategoryIndexName);
                        var colid = Api.GetTableColumnid(sid, tableid, ColumnNames.Category);
                        HashSet<string> categories = new HashSet<string>();
                        if (Api.TryMoveFirst(sid, tableid))
                        {
                            do
                            {
                                var c = Api.RetrieveColumnAsString(sid, tableid, colid, Encoding.Unicode, RetrieveColumnGrbit.RetrieveFromIndex);
                                categories.Add(c);
                            } while (Api.TryMoveNext(sid, tableid));
                        }
                        IList<string> result = categories.ToList();
                        return result;
                    }
                    finally
                    {
                        Api.JetCloseTable(sid, tableid);
                    }
                }
            });

        }

        /// <summary>
        /// Загрузить ссылку на доску.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Ссылка на доску.</returns>
        public IAsyncOperation<IBoardReference> LoadReference(ILink link)
        {
            return DoLoadReference(link).AsAsyncOperation();
        }

        /// <summary>
        /// Загрузить ссылку на доску.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Ссылка на доску.</returns>
        protected Task<IBoardReference> DoLoadReference(ILink link)
        {
            if (link == null)
            {
                const IBoardReference result = null;
                return Task.FromResult(result);
            }
            return QueryReadonlyThreadUnsafeAsync(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    var tableid = OpenTableReadOnly(session);
                    try
                    {
                        var columnMap = Api.GetColumnDictionary(sid, tableid);
                        var id = GetId(link);
                        IBoardReference result = null;
                        Api.MakeKey(sid, tableid, id, Encoding.Unicode, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(sid, tableid, SeekGrbit.SeekEQ))
                        {
                            result = ReadFullRow(sid, tableid, columnMap);
                        }
                        return result;
                    }
                    finally
                    {
                        Api.JetCloseTable(sid, tableid);
                    }
                }
            });
        }

        /// <summary>
        /// Прочитать короткую информацию из текущей записи.
        /// </summary>
        /// <param name="sid">Сессия.</param>
        /// <param name="tableid">Таблица.</param>
        /// <param name="columnMap">Карта столбцов.</param>
        /// <returns>Короткая информация.</returns>
        protected virtual IBoardShortInfo ReadShortInfo(Session sid, JET_TABLEID tableid, IDictionary<string, JET_COLUMNID> columnMap)
        {
            return new BoardShortInfo()
            {
                BoardLink = CreateBoardLink(Api.RetrieveColumnAsString(sid, tableid, columnMap[ColumnNames.Id], Encoding.Unicode)),
                ShortName = Api.RetrieveColumnAsString(sid, tableid, columnMap[ColumnNames.ShortName], Encoding.Unicode),
                Category = Api.RetrieveColumnAsString(sid, tableid, columnMap[ColumnNames.Category], Encoding.Unicode),
                DisplayName = Api.RetrieveColumnAsString(sid, tableid, columnMap[ColumnNames.DisplayName], Encoding.Unicode),
                IsAdult = Api.RetrieveColumnAsBoolean(sid, tableid, columnMap[ColumnNames.IsAdult]) ?? false
            };
        }

        /// <summary>
        /// Создать объект для полной информации о доске.
        /// </summary>
        /// <returns>Объект.</returns>
        protected virtual BoardReference CreateBoardReferenceObject()
        {
            return new BoardReference();
        }

        /// <summary>
        /// Прочитать полную информацию из текущей записи.
        /// </summary>
        /// <param name="sid">Сессия.</param>
        /// <param name="tableid">Таблица.</param>
        /// <param name="columnMap">Карта столбцов.</param>
        /// <returns>Короткая информация.</returns>
        protected virtual IBoardReference ReadFullRow(Session sid, JET_TABLEID tableid, IDictionary<string, JET_COLUMNID> columnMap)
        {
            var result = CreateBoardReferenceObject();
            result.BoardLink = CreateBoardLink(Api.RetrieveColumnAsString(sid, tableid, columnMap[ColumnNames.Id], Encoding.Unicode));
            result.ShortName = Api.RetrieveColumnAsString(sid, tableid, columnMap[ColumnNames.ShortName], Encoding.Unicode);
            result.Category = Api.RetrieveColumnAsString(sid, tableid, columnMap[ColumnNames.Category], Encoding.Unicode);
            result.DisplayName = Api.RetrieveColumnAsString(sid, tableid, columnMap[ColumnNames.DisplayName], Encoding.Unicode);
            result.IsAdult = Api.RetrieveColumnAsBoolean(sid, tableid, columnMap[ColumnNames.IsAdult]) ?? false;
            result.BumpLimit = Api.RetrieveColumnAsInt32(sid, tableid, columnMap[ColumnNames.BumpLimit]);
            result.DefaultName = Api.RetrieveColumnAsString(sid, tableid, columnMap[ColumnNames.DefaultName], Encoding.Unicode);
            result.Pages = Api.RetrieveColumnAsInt32(sid, tableid, columnMap[ColumnNames.Pages]);
            byte[] extended;
            var extendedSize = Api.RetrieveColumnSize(sid, tableid, columnMap[ColumnNames.ExtendedData]);
            if (extendedSize == null)
            {
                extended = null;
            }
            else
            {
                extended = new byte[extendedSize.Value];
            }
            SetExtendedInfo(extended, result);
            return result;
        }

        /// <summary>
        /// Десериализовать дополнительную ифнмормацию.
        /// </summary>
        /// <param name="extended">Дополнительная информация.</param>
        /// <param name="result">Результат.</param>
        protected virtual void SetExtendedInfo(byte[] extended, BoardReference result)
        {
            BoardExtendedInfo.SetExtendedInfoFor(DeserializeDataContract<BoardExtendedInfo>(extended), result, LinkSerialization);
        }

        /// <summary>
        /// Загрузить ссылки.
        /// </summary>
        /// <param name="start">Начало.</param>
        /// <param name="count">Количество.</param>
        /// <param name="query">Запрос.</param>
        /// <returns>Ссылки.</returns>
        public IAsyncOperation<IList<IBoardShortInfo>> LoadShortReferences(int start, int count, BoardReferenceStoreQuery query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Загрузить ссылки.
        /// </summary>
        /// <param name="links">Список ссылок.</param>
        /// <returns>Ссылки на доски.</returns>
        public IAsyncOperation<IList<IBoardShortInfo>> LoadShortReferences(IList<ILink> links)
        {
            return DoLoadShortReferences(links).AsAsyncOperation();
        }

        /// <summary>
        /// Загрузить ссылки.
        /// </summary>
        /// <param name="links">Список ссылок.</param>
        /// <returns>Ссылки на доски.</returns>
        protected virtual Task<IList<IBoardShortInfo>> DoLoadShortReferences(IList<ILink> links)
        {
            if (links == null || links.Count == 0)
            {
                IList<IBoardShortInfo> result = new List<IBoardShortInfo>();
                return Task.FromResult(result);
            }
            return QueryReadonlyThreadUnsafeAsync(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    var tableid = OpenTableReadOnly(session);
                    try
                    {
                        var columnMap = Api.GetColumnDictionary(sid, tableid);
                        var ids = links.Select(GetId).Distinct().OrderBy(l => l).ToArray();
                        IList<IBoardShortInfo> result = new List<IBoardShortInfo>();
                        foreach (var id in ids)
                        {
                            Api.MakeKey(sid, tableid, id, Encoding.Unicode, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(sid, tableid, SeekGrbit.SeekEQ))
                            {
                                result.Add(ReadShortInfo(sid, tableid, columnMap));
                            }
                        }
                        return result;
                    }
                    finally
                    {
                        Api.JetCloseTable(sid, tableid);
                    }
                }
            });
        }

        public IAsyncAction Clear()
        {
            throw new NotImplementedException();
        }

        public IAsyncAction UpdateReference(IBoardReference reference)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction UpdateReferences(IList<IBoardReference> references, bool clearPrevious)
        {
            throw new NotImplementedException();
        }
    }
}