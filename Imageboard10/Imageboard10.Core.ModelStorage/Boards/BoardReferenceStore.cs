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
using Imageboard10.Core.ModelStorage.UnitTests;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Utility;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.ModelStorage.Boards
{
    /// <summary>
    /// Базовая реализация хранилища ссылок на доски.
    /// </summary>
    public class BoardReferenceStore : ModelStorageBase<IBoardReferenceStore>, IBoardReferenceStore, IBoardReferenceStoreForTests, IStaticModuleQueryFilter
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
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        public override object QueryView(Type viewType)
        {
            if (viewType == typeof(IBoardReferenceStoreForTests))
            {
                return this;
            }
            return base.QueryView(viewType);
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
        /// <param name="sid">Сессия.</param>
        /// <param name="tableid">Идентификатор таблицы.</param>
        /// <param name="query">Запрос.</param>
        /// <returns>Результат позиционирования на первой записи индекса.</returns>
        protected virtual bool SelectIndex(Session sid, JET_TABLEID tableid, BoardReferenceStoreQuery query)
        {
            if (query.Category == null && query.IsAdult == null)
            {
                Api.JetSetTableSequential(sid, tableid, SetTableSequentialGrbit.None);
                return Api.TryMoveFirst(sid, tableid);
            }
            if (query.Category == null)
            {
                Api.JetSetCurrentIndex(sid, tableid, TableIsAdultIndexName);
                Api.MakeKey(sid, tableid, query.IsAdult.Value, MakeKeyGrbit.NewKey);
                return Api.TrySeek(sid, tableid, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
            }
            if (query.IsAdult == null)
            {
                Api.JetSetCurrentIndex(sid, tableid, TableCategoryIndexName);
                Api.MakeKey(sid, tableid, query.Category, Encoding.Unicode, MakeKeyGrbit.NewKey);
                return Api.TrySeek(sid, tableid, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
            }
            Api.JetSetCurrentIndex(sid, tableid, TableIsAdultAndCategoryIndexName);
            Api.MakeKey(sid, tableid, query.IsAdult.Value, MakeKeyGrbit.NewKey);
            Api.MakeKey(sid, tableid, query.Category, Encoding.Unicode, MakeKeyGrbit.None);
            return Api.TrySeek(sid, tableid, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
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
        protected virtual async Task<int> DoGetCount(BoardReferenceStoreQuery query)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(session =>
            {
                var sid = session.Session;
                using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                {
                    if (!SelectIndex(sid, table, query))
                    {
                        return 0;
                    }
                    int count;
                    Api.JetIndexRecordCount(sid, table, out count, int.MaxValue);
                    return count;
                }
            });
        }

        /// <summary>
        /// Получить количество категорий.
        /// </summary>
        /// <returns>Количество категорий.</returns>
        public IAsyncOperation<int> GetCategoryCount()
        {
            return DoGetCategoryCount(null).AsAsyncOperation();
        }

        /// <summary>
        /// Получить количество категорий.
        /// </summary>
        /// <param name="isAdult">Только для взрослы. null = не имеет значения.</param>
        /// <returns>Количество категорий.</returns>
        public IAsyncOperation<int> GetCategoryCount(bool? isAdult)
        {
            return DoGetCategoryCount(isAdult).AsAsyncOperation();
        }

        /// <summary>
        /// Получить количество категорий.
        /// </summary>
        /// <param name="isAdult">Только для взрослы. null = не имеет значения.</param>
        /// <returns>Количество категорий.</returns>
        protected virtual async Task<int> DoGetCategoryCount(bool? isAdult)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        int count = 0;
                        if (isAdult == null)
                        {
                            Api.JetSetCurrentIndex(sid, table, TableCategoryIndexName);
                            if (Api.TryMoveFirst(sid, table))
                            {
                                do
                                {
                                    count++;
                                } while (Api.TryMove(sid, table, JET_Move.Next, MoveGrbit.MoveKeyNE));
                            }
                        }
                        else
                        {
                            var acolid = Api.GetTableColumnid(sid, table, ColumnNames.IsAdult);
                            Api.JetSetCurrentIndex(sid, table, TableIsAdultAndCategoryIndexName);
                            if (Api.TryMoveFirst(sid, table))
                            {
                                do
                                {
                                    var a = Api.RetrieveColumnAsBoolean(sid, table, acolid, RetrieveColumnGrbit.RetrieveFromIndex);
                                    if (a == isAdult.Value)
                                    {
                                        count++;
                                    }
                                } while (Api.TryMove(sid, table, JET_Move.Next, MoveGrbit.MoveKeyNE));
                            }
                        }
                        return count;
                    }
                }
            });
        }

        /// <summary>
        /// Получить все ссылки на доски.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Ссылки на доски.</returns>
        public IAsyncOperation<IList<ILink>> GetBoardLinks(BoardReferenceStoreQuery query)
        {
            return DoGetBoardLinks(query).AsAsyncOperation();
        }

        /// <summary>
        /// Получить все ссылки на доски.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Ссылки на доски.</returns>
        protected virtual async Task<IList<ILink>> DoGetBoardLinks(BoardReferenceStoreQuery query)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    using (var tableid = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var idSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var colid = Api.GetTableColumnid(sid, tableid, ColumnNames.Id);
                        if (SelectIndex(sid, tableid, query))
                        {
                            do
                            {
                                idSet.Add(Api.RetrieveColumnAsString(sid, tableid, colid, Encoding.Unicode));
                            } while (Api.TryMoveNext(sid, tableid));
                        }
                        IList<ILink> result = idSet.Select(CreateBoardLink).ToList();
                        return result;
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
            return DoGetAllCategories(null).AsAsyncOperation();
        }

        /// <summary>
        /// Получить все категории.
        /// </summary>
        /// <param name="isAdult">Только для взрослых. null = не имеет значения.</param>
        /// <returns>Все категории.</returns>
        public IAsyncOperation<IList<string>> GetAllCategories(bool? isAdult)
        {
            return DoGetAllCategories(isAdult).AsAsyncOperation();
        }

        /// <summary>
        /// Получить все категории.
        /// </summary>
        /// <param name="isAdult">Только для взрослых. null = не имеет значения.</param>
        /// <returns>Все категории.</returns>
        protected virtual async Task<IList<string>> DoGetAllCategories(bool? isAdult)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    using (var tableid = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var colid = Api.GetTableColumnid(sid, tableid, ColumnNames.Category);
                        HashSet<string> categories = new HashSet<string>();
                        if (isAdult == null)
                        {
                            Api.JetSetCurrentIndex(sid, tableid, TableCategoryIndexName);
                            if (Api.TryMoveFirst(sid, tableid))
                            {
                                do
                                {
                                    var c = Api.RetrieveColumnAsString(sid, tableid, colid, Encoding.Unicode, RetrieveColumnGrbit.RetrieveFromIndex);
                                    categories.Add(c);
                                } while (Api.TryMove(sid, tableid, JET_Move.Next, MoveGrbit.MoveKeyNE));
                            }
                        }
                        else
                        {
                            Api.JetSetCurrentIndex(sid, tableid, TableIsAdultAndCategoryIndexName);
                            var acolid = Api.GetTableColumnid(sid, tableid, ColumnNames.IsAdult);
                            if (Api.TryMoveFirst(sid, tableid))
                            {
                                do
                                {
                                    var a = Api.RetrieveColumnAsBoolean(sid, tableid, acolid, RetrieveColumnGrbit.RetrieveFromIndex);
                                    if (a == isAdult.Value)
                                    {
                                        var c = Api.RetrieveColumnAsString(sid, tableid, colid, Encoding.Unicode, RetrieveColumnGrbit.RetrieveFromIndex);
                                        categories.Add(c);
                                    }
                                } while (Api.TryMove(sid, tableid, JET_Move.Next, MoveGrbit.MoveKeyNE));
                            }
                        }
                        IList<string> result = categories.ToList();
                        return result;
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
        protected virtual async Task<IBoardReference> DoLoadReference(ILink link)
        {
            CheckModuleReady();
            if (link == null)
            {
                const IBoardReference result = null;
                return result;
            }
            await WaitForTablesInitialize();
            return await QueryReadonly(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    using (var tableid = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
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
                }
            });
        }

        /// <summary>
        /// Прочитать короткую информацию из текущей записи.
        /// </summary>
        /// <param name="sid">Сессия.</param>
        /// <param name="tableid">Таблица.</param>
        /// <param name="columnmap">Карта столбцов.</param>
        /// <returns>Короткая информация.</returns>
        protected virtual IBoardShortInfo ReadShortInfo(Session sid, JET_TABLEID tableid, IDictionary<string, JET_COLUMNID> columnmap)
        {
            var columns = new ColumnValue[]
            {
                // 0
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.Id],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 1
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.Category],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 2
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.ShortName],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 3
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.DisplayName],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 4
                new BoolColumnValue()
                {
                    Columnid = columnmap[ColumnNames.IsAdult],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
            };
            Api.RetrieveColumns(sid, tableid, columns);
            return new BoardShortInfo()
            {
                BoardLink = CreateBoardLink(((StringColumnValue)columns[0]).Value),
                Category = ((StringColumnValue)columns[1]).Value,
                ShortName = ((StringColumnValue)columns[2]).Value,
                DisplayName = ((StringColumnValue)columns[3]).Value,
                IsAdult = ((BoolColumnValue)columns[4]).Value ?? false
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
        /// <param name="columnmap">Карта столбцов.</param>
        /// <returns>Короткая информация.</returns>
        protected virtual IBoardReference ReadFullRow(Session sid, JET_TABLEID tableid, IDictionary<string, JET_COLUMNID> columnmap)
        {
            var columns = new ColumnValue[]
            {
                // 0
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.Id],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 1
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.Category],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 2
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.ShortName],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 3
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.DisplayName],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 4
                new BoolColumnValue()
                {
                    Columnid = columnmap[ColumnNames.IsAdult],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 5
                new BytesColumnValue()
                {
                    Columnid = columnmap[ColumnNames.ExtendedData],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 6
                new Int32ColumnValue()
                {
                    Columnid = columnmap[ColumnNames.BumpLimit],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 7
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.DefaultName],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
                // 8
                new Int32ColumnValue()
                {
                    Columnid = columnmap[ColumnNames.Pages],
                    RetrieveGrbit = RetrieveColumnGrbit.None
                },
            };
            Api.RetrieveColumns(sid, tableid, columns);
            var result = CreateBoardReferenceObject();
            result.BoardLink = CreateBoardLink(((StringColumnValue)columns[0]).Value);
            result.Category = ((StringColumnValue)columns[1]).Value;
            result.ShortName = ((StringColumnValue) columns[2]).Value;
            result.DisplayName = ((StringColumnValue) columns[3]).Value;
            result.IsAdult = ((BoolColumnValue) columns[4]).Value ?? false;
            SetExtendedInfo(((BytesColumnValue) columns[5]).Value, result);
            result.BumpLimit = ((Int32ColumnValue) columns[6]).Value;
            result.DefaultName = ((StringColumnValue) columns[7]).Value;
            result.Pages = ((Int32ColumnValue) columns[8]).Value;
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
            return DoLoadShortReferences(start, count, query).AsAsyncOperation();
        }

        /// <summary>
        /// Загрузить ссылки.
        /// </summary>
        /// <param name="start">Начало.</param>
        /// <param name="count">Количество.</param>
        /// <param name="query">Запрос.</param>
        /// <returns>Ссылки.</returns>
        protected async virtual Task<IList<IBoardShortInfo>> DoLoadShortReferences(int start, int count, BoardReferenceStoreQuery query)
        {
            CheckModuleReady();
            if (count < 1)
            {
                IList<IBoardShortInfo> result = new List<IBoardShortInfo>();
                return result;
            }
            await WaitForTablesInitialize();
            return await QueryReadonly(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    using (var tableid = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var columnMap = Api.GetColumnDictionary(sid, tableid);
                        IList<IBoardShortInfo> result = new List<IBoardShortInfo>();
                        bool isMoved = SelectIndex(sid, tableid, query);
                        if (isMoved)
                        {
                            if (start > 0)
                            {
                                isMoved = Api.TryMove(sid, tableid, (JET_Move)start, MoveGrbit.None);
                            }
                        }
                        if (isMoved)
                        {
                            int cnt = count;
                            do
                            {
                                cnt--;
                                result.Add(ReadShortInfo(sid, tableid, columnMap));
                            } while (Api.TryMoveNext(sid, tableid) && cnt > 0);
                        }
                        return result;
                    }
                }
            });
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
        protected virtual async Task<IList<IBoardShortInfo>> DoLoadShortReferences(IList<ILink> links)
        {
            CheckModuleReady();
            if (links == null || links.Count == 0)
            {
                IList<IBoardShortInfo> result = new List<IBoardShortInfo>();
                return result;
            }
            await WaitForTablesInitialize();
            return await QueryReadonly(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    using (var tableid = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
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
                }
            });
        }

        /// <summary>
        /// Очистить всю информацию.
        /// </summary>
        public IAsyncAction Clear()
        {
            return DoClear().AsAsyncAction();
        }

        /// <summary>
        /// Очистить всю информацию.
        /// </summary>
        protected virtual async Task DoClear()
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            await UpdateAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                    {
                        DeleteAllRows(table);
                    }
                    return true;
                });
                return Nothing.Value;
            });
        }

        /// <summary>
        /// Обновить данные в текущей строке таблицы.
        /// </summary>
        /// <param name="table">Таблица.</param>
        /// <param name="reference">Ссылка на доску.</param>
        /// <param name="columnmap">Карта столбцов.</param>
        protected virtual void UpdateFullRowInfo(EsentTable table, IBoardReference reference, IDictionary<string, JET_COLUMNID> columnmap)
        {
            var columns = new ColumnValue[]
            {
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.Id],
                    Value = GetId(reference),
                    SetGrbit = SetColumnGrbit.None                    
                },
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.Category],
                    Value = reference.Category ?? "",
                    SetGrbit = SetColumnGrbit.None
                },
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.ShortName],
                    Value = reference.ShortName,
                    SetGrbit = SetColumnGrbit.None
                },
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.DisplayName],
                    Value = reference.DisplayName,
                    SetGrbit = SetColumnGrbit.None
                },
                new BoolColumnValue()
                {
                    Columnid = columnmap[ColumnNames.IsAdult],
                    Value = reference.IsAdult,
                    SetGrbit = SetColumnGrbit.None
                },
                new BytesColumnValue()
                {
                    Columnid = columnmap[ColumnNames.ExtendedData],
                    Value = SerializeDataContract(BoardExtendedInfo.ToContract(reference, LinkSerialization)),
                    SetGrbit = SetColumnGrbit.None
                },
                new Int32ColumnValue()
                {
                    Columnid = columnmap[ColumnNames.BumpLimit],
                    Value = reference.BumpLimit,
                    SetGrbit = SetColumnGrbit.None
                },
                new StringColumnValue()
                {
                    Columnid = columnmap[ColumnNames.DefaultName],
                    Value = reference.DefaultName,
                    SetGrbit = SetColumnGrbit.None
                },
                new Int32ColumnValue()
                {
                    Columnid = columnmap[ColumnNames.Pages],
                    Value = reference.Pages,
                    SetGrbit = SetColumnGrbit.None
                },
            };
            Api.SetColumns(table.Session, table, columns);
        }

        /// <summary>
        /// Обновить ссылку.
        /// </summary>
        /// <param name="reference">Ссылка.</param>
        public IAsyncAction UpdateReference(IBoardReference reference)
        {
            return DoUpdateReference(reference).AsAsyncAction();
        }

        /// <summary>
        /// Обновить ссылку.
        /// </summary>
        /// <param name="reference">Ссылка.</param>
        protected virtual async Task DoUpdateReference(IBoardReference reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            CheckModuleReady();
            await WaitForTablesInitialize();
            await UpdateAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                    {
                        DoUpdateOneRow(table, reference, false);
                    }
                    return true;
                });
                return Nothing.Value;
            });
        }


        /// <summary>
        /// Обновить одну строку.
        /// </summary>
        /// <param name="table">Таблица.</param>
        /// <param name="reference">Ссылка.</param>
        /// <param name="alwaysInsert">Всегда вставлять (после очистки таблицы).</param>
        protected virtual void DoUpdateOneRow(EsentTable table, IBoardReference reference, bool alwaysInsert)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (!alwaysInsert)
            {
                var id = GetId(reference);
                Api.MakeKey(table.Session, table, id, Encoding.Unicode, MakeKeyGrbit.NewKey);
                Update rowUpdate;
                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                {
                    rowUpdate = new Update(table.Session, table, JET_prep.Replace);
                }
                else
                {
                    rowUpdate = new Update(table.Session, table, JET_prep.Insert);
                }
                using (rowUpdate)
                {
                    UpdateFullRowInfo(table, reference, Api.GetColumnDictionary(table.Session, table));
                    rowUpdate.Save();
                }
            }
            else
            {
                using (var rowUpdate = new Update(table.Session, table, JET_prep.Insert))
                {
                    UpdateFullRowInfo(table, reference, Api.GetColumnDictionary(table.Session, table));
                    rowUpdate.Save();
                }
            }
        }

        /// <summary>
        /// Обновить ссылки.
        /// </summary>
        /// <param name="references">Ссылки.</param>
        /// <param name="clearPrevious">Очистить предыдущие.</param>
        public IAsyncAction UpdateReferences(IList<IBoardReference> references, bool clearPrevious)
        {
            return DoUpdateReferences(references, clearPrevious).AsAsyncAction();
        }

        /// <summary>
        /// Обновить ссылки.
        /// </summary>
        /// <param name="references">Ссылки.</param>
        /// <param name="clearPrevious">Очистить предыдущие.</param>
        protected virtual async Task DoUpdateReferences(IList<IBoardReference> references, bool clearPrevious)
        {
            CheckModuleReady();
            if (references == null || references.Count == 0)
            {
                return;
            }
            await WaitForTablesInitialize();
            await UpdateAsync(async session =>
            {
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                    {
                        if (clearPrevious)
                        {
                            using (var transaction = new Transaction(table.Session))
                            {
                                DeleteAllRows(table);
                                transaction.Commit(CommitTransactionGrbit.None);
                            }
                        }
                        foreach (var references1 in references.Where(r => r != null).SplitSet(5))
                        {
                            using (var transaction = new Transaction(table.Session))
                            {
                                foreach (var reference in references1)
                                {
                                    DoUpdateOneRow(table, reference, clearPrevious);
                                }
                                transaction.Commit(CommitTransactionGrbit.None);
                            }
                        }
                    }
                });
                return Nothing.Value;
            });

        }

        /// <summary>
        /// Имя таблицы с досками.
        /// </summary>
        string IBoardReferenceStoreForTests.BoardsTableName => TableName;

        /// <summary>
        /// Проверить запрос.
        /// </summary>
        /// <typeparam name="T">Тип запроса.</typeparam>
        /// <param name="query">Запрос.</param>
        /// <returns>Результат.</returns>
        bool IStaticModuleQueryFilter.CheckQuery<T>(T query)
        {
            if (typeof(T) == typeof(string))
            {
                var s = (string) (object) query;
                if (EngineId.Equals(s, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}