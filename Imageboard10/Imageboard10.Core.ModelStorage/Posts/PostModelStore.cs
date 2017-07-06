using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Utility;
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

        protected async Task DoDeleteEntitiesList(IEsentSession session, IEnumerable<PostStoreEntityId> toDelete)
        {
            async Task Delete(PostStoreEntityId[] toDeletePart)
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                    {
                        foreach (var id in toDeletePart)
                        {
                            Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                            {
                                Api.JetDelete(table.Session, table);
                            }
                        }
                    }
                    return true;
                });
            }

            var split = toDelete.SplitSet(100).Select(s => s.ToArray());
            foreach (var part in split)
            {
                await Delete(part);
            }
        }

        private static readonly HashSet<PostStoreEntityType> AllowedToAdd = new HashSet<PostStoreEntityType>()
        {
            PostStoreEntityType.BoardPage,
            PostStoreEntityType.Catalog,
            PostStoreEntityType.Thread            
        };

        protected void CheckLinkEngine(ILink link)
        {
            if (link == null) throw new ArgumentNullException(nameof(link));
            if (link is IEngineLink el)
            {
                if (!string.Equals(el.Engine, EngineId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Передана сущность с идентификатором сетевого модуля {el.Engine} вместо {EngineId}");
                }
            }
        }

        /// <summary>
        /// Получить информацию о посте из ссылки.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Информация о посте.</returns>
        protected virtual (string boardId, int threadId, int postId) ExtractPostLinkData(ILink link)
        {
            switch (link)
            {
                case PostLink l:
                    return (l.Board, l.OpPostNum, l.PostNum);
                case ThreadLink l:
                    return (l.Board, l.OpPostNum, l.OpPostNum);
                default:
                    throw new ArgumentException($"Невозможно определить информацию о доске и номере поста из ссылки {link.GetLinkHash()}");
            }
        }

        /// <summary>
        /// Получить информацию о треде или каталоге из ссылки.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Информация о посте.</returns>
        protected virtual (string boardId, int sequenceId) ExtractCatalogOrThreadLinkData(ILink link)
        {
            switch (link)
            {
                case CatalogLink l:
                    return (l.Board, (int)l.SortMode);
                case ThreadLink l:
                    return (l.Board, l.OpPostNum);
                default:
                    throw new ArgumentException($"Невозможно определить информацию о доске и сортировке каталога из ссылки {link.GetLinkHash()}");
            }
        }

        private bool SeekExistingEntityInSequence(EsentTable table, PostStoreEntityId directParent, int postId, out PostStoreEntityId id)
        {
            Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
            Api.MakeKey(table.Session, table, directParent.Id, MakeKeyGrbit.NewKey);
            Api.MakeKey(table.Session, table, postId, MakeKeyGrbit.None);
            var r = Api.TrySeek(table.Session, table.Table, SeekGrbit.SeekEQ);
            if (r)
            {
                var id1 = Api.RetrieveColumnAsInt32(table.Session, table.Table, Api.GetTableColumnid(table.Session, table.Table, ColumnNames.Id), RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                if (id1 == null)
                {
                    throw new InvalidOperationException($"Невозможно получить первичный ключ для {EngineId}:{directParent},{postId}");
                }
                id = new PostStoreEntityId() { Id = id1.Value };
            }
            else
            {
                id = new PostStoreEntityId() { Id = -1 };
            }
            return r;
        }

        private bool SeekExistingEntityOnBoard(EsentTable table, PostStoreEntityType entityType, string boardId, int sequenceId, out PostStoreEntityId id)
        {
            Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.TypeAndPostId)));
            Api.MakeKey(table.Session, table, (byte)entityType, MakeKeyGrbit.NewKey);
            Api.MakeKey(table.Session, table, boardId, Encoding.Unicode, MakeKeyGrbit.None);
            Api.MakeKey(table.Session, table, sequenceId, MakeKeyGrbit.None);
            var r = Api.TrySeek(table.Session, table.Table, SeekGrbit.SeekEQ);
            if (r)
            {
                var id1 = Api.RetrieveColumnAsInt32(table.Session, table.Table, Api.GetTableColumnid(table.Session, table.Table, ColumnNames.Id), RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                if (id1 == null)
                {
                    throw new InvalidOperationException($"Невозможно получить первичный ключ для треда или каталога {EngineId}:{entityType},{boardId},{sequenceId}");
                }
                id = new PostStoreEntityId() { Id = id1.Value };
            }
            else
            {
                id = new PostStoreEntityId() { Id = -1 };
            }
            return r;
        }

        /// <summary>
        /// Флаги, управляемые сервером.
        /// </summary>
        /// <returns>Список флагов.</returns>
        protected virtual IEnumerable<Guid> ServerFlags()
        {
            yield return BoardPostFlags.Closed;
            yield return BoardPostFlags.AdminTrip;
            yield return BoardPostFlags.Banned;
            yield return BoardPostFlags.Endless;
            yield return BoardPostFlags.IsEdited;
            yield return BoardPostFlags.Op;
            yield return BoardPostFlags.Sage;
            yield return BoardPostFlags.Sticky;
            yield return BoardPostFlags.ThreadOpPost;
            yield return BoardPostFlags.ThreadPreview;
            yield return PostCollectionFlags.EnableCountryFlags;
            yield return PostCollectionFlags.EnableAudio;
            yield return PostCollectionFlags.EnableDices;
            yield return PostCollectionFlags.EnableIcons;
            yield return PostCollectionFlags.EnableImages;
            yield return PostCollectionFlags.EnableLikes;
            yield return PostCollectionFlags.EnableNames;
            yield return PostCollectionFlags.EnableOekaki;
            yield return PostCollectionFlags.EnablePosting;
            yield return PostCollectionFlags.EnableSage;
            yield return PostCollectionFlags.EnableShield;
            yield return PostCollectionFlags.EnableSubject;
            yield return PostCollectionFlags.EnableThreadTags;
            yield return PostCollectionFlags.EnableTripcodes;
            yield return PostCollectionFlags.EnableVideo;
            yield return PostCollectionFlags.IsBoard;
            yield return PostCollectionFlags.IsClosed;
            yield return PostCollectionFlags.IsIndex;
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

        public IAsyncOperation<IList<PostStoreEntityId>> GetChildren(PostStoreEntityId collectionId, int skip, int? count)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<bool> IsChildrenLoaded(PostStoreEntityId collectionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<int> GetCollectionSize(PostStoreEntityId collectionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<int> GetTotalSize(PostStoreEntityType type)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<PostStoreEntityId> FindEntity(PostStoreEntityType type, ILink link)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IPostStoreEntityIdSearchResult>> FindEntities(PostStoreEntityId? parentId, IList<ILink> links)
        {
            throw new NotImplementedException();
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

        private void WriteThreadPreviewCount(BinaryWriter wr, int? cnt)
        {
            if (cnt == null)
            {
                wr.Write(false);
            }
            else
            {
                wr.Write(true);
                wr.Write(cnt.Value);
            }
        }

        private int? ReadThreadPreviewCount(BinaryReader rd)
        {
            var f = rd.ReadBoolean();
            if (f)
            {
                return rd.ReadInt32();
            }
            return null;
        }

        private byte[] WriteThreadPreviewCounts(IThreadPreviewPostCollection pc)
        {
            using (var str = new MemoryStream())
            {
                using (var wr = new BinaryWriter(str, Encoding.UTF8, true))
                {
                    WriteThreadPreviewCount(wr, pc?.Omit);
                    WriteThreadPreviewCount(wr, pc?.ReplyCount);
                    WriteThreadPreviewCount(wr, pc?.OmitImages);
                    WriteThreadPreviewCount(wr, pc?.ImageCount);
                    wr.Flush();
                }
                return str.ToArray();
            }
        }

        private (int? omit, int? replyCount, int? omitImages, int? imageCount) ReadThreadPreviewCounts(byte[] data)
        {
            try
            {
                if (data == null)
                {
                    return (null, null, null, null);
                }
                using (var str = new MemoryStream(data))
                {
                    using (var rd = new BinaryReader(str))
                    {
                        return (ReadThreadPreviewCount(rd), ReadThreadPreviewCount(rd), ReadThreadPreviewCount(rd), ReadThreadPreviewCount(rd));
                    }
                }
            }
            catch
            {
                return (null, null, null, null);
            }
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

        public IAsyncOperation<IList<PostStoreEntityId>> Delete(IList<PostStoreEntityId> ids)
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

        private IEnumerable<(PostStoreEntityId id, PostStoreEntityId parentId)> FindAllChildren(EsentTable table, IEnumerable<PostStoreEntityId> parents)
        {
            Api.JetSetCurrentIndex(table.Session, table.Table, GetIndexName(TableName, nameof(Indexes.ParentId)));
            var colid = Api.GetTableColumnid(table.Session, table, ColumnNames.Id);
            foreach (var id in parents.Distinct())
            {
                Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                {
                    do
                    {
                        var cid = Api.RetrieveColumnAsInt32(table.Session, table.Table, colid, RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                        if (cid.HasValue)
                        {
                            yield return (new PostStoreEntityId() { Id = cid.Value }, id);
                        }
                    } while (Api.TryMoveNext(table.Session, table.Table));
                }
            }
        }

        private IEnumerable<(int sequenceId, PostStoreEntityId parentId)> FindAllChildrenSeqNums(EsentTable table, IEnumerable<PostStoreEntityId> parents)
        {
            Api.JetSetCurrentIndex(table.Session, table.Table, GetIndexName(TableName, nameof(Indexes.ParentId)));
            var colid = Api.GetTableColumnid(table.Session, table, ColumnNames.SequenceNumber);
            foreach (var id in parents.Distinct())
            {
                Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                {
                    do
                    {
                        var cid = Api.RetrieveColumnAsInt32(table.Session, table.Table, colid, RetrieveColumnGrbit.None);
                        if (cid.HasValue)
                        {
                            yield return (cid.Value, id);
                        }
                    } while (Api.TryMoveNext(table.Session, table.Table));
                }
            }
        }

        private IEnumerable<(PostStoreEntityId id, PostStoreEntityId parentId)> FindAllChildren(EsentTable table, PostStoreEntityId parent)
        {
            return FindAllChildren(table, new [] {parent});
        }

        private IEnumerable<(int sequenceId, PostStoreEntityId parentId)> FindAllChildrenSeqNums(EsentTable table, PostStoreEntityId parent)
        {
            return FindAllChildrenSeqNums(table, new[] { parent });
        }

        private IEnumerable<PostStoreEntityId> FindAllParents(EsentTable table)
        {
            var colid = Api.GetTableColumnid(table.Session, table.Table, ColumnNames.ParentId);
            Api.JetSetCurrentIndex(table.Session, table.Table, GetIndexName(TableName, nameof(Indexes.ParentId)));
            if (Api.TryMoveFirst(table.Session, table))
            {
                do
                {
                    var id = Api.RetrieveColumnAsInt32(table.Session, table, colid);
                    if (id != null)
                    {
                        yield return new PostStoreEntityId() { Id = id.Value };
                    }
                } while (Api.TryMove(table.Session, table, JET_Move.Next, MoveGrbit.MoveKeyNE));
            }
        }

        private async Task SetEntityChildrenLoadStatus(PostStoreEntityId id, byte status)
        {
            await UpdateAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                    {
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            using (var update = new Update(table.Session, table, JET_prep.Replace))
                            {
                                Api.SetColumn(table.Session, table, Api.GetTableColumnid(table.Session, table.Table, ColumnNames.ChildrenLoadStage), status);
                                update.Save();
                            }
                        }
                    }
                    return true;
                });
                return Nothing.Value;
            });
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