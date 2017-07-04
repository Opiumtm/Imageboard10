using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;
using Imageboard10.Core.Utility;
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

        public IAsyncOperation<IBoardPostEntity> Load(Guid id, PostStoreLoadMode mode)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostEntity>> Load(IList<Guid> ids, PostStoreLoadMode mode)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostEntity>> Load(Guid? parentId, int skip, int? count, PostStoreLoadMode mode)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<Guid>> GetChildren(Guid collectionId, int skip, int? count)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<int> GetCollectionSize(Guid collectionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<int> GetTotalSize(PostStoreEntityType type)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<Guid> FindEntity(PostStoreEntityType type, ILink link)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IPostStoreEntityIdSearchResult>> FindEntities(Guid? parentId, IList<ILink> links)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IBoardPostStoreAccessInfo> GetAccessInfo(Guid id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostStoreAccessInfo>> GetAccessInfos(IList<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostStoreAccessInfo>> GetAllAccessInfos()
        {
            throw new NotImplementedException();
        }

        public IAsyncAction Touch(Guid id, DateTimeOffset? accessTime)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<string> GetEtag(Guid id)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction UpdateEtag(Guid id, string etag)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction SetCollectionUpdateInfo(IBoardPostCollectionUpdateInfo updateInfo)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction SetReadPostsCount(Guid id, int readPosts)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperationWithProgress<Guid, OperationProgress> SaveCollection(IBoardPostEntity collection, bool replace, PostStoreStaleDataClearPolicy cleanupPolicy)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IBoardPostCollectionInfoSet> LoadCollectionInfoSet(Guid collectionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction UpdateLikes(IList<IBoardPostLikesStoreInfo> likes)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostLikes>> LoadLikes(IList<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction UpdateFlags(IList<FlagUpdateAction> flags)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<Guid>> LoadFlags(Guid id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<Guid>> GetPostQuotes(Guid id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<PostStoreEntityType> GetCollectionType(Guid collectionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<int> GetMediaCount(Guid id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IPostMedia>> GetPostMedia(Guid id, int skip, int? count)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IPostDocument> GetDocument(Guid id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<Guid>> Delete(IList<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction ClearAllData()
        {
            throw new NotImplementedException();
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

        private async Task DoClearUnfinishedData()
        {
            var toDelete = new Dictionary<Guid, List<Guid>>();
            var orphanParents = new HashSet<Guid>();
            await QueryReadonly(session =>
            {
                using (var parTable = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                {
                    using (var idTable = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var colIds = Api.GetColumnDictionary(parTable.Session, parTable.Table);
                        Api.JetSetCurrentIndex(parTable.Session, parTable.Table, GetIndexName(TableName, nameof(Indexes.ParentId)));
                        if (Api.TryMoveFirst(parTable.Session, parTable.Table))
                        {
                            do
                            {
                                var id = Api.RetrieveColumnAsGuid(parTable.Session, parTable.Table, colIds[ColumnNames.ParentId], RetrieveColumnGrbit.RetrieveFromIndex);
                                if (id != null)
                                {
                                    Api.MakeKey(idTable.Session, idTable.Table, id.Value, MakeKeyGrbit.NewKey);
                                    if (!Api.TrySeek(idTable.Session, idTable.Table, SeekGrbit.SeekEQ))
                                    {
                                        orphanParents.Add(id.Value);
                                    }
                                }
                            } while (Api.TryMove(parTable.Session, parTable.Table, JET_Move.Next, MoveGrbit.MoveKeyNE));
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
                            var id = Api.RetrieveColumnAsGuid(incTable.Session, incTable.Table, colIds[ColumnNames.Id], RetrieveColumnGrbit.RetrieveFromIndex);
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
                        var colIds = Api.GetColumnDictionary(table.Session, table.Table);
                        Api.JetSetCurrentIndex(table.Session, table.Table, GetIndexName(TableName, nameof(Indexes.ParentId)));
                        foreach (var id in orphanParents)
                        {
                            Api.MakeKey(table.Session, table.Table, id, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(table.Session, table.Table, SeekGrbit.SeekEQ))
                            {
                                do
                                {
                                    var id1 = Api.RetrieveColumnAsGuid(table.Session, table.Table, colIds[ColumnNames.Id], RetrieveColumnGrbit.RetrieveFromIndex);
                                    if (id1 != null)
                                    {
                                        if (!toDelete.ContainsKey(id))
                                        {
                                            toDelete[id] = new List<Guid>();
                                        }
                                        toDelete[id].Add(id1.Value);
                                    }
                                } while (Api.TryMoveNext(table.Session, table.Table));
                            }
                        }
                    }
                    return Nothing.Value;
                });
                if (toDelete.Count > 0)
                {
                    foreach (var idk in toDelete)
                    {
                        var parentKey = idk.Key;
                        foreach (var ids in idk.Value.SplitSet(100))
                        {
                            var idsArr = ids.ToArray();
                            await UpdateAsync(async session =>
                            {
                                await session.RunInTransaction(() =>
                                {
                                    using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                                    {
                                        foreach (var id in idsArr)
                                        {
                                            Api.MakeKey(table.Session, table, id, MakeKeyGrbit.NewKey);
                                            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                                            {
                                                Api.JetDelete(table.Session, table.Table);
                                            }
                                        }
                                    }
                                    return true;
                                });
                                return Nothing.Value;
                            });
                        }
                        await UpdateAsync(async session =>
                        {
                            await session.RunInTransaction(() =>
                            {
                                using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                                {
                                    Api.MakeKey(table.Session, table, parentKey, MakeKeyGrbit.NewKey);
                                    if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                                    {
                                        using (var update = new Update(table.Session, table, JET_prep.Replace))
                                        {
                                            Api.SetColumn(table.Session, table, Api.GetTableColumnid(table.Session, table.Table, ColumnNames.ChildrenLoadStage), ChildrenLoadStageId.NotStarted);
                                            update.Save();
                                        }
                                    }
                                }
                                return true;
                            });
                            return Nothing.Value;
                        });
                    }
                }
            }
        }

        public IAsyncOperation<IList<IBoardPostStoreAccessLogItem>> GetAccessLog(PostStoreAccessLogQuery query)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<Guid>> QueryByFlags(PostStoreEntityType type, Guid? parentId, IList<Guid> havingFlags, IList<Guid> notHavingFlags)
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
    }
}