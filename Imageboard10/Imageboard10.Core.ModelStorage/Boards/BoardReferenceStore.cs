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

        protected BoardReferenceTable OpenTable(IEsentSession session, OpenTableGrbit grbit)
        {
            var table = session.OpenTable(TableName, grbit);
            return new BoardReferenceTable(table.Session, table);
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
        /// Создать или обновить таблицы.
        /// </summary>
        protected override async ValueTask<Nothing> CreateOrUpgradeTables()
        {
            await EnsureTable(TableName, 1, InitializeTable, null, true);
            return Nothing.Value;
        }

        /// <summary>
        /// Инициализировать таблицу.
        /// </summary>
        /// <param name="session">Сессия.</param>
        /// <param name="tableid">Идентификатор таблицы.</param>
        protected virtual void InitializeTable(IEsentSession session, JET_TABLEID tableid)
        {
            BoardReferenceTable.CreateColumnsAndIndexes(session.Session, tableid);
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
        /// <param name="table">Таблица.</param>
        /// <param name="query">Запрос.</param>
        /// <returns>Результат позиционирования на первой записи индекса.</returns>
        protected virtual bool SelectIndex(BoardReferenceTable table, BoardReferenceStoreQuery query)
        {
            if (query.Category == null && query.IsAdult == null)
            {
                Api.JetSetTableSequential(table.Session, table, SetTableSequentialGrbit.None);
                return table.TryMoveFirst();
            }
            if (query.Category == null)
            {
                table.Indexes.IsAdultIndex.SetAsCurrentIndex();
                table.Indexes.IsAdultIndex.SetKey(table.Indexes.IsAdultIndex.CreateKey(query.IsAdult.Value));
                return Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
            }
            if (query.IsAdult == null)
            {
                table.Indexes.CategoryIndex.SetAsCurrentIndex();
                table.Indexes.CategoryIndex.SetKey(table.Indexes.CategoryIndex.CreateKey(query.Category));
                return Api.TrySeek(table.Session, table.Table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
            }
            table.Indexes.IsAdultAndCategoryIndex.SetAsCurrentIndex();
            table.Indexes.IsAdultAndCategoryIndex.SetKey(table.Indexes.IsAdultAndCategoryIndex.CreateKey(query.IsAdult.Value, query.Category));
            return Api.TrySeek(table.Session, table.Table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
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
            return await OpenSession(session =>
            {
                var sid = session.Session;
                using (var table = OpenTable(session, OpenTableGrbit.ReadOnly))
                {
                    if (!SelectIndex(table, query))
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
            return await OpenSession(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    using (var table = OpenTable(session, OpenTableGrbit.ReadOnly))
                    {
                        int count = 0;
                        if (isAdult == null)
                        {
                            table.Indexes.CategoryIndex.SetAsCurrentIndex();
                            count += table.EnumerateUnique().Count();
                        }
                        else
                        {
                            table.Indexes.IsAdultAndCategoryIndex.SetAsCurrentIndex();
                            count += table.Indexes.IsAdultAndCategoryIndex.EnumerateUniqueAsIsAdultFromIndex().Count(v => v.IsAdult == isAdult.Value);
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
            return await OpenSession(session =>
            {
                using (new Transaction(session.Session))
                {
                    using (var table = OpenTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var idSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        if (SelectIndex(table, query))
                        {
                            do
                            {
                                idSet.Add(table.Columns.Id);
                            } while (table.TryMoveNext());
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
            return await OpenSession(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    using (var table = OpenTable(session, OpenTableGrbit.ReadOnly))
                    {
                        HashSet<string> categories = new HashSet<string>();
                        if (isAdult == null)
                        {
                            table.Indexes.CategoryIndex.SetAsCurrentIndex();
                            foreach (var c in table.Indexes.CategoryIndex.EnumerateUniqueAsCategoryFromIndex())
                            {
                                categories.Add(c.Category);
                            }
                        }
                        else
                        {
                            table.Indexes.IsAdultAndCategoryIndex.SetAsCurrentIndex();
                            foreach (var c in table.Indexes.IsAdultAndCategoryIndex.EnumerateUniqueAsIsAdultAndCategoryFromIndex())
                            {
                                if (c.IsAdult == isAdult.Value)
                                {
                                    categories.Add(c.Category);
                                }
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
            return await OpenSession(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    using (var table = OpenTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var id = GetId(link);
                        IBoardReference result = null;
                        if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(id)))
                        {
                            result = ReadFullRow(table);
                        }
                        return result;
                    }
                }
            });
        }

        /// <summary>
        /// Прочитать короткую информацию из текущей записи.
        /// </summary>
        /// <param name="table">Таблица.</param>
        /// <returns>Короткая информация.</returns>
        protected virtual IBoardShortInfo ReadShortInfo(BoardReferenceTable table)
        {
            var viewData = table.Views.ShortInfoView.Fetch();
            return new BoardShortInfo()
            {
                BoardLink = CreateBoardLink(viewData.Id),
                Category = viewData.Category,
                ShortName = viewData.ShortName,
                DisplayName = viewData.DisplayName,
                IsAdult = viewData.IsAdult
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
        /// <param name="table">Таблица.</param>
        /// <returns>Полная информация.</returns>
        protected virtual IBoardReference ReadFullRow(BoardReferenceTable table)
        {
            var viewData = table.Views.FullRowView.Fetch();
            var result = CreateBoardReferenceObject();
            result.BoardLink = CreateBoardLink(viewData.Id);
            result.Category = viewData.Category;
            result.ShortName = viewData.ShortName;
            result.DisplayName = viewData.DisplayName;
            result.IsAdult = viewData.IsAdult;
            SetExtendedInfo(viewData.ExtendedData, result);
            result.BumpLimit = viewData.BumpLimit;
            result.DefaultName = viewData.DefaultName;
            result.Pages = viewData.Pages;
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
        protected virtual async Task<IList<IBoardShortInfo>> DoLoadShortReferences(int start, int count, BoardReferenceStoreQuery query)
        {
            CheckModuleReady();
            if (count < 1)
            {
                IList<IBoardShortInfo> result = new List<IBoardShortInfo>();
                return result;
            }
            await WaitForTablesInitialize();
            return await OpenSession(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    using (var table = OpenTable(session, OpenTableGrbit.ReadOnly))
                    {
                        IList<IBoardShortInfo> result = new List<IBoardShortInfo>();
                        bool isMoved = SelectIndex(table, query);
                        if (isMoved)
                        {
                            if (start > 0)
                            {
                                isMoved = Api.TryMove(table.Session, table, (JET_Move)start, MoveGrbit.None);
                            }
                        }
                        if (isMoved)
                        {
                            int cnt = count;
                            do
                            {
                                cnt--;
                                result.Add(ReadShortInfo(table));
                            } while (table.TryMoveNext() && cnt > 0);
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
            return await OpenSession(session =>
            {
                var sid = session.Session;
                using (new Transaction(sid))
                {
                    using (var table = OpenTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var ids = links.Select(GetId).Distinct().OrderBy(l => l).ToArray();
                        IList<IBoardShortInfo> result = new List<IBoardShortInfo>();
                        foreach (var id in ids)
                        {
                            if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(id)))
                            {
                                result.Add(ReadShortInfo(table));
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
            await OpenSessionAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = OpenTable(session, OpenTableGrbit.None))
                    {
                        DeleteAllRows(table);
                    }
                    return true;
                }, 1.5);
                return Nothing.Value;
            });
        }

        /// <summary>
        /// Обновить данные в текущей строке таблицы.
        /// </summary>
        /// <param name="table">Таблица.</param>
        /// <param name="reference">Ссылка на доску.</param>
        /// <param name="prep">Тип обновления.</param>
        protected virtual void UpdateFullRowInfo(BoardReferenceTable table, IBoardReference reference, JET_prep prep)
        {
            var row = new BoardReferenceTable.ViewValues.FullRowView()
            {
                Id = GetId(reference),
                Category = reference.Category ?? "",
                Pages = reference.Pages,
                IsAdult = reference.IsAdult,
                DefaultName = reference.DefaultName,
                ShortName = reference.ShortName,
                DisplayName = reference.DisplayName,
                ExtendedData = SerializeDataContract(BoardExtendedInfo.ToContract(reference, LinkSerialization)),
                BumpLimit = reference.BumpLimit
            };
            if (prep == JET_prep.Replace)
            {
                table.Update.UpdateAsFullRowView(ref row);
            } else if (prep == JET_prep.Insert)
            {
                table.Insert.InsertAsFullRowView(ref row);
            }
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
            await OpenSessionAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = OpenTable(session, OpenTableGrbit.None))
                    {
                        DoUpdateOneRow(table, reference, false);
                    }
                    return true;
                }, 1.5);
                return Nothing.Value;
            });
        }


        /// <summary>
        /// Обновить одну строку.
        /// </summary>
        /// <param name="table">Таблица.</param>
        /// <param name="reference">Ссылка.</param>
        /// <param name="alwaysInsert">Всегда вставлять (после очистки таблицы).</param>
        protected virtual void DoUpdateOneRow(BoardReferenceTable table, IBoardReference reference, bool alwaysInsert)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (!alwaysInsert)
            {
                var id = GetId(reference);
                if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(id)))
                {
                    UpdateFullRowInfo(table, reference, JET_prep.Replace);
                }
                else
                {
                    UpdateFullRowInfo(table, reference, JET_prep.Insert);
                }
            }
            else
            {
                UpdateFullRowInfo(table, reference, JET_prep.Insert);
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
            await OpenSessionAsync(async session =>
            {
                if (clearPrevious)
                {
                    await session.RunInTransaction(() =>
                    {
                        using (var table = OpenTable(session, OpenTableGrbit.None))
                        {
                            DeleteAllRows(table);
                            return true;
                        }
                    }, 2);
                }
                return Nothing.Value;
            });
            await ParallelizeOnSessions(references.Where(r => r != null).SplitSet(15).DistributeToProcess(5), async (session, list) =>
            {
                foreach (var refs in list)
                {
                    var r = refs.ToArray();
                    await session.RunInTransaction(() =>
                    {
                        using (var table = OpenTable(session, OpenTableGrbit.None))
                        {
                            foreach (var reference in r)
                            {
                                DoUpdateOneRow(table, reference, clearPrevious);
                            }
                        }
                        return true;
                    }, 1.5);
                }
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