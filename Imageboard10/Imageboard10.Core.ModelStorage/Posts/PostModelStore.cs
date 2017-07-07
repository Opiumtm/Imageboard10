using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;
using Imageboard10.Core.Models.Links;
using Imageboard10.ModuleInterface;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Хранилище постов.
    /// </summary>
    public partial class PostModelStore : ModelStorageBase<IBoardPostStore>, IBoardPostStore
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="engineId">Идентификатор движка.</param>
        public PostModelStore(string engineId)
        {
            EngineId = engineId ?? throw new ArgumentNullException(nameof(engineId));
        }

        /// <summary>
        /// Создать или обновить таблицы.
        /// </summary>
        protected override async ValueTask<Nothing> CreateOrUpgradeTables()
        {
            await EnsureTable(TableName, 1, InitializeMainTable, null, true);
            await EnsureTable(AccessLogTableName, 1, InitializeAccessLogTable, null, true);
            await EnsureTable(MediaFilesTableName, 1, InitializeMediaFilesTable, null, true);
            try
            {
                await DoClearUnfinishedData();
            }
            catch (Exception ex)
            {
                GlobalErrorHandler?.SignalError(ex);
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Получить ссылку на сущность.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Ссылка.</returns>
        public IAsyncOperation<ILink> GetEntityLink(PostStoreEntityId id)
        {
            async Task<ILink> Do()
            {
                return await QueryReadonly(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var colids = Api.GetColumnDictionary(table.Session, table);
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            return GetLinkAtCurrentPosition(table, colids);
                        }
                        return null;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить ссылки на сущности.
        /// </summary>
        /// <param name="ids">Идентификаторы.</param>
        /// <returns>Ссылки.</returns>
        public IAsyncOperation<IList<ILinkWithStoreId>> GetEntityLinks(PostStoreEntityId[] ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            var ids2 = ids.Select(i => i.Id).Distinct().ToArray();

            async Task<IList<ILinkWithStoreId>> Do()
            {
                return await QueryReadonly(session =>
                {
                    var result = new List<ILinkWithStoreId>();
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var colids = Api.GetColumnDictionary(table.Session, table);

                        foreach (var id in ids2)
                        {
                            Api.MakeKey(table.Session, table, id, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                            {
                                result.Add(new LinkWithStoreId()
                                {
                                    Link = GetLinkAtCurrentPosition(table, colids),
                                    Id = new PostStoreEntityId() { Id = id }
                                });
                            }
                        }
                    }
                    return result;
                });
            }

            return Do().AsAsyncOperation();
        }

        public IAsyncOperation<IBoardPostEntity> Load(PostStoreEntityId id, PostStoreLoadMode mode)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostEntity>> Load(IList<PostStoreEntityId> ids, PostStoreLoadMode mode)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostEntity>> Load(PostStoreEntityId? parentId, int skip, int? count, PostStoreLoadMode mode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получить дочерние сущности.
        /// </summary>
        /// <param name="collectionId">Идентификатор коллекции.</param>
        /// <param name="skip">Пропустить постов.</param>
        /// <param name="count">Сколько взять постов (максимально).</param>
        /// <returns>Идентификаторы сущностей.</returns>
        public IAsyncOperation<IList<PostStoreEntityId>> GetChildren(PostStoreEntityId collectionId, int skip, int? count)
        {
            async Task<IList<PostStoreEntityId>> Do()
            {
                return await QueryReadonly(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var result = new List<PostStoreEntityId>();
                        Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
                        Api.MakeKey(table.Session, table, collectionId.Id, MakeKeyGrbit.NewKey);
                        var colid = Api.GetTableColumnid(table.Session, table, ColumnNames.Id);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            int counted = 0;
                            bool skipped = true;
                            if (skip > 0)
                            {
                                skipped = Api.TryMove(table.Session, table, (JET_Move) skip, MoveGrbit.None);
                            }
                            if (skipped)
                            {
                                do
                                {
                                    counted++;
                                    if (counted > count)
                                    {
                                        break;
                                    }
                                    var id = Api.RetrieveColumnAsInt32(table.Session, table, colid) ?? -1;
                                    result.Add(new PostStoreEntityId() { Id = id});
                                } while (Api.TryMoveNext(table.Session, table.Table));
                            }
                        }
                        return result;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Статус загрузки дочерних сущностей.
        /// </summary>
        /// <param name="collectionId">Коллекция.</param>
        /// <returns>Результат.</returns>
        public IAsyncOperation<bool> IsChildrenLoaded(PostStoreEntityId collectionId)
        {
            async Task<bool> Do()
            {
                return await QueryReadonly(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.MakeKey(table.Session, table, collectionId.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            var ls = Api.RetrieveColumnAsByte(table.Session, table, Api.GetTableColumnid(table.Session, table, ColumnNames.ChildrenLoadStage));
                            if (ls == ChildrenLoadStageId.Completed)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить количество постов в коллекции.
        /// </summary>
        /// <param name="collectionId">Идентификатор коллекции.</param>
        /// <returns>Количество постов.</returns>
        public IAsyncOperation<int> GetCollectionSize(PostStoreEntityId collectionId)
        {
            async Task<int> Do()
            {
                return await QueryReadonly(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.ParentId)));
                        Api.MakeKey(table.Session, table, collectionId.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            int cnt;
                            Api.JetIndexRecordCount(table.Session, table, out cnt, int.MaxValue);
                            return cnt;
                        }
                        return 0;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить общее количество сущностей в базе.
        /// </summary>
        /// <param name="type">Тип сущности.</param>
        /// <returns>Количество сущностей.</returns>
        public IAsyncOperation<int> GetTotalSize(PostStoreEntityType type)
        {
            async Task<int> Do()
            {
                return await QueryReadonly(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        int cnt;
                        Api.JetIndexRecordCount(table.Session, table, out cnt, int.MaxValue);
                        return cnt;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Найти коллекцию.
        /// </summary>
        /// <param name="type">Тип сущности.</param>
        /// <param name="link">Ссылка на коллекцию.</param>
        /// <returns>Коллекция.</returns>
        public IAsyncOperation<PostStoreEntityId?> FindEntity(PostStoreEntityType type, ILink link)
        {
            async Task<PostStoreEntityId?> Do()
            {
                var key = ExtractLinkKey(type, link) ?? throw new ArgumentException($"Невозможно определить информацию для поиска из ссылки {link?.GetLinkHash()}.", nameof(link));
                return await QueryReadonly<PostStoreEntityId?>(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.TypeAndPostId)));
                        Api.MakeKey(table.Session, table, (byte)type, MakeKeyGrbit.NewKey);
                        Api.MakeKey(table.Session, table, key.boardId, Encoding.Unicode, MakeKeyGrbit.None);
                        Api.MakeKey(table.Session, table, key.sequenceId, MakeKeyGrbit.None);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            var id = Api.RetrieveColumnAsInt32(table.Session, table, Api.GetTableColumnid(table.Session, table, ColumnNames.Id), RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                            if (id != null)
                            {
                                return new PostStoreEntityId()
                                {
                                    Id = id.Value
                                };
                            }
                        }
                        return null;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Найти коллекции.
        /// </summary>
        /// <param name="parentId">Идентификатор родительской коллекции.</param>
        /// <param name="links">Ссылки.</param>
        /// <returns>Коллекции.</returns>
        public IAsyncOperation<IList<IPostStoreEntityIdSearchResult>> FindEntities(PostStoreEntityId? parentId, IList<ILink> links)
        {
            async Task<IList<IPostStoreEntityIdSearchResult>> Do()
            {
                if (links == null || links.Count == 0)
                {
                    return new List<IPostStoreEntityIdSearchResult>();
                }
                var toFind = links.Distinct(BoardLinkEqualityComparer.Instance).Select(k => new { k, l = ExtractLinkKey(k)})
                    .Where(l => l.l != null)
                    .Select(l => new { l.k, l = l.l.Value })
                    .SelectMany(l => ToEntityTypes(l.l.entityType).Select(et => (boardId: l.l.boardId, sequenceId: l.l.sequenceId, entityType: et, link: l.k)))
                    .ToArray();
                return await QueryReadonly(session =>
                {
                    var result = new List<IPostStoreEntityIdSearchResult>();
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var colids = Api.GetColumnDictionary(table.Session, table);
                        Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.TypeAndPostId)));
                        foreach (var key in toFind)
                        {
                            Api.MakeKey(table.Session, table, (byte)key.entityType, MakeKeyGrbit.NewKey);
                            Api.MakeKey(table.Session, table, key.boardId, Encoding.Unicode, MakeKeyGrbit.None);
                            Api.MakeKey(table.Session, table, key.sequenceId, MakeKeyGrbit.None);
                            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                            {
                                do
                                {
                                    bool skip = false;
                                    if (parentId != null)
                                    {
                                        var parents = EnumMultivalueColumn<Int32ColumnValue>(table, colids[ColumnNames.ParentId]);
                                        // ReSharper disable once SimplifyLinqExpression
                                        if (!parents.Any(c => c.Value == parentId.Value.Id))
                                        {
                                            skip = true;
                                        }
                                    }
                                    var idV = Api.RetrieveColumnAsInt32(table.Session, table.Table, colids[ColumnNames.Id], RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                                    if (idV == null)
                                    {
                                        skip = true;
                                    }
                                    if (!skip)
                                    {
                                        result.Add(new PostStoreEntityIdSearchResult()
                                        {
                                            EntityType = key.entityType,
                                            Link = key.link,
                                            Id = new PostStoreEntityId() { Id = idV.Value }
                                        });
                                    }
                                } while (Api.TryMoveNext(table.Session, table));
                            }
                        }
                    }
                    return result;
                });
            }

            return Do().AsAsyncOperation();
        }

        public IAsyncOperation<IBoardPostStoreAccessInfo> GetAccessInfo(PostStoreEntityId id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostStoreAccessInfo>> GetAccessInfos(IList<PostStoreEntityId> ids)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostStoreAccessInfo>> GetAllAccessInfos()
        {
            throw new NotImplementedException();
        }

        public IAsyncAction Touch(PostStoreEntityId id, DateTimeOffset? accessTime)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<string> GetEtag(PostStoreEntityId id)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction UpdateEtag(PostStoreEntityId id, string etag)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction SetCollectionUpdateInfo(IBoardPostCollectionUpdateInfo updateInfo)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction SetReadPostsCount(PostStoreEntityId id, int readPosts)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Сохранить коллекцию.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        /// <param name="replace">Режим замены.</param>
        /// <param name="cleanupPolicy">Политика зачистки старых данных. Если null - не производить зачистку.</param>
        /// <returns>Идентификатор коллекции.</returns>
        public IAsyncOperationWithProgress<PostStoreEntityId, OperationProgress> SaveCollection(IBoardPostEntity collection, BoardPostCollectionUpdateMode replace, PostStoreStaleDataClearPolicy cleanupPolicy)
        {
            return SaveCollection(collection, replace, cleanupPolicy, null);
        }

        public IAsyncOperation<IBoardPostCollectionInfoSet> LoadCollectionInfoSet(Guid collectionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction UpdateLikes(IList<IBoardPostLikesStoreInfo> likes)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostLikes>> LoadLikes(IList<PostStoreEntityId> ids)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction UpdateFlags(IList<FlagUpdateAction> flags)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<Guid>> LoadFlags(PostStoreEntityId id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<PostStoreEntityId>> GetPostQuotes(PostStoreEntityId id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<PostStoreEntityType> GetCollectionType(PostStoreEntityId collectionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<int> GetMediaCount(PostStoreEntityId id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IPostMedia>> GetPostMedia(PostStoreEntityId id, int skip, int? count)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IPostDocument> GetDocument(PostStoreEntityId id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Удалить. Удаление всегда производится рекурсивно.
        /// </summary>
        /// <param name="ids">Список сущностей.</param>
        /// <returns>Список идентификаторов удалённых сущностей.</returns>
        public IAsyncOperation<IList<PostStoreEntityId>> Delete(IList<PostStoreEntityId> ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));

            async Task<IList<PostStoreEntityId>> Do()
            {
                var allEntities = await QueryReadonly(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        return FindAllChildren(table, ids).ToArray();
                    }
                });
                return await UpdateAsync(async session =>
                {
                    return await DoDeleteEntitiesList(session, allEntities.Select(e => e.id));
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Очистить все данные.
        /// </summary>
        public IAsyncAction ClearAllData()
        {
            async Task Do()
            {
                bool foundAny = true;
                do
                {
                    await UpdateAsync(async session =>
                    {
                        await session.RunInTransaction(() =>
                        {
                            int counter = 200;
                            using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                            {
                                if (Api.TryMoveFirst(table.Session, table))
                                {
                                    do
                                    {
                                        counter--;
                                        Api.JetDelete(table.Session, table);
                                    } while (counter > 0 && Api.TryMoveNext(table.Session, table));
                                }
                                else foundAny = false;
                            }
                            return true;
                        });
                        return Nothing.Value;
                    });
                } while (foundAny);
            }

            return Do().AsAsyncAction();
        }

        public IAsyncAction ClearStaleData(PostStoreStaleDataClearPolicy policy)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Очистить незавершённые загрузки.
        /// </summary>
        public IAsyncAction ClearUnfinishedData()
        {
            async Task Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();
                await DoClearUnfinishedData();
            }

            return Do().AsAsyncAction();
        }

        public IAsyncOperation<IList<IBoardPostStoreAccessLogItem>> GetAccessLog(PostStoreAccessLogQuery query)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<PostStoreEntityId>> QueryByFlags(PostStoreEntityType type, PostStoreEntityId? parentId, IList<Guid> havingFlags, IList<Guid> notHavingFlags)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction ClearAccessLog(double maxAgeSec)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction SyncAccessLog(IList<IBoardPostStoreAccessLogItem> accessLog)
        {
            throw new NotImplementedException();
        }

        private async Task DoClearUnfinishedData()
        {
            var toDelete = new Dictionary<int, List<int>>();
            var orphanParents = new HashSet<int>();
            await QueryReadonly(session =>
            {
                using (var parTable = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                {
                    using (var idTable = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        foreach (var id in FindAllParents(parTable))
                        {
                            Api.MakeKey(idTable.Session, idTable.Table, id.Id, MakeKeyGrbit.NewKey);
                            if (!Api.TrySeek(idTable.Session, idTable.Table, SeekGrbit.SeekEQ))
                            {
                                orphanParents.Add(id.Id);
                            }
                        }
                    }
                }
                using (var incTable = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                {
                    var colIds = Api.GetColumnDictionary(incTable.Session, incTable.Table);
                    Api.JetSetCurrentIndex(incTable.Session, incTable.Table, GetIndexName(TableName, nameof(Indexes.ChildrenLoadStage)));
                    Api.MakeKey(incTable.Session, incTable.Table, ChildrenLoadStageId.Started, MakeKeyGrbit.NewKey);
                    if (Api.TryMoveFirst(incTable.Session, incTable.Table))
                    {
                        do
                        {
                            var id = Api.RetrieveColumnAsInt32(incTable.Session, incTable.Table, colIds[ColumnNames.Id], RetrieveColumnGrbit.RetrieveFromIndex);
                            if (id != null)
                            {
                                orphanParents.Add(id.Value);
                            }
                        } while (Api.TryMoveNext(incTable.Session, incTable.Table));
                    }
                }
                return Nothing.Value;
            });
            if (orphanParents.Count > 0)
            {
                await QueryReadonly(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(table.Session, table.Table, GetIndexName(TableName, nameof(Indexes.ParentId)));
                        foreach (var child in FindAllChildren(table, orphanParents.Select(p => new PostStoreEntityId() { Id = p })))
                        {
                            if (!toDelete.ContainsKey(child.parentId.Id))
                            {
                                toDelete[child.parentId.Id] = new List<int>();
                            }
                            toDelete[child.parentId.Id].Add(child.id.Id);
                        }
                    }
                    return Nothing.Value;
                });
                if (toDelete.Count > 0)
                {
                    foreach (var idk in toDelete)
                    {
                        var parentKey = idk.Key;
                        var children = idk.Value;
                        await UpdateAsync(async session =>
                        {
                            await DoDeleteEntitiesList(session, children.Select(c => new PostStoreEntityId() { Id = c}));
                            return Nothing.Value;
                        });
                        await SetEntityChildrenLoadStatus(new PostStoreEntityId() {Id = parentKey}, ChildrenLoadStageId.NotStarted);
                    }
                }
            }
        }
    }
}