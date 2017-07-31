using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.Tasks;
using Imageboard10.Core.Utility;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Хранилище постов.
    /// </summary>
    public partial class PostModelStore
    {
        // ReSharper disable InconsistentNaming
        private struct BasicEntityInfo
        {
            public PostStoreEntityType entityType;
            public GenericPostStoreEntityType genEntityType;
            public ILink link;
            public ILink parentLink;
            public PostStoreEntityId entityId;
            public PostStoreEntityId? parentEntityId;
            public int sequenceId;
            public int? parentSequenceId;
            public string boardId;
        }

        private struct LoadPostDataContext : IDisposable
        {
            public PostsTable table;
            public MediaFilesTable mediaTable;
            public PostsTable quotesTable;

            public void Dispose()
            {
                table?.Dispose();
                mediaTable?.Dispose();
                quotesTable?.Dispose();
            }
        }
        // ReSharper enable InconsistentNaming

        private LoadPostDataContext CreateLoadContext(IEsentSession session, bool openMedia, bool openQuotes)
        {
            var r = new LoadPostDataContext();
            try
            {
                r.table = OpenPostsTable(session, OpenTableGrbit.ReadOnly);
                if (openMedia)
                {
                    r.mediaTable = OpenMediaFilesTable(session, OpenTableGrbit.ReadOnly);
                    r.mediaTable.Indexes.SequencesIndex.SetAsCurrentIndex();
                }
                if (openQuotes)
                {
                    r.quotesTable = OpenPostsTable(session, OpenTableGrbit.ReadOnly);
                    r.quotesTable.Indexes.QuotedPostsIndex.SetAsCurrentIndex();
                }
            }
            catch
            {
                r.Dispose();
                throw;
            }
            return r;
        }

        private List<ILink> LoadQuotesForPost(PostsTable quotesTable, bool setIndex, ref BasicEntityInfo bi)
        {
            var r = new HashSet<ILink>(BoardLinkEqualityComparer.Instance);
            if (bi.parentEntityId != null && bi.parentSequenceId != null)
            {
                var index = quotesTable.Indexes.QuotedPostsIndex;
                if (setIndex)
                {
                    index.SetAsCurrentIndex();
                }
                foreach (var seqId in index.EnumerateAsSequenceNumberView(index.CreateKey(bi.parentEntityId.Value.Id, bi.sequenceId)))
                {
                    r.Add(new PostLink()
                    {
                        Engine = EngineId,
                        Board = bi.boardId,
                        OpPostNum = bi.parentSequenceId.Value,
                        PostNum = seqId.SequenceNumber
                    });
                }
            }
            return r.OrderBy(l => l, BoardLinkComparer.Instance).ToList();
        }

        private List<IPostMedia> LoadPostMedia(MediaFilesTable mediaTable, ref BasicEntityInfo bi, bool setIndex)
        {
            var r = new List<IPostMedia>();
            var index = mediaTable.Indexes.SequencesIndex;
            if (setIndex)
            {
                index.SetAsCurrentIndex();
            }
            foreach (var md in index.EnumerateAsMediaDataView(index.CreateKey(bi.entityId.Id)))
            {
                var m = ObjectSerializationService.Deserialize(md.MediaData) as IPostMedia;
                if (m != null)
                {
                    r.Add(m);
                }
            }
            return r;
        }

        private void LoadBasicInfo(PostsTable table, ref BasicEntityInfo bi)
        {
            var v = table.Views.BasicLoadInfoView.Fetch();
            bi.entityType = (PostStoreEntityType) v.EntityType;
            bi.genEntityType = ToGenericEntityType(bi.entityType);
            (bi.link, bi.parentLink, bi.sequenceId, bi.boardId, bi.parentSequenceId) = LoadEntityLinks(v, bi.genEntityType);
            bi.entityId = new PostStoreEntityId() { Id = v.Id };
            bi.parentEntityId = v.DirectParentId != null ? (PostStoreEntityId?)(new PostStoreEntityId() { Id = v.DirectParentId.Value }) : null;
        }

        private void LoadBasicInfo(PostsTable.ViewValues.BasicLoadInfoView v, ref BasicEntityInfo bi)
        {
            bi.entityType = (PostStoreEntityType)v.EntityType;
            bi.genEntityType = ToGenericEntityType(bi.entityType);
            (bi.link, bi.parentLink, bi.sequenceId, bi.boardId, bi.parentSequenceId) = LoadEntityLinks(v, bi.genEntityType);
            bi.entityId = new PostStoreEntityId() { Id = v.Id };
            bi.parentEntityId = v.DirectParentId != null ? (PostStoreEntityId?)(new PostStoreEntityId() { Id = v.DirectParentId.Value }) : null;
        }

        private IBoardPostEntity LoadLinkOnly(PostsTable table)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            LoadBasicInfo(table, ref bi);
            return new PostModelStoreBareEntityLink()
            {
                EntityType = bi.entityType,
                Link = bi.link,
                ParentLink = bi.parentLink,
                StoreId = bi.entityId,
                StoreParentId = bi.parentEntityId
            };
        }

        private void SetBareEntityData(PostsTable table, PostModelStoreBareEntity data, ref BasicEntityInfo bi)
        {
            SetBareEntityData(table.Views.BareEntityLoadInfoView.Fetch(), data, ref bi);
        }

        private void SetBareEntityData(PostsTable.ViewValues.BareEntityLoadInfoView v, PostModelStoreBareEntity data, ref BasicEntityInfo bi)
        {
            LoadBasicInfo(v, ref bi);
            data.EntityType = bi.entityType;
            data.Link = bi.link;
            data.ParentLink = bi.parentLink;
            data.Thumbnail = ObjectSerializationService.Deserialize(v.Thumbnail) as IPostMediaWithSize;
            data.Subject = v.Subject;
            data.StoreId = bi.entityId;
            data.StoreParentId = bi.parentEntityId;
        }

        private IBoardPostEntity LoadBareEntity(PostsTable table)
        {
            var r = new PostModelStoreBareEntity();
            BasicEntityInfo bi = default(BasicEntityInfo);
            SetBareEntityData(table, r, ref bi);
            return r;
        }

        private void SetPostLightData(IEsentSession session, PostsTable table, bool getPostCount, PostModelStorePostLight data, ref BasicEntityInfo bi)
        {
            SetPostLightData(session, table.Views.PostLightLoadView.Fetch(), getPostCount, data, ref bi);
        }

        private void SetPostLightData(IEsentSession session, PostsTable.ViewValues.PostLightLoadView v, bool getPostCount, PostModelStorePostLight data, ref BasicEntityInfo bi)
        {
            SetBareEntityData(v, data, ref bi);
            data.BoardSpecificDate = v.BoardSpecificDate;
            data.Counter = getPostCount && bi.parentEntityId != null ? GetPostCounterNumber(session, bi.parentEntityId.Value, bi.sequenceId) ?? 0 : 0;
            data.Date = FromUtcToOffset(v.Date) ?? DateTimeOffset.MinValue;
            data.Flags = v.Flags.Where(g => g?.Value != null).Select(g => g.Value.Value).Distinct().ToList();
            data.TagsSet = v.ThreadTags
                .Where(t => !string.IsNullOrEmpty(t?.Value))
                .Select(t => t.Value)
                .Distinct()
                .OrderBy(t => t, StringComparer.CurrentCulture)
                .ToArray();
            data.LLikes = v.Likes;
            data.LDislikes = v.Dislikes;
        }

        private IBoardPostEntity LoadPostLight(IEsentSession session, PostsTable table, bool getPostCount)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            var r = new PostModelStorePostLight();
            SetPostLightData(session, table, getPostCount, r, ref bi);
            return r;
        }

        private void SetPostData(IEsentSession session, ref LoadPostDataContext loadContext, PostsTable.ViewValues.PostFullLoadView v, bool getPostCount, PostModelStorePost data, ref BasicEntityInfo bi)
        {
            SetPostLightData(session, v, getPostCount, data, ref bi);
            var posterName = v.PosterName;
            var otherData = DeserializeDataContract<PostOtherData>(v.OtherDataBinary);
            otherData?.FillPostData(data, LinkSerialization, posterName);
            data.Comment = ObjectSerializationService.Deserialize(v.Document) as IPostDocument;
            data.LoadedTime = FromUtcToOffset(v.LoadedTime) ?? DateTimeOffset.MinValue;
            data.MediaFiles = LoadPostMedia(loadContext.mediaTable ?? throw new InvalidOperationException("Таблица медиафайлов не открыта"), ref bi, false);
            data.Quotes = LoadQuotesForPost(loadContext.quotesTable ?? throw new InvalidOperationException("Таблица цитат не открыта"), false, ref bi);
        }

        private void SetPostData(IEsentSession session, ref LoadPostDataContext loadContext, bool getPostCount, PostModelStorePost data, ref BasicEntityInfo bi)
        {
            SetPostData(session, ref loadContext, loadContext.table.Views.PostFullLoadView.Fetch(), getPostCount, data, ref bi);
        }

        private IBoardPostEntity LoadPost(IEsentSession session, ref LoadPostDataContext loadContext, bool getPostCount)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            var r = new PostModelStorePost();
            SetPostData(session, ref loadContext, getPostCount, r, ref bi);
            return r;
        }

        private void SetPostCollectionData(PostsTable.ViewValues.PostCollectionLoadInfoView v, PostModelStoreCollection data, ref BasicEntityInfo bi)
        {
            SetBareEntityData(v, data, ref bi);
            data.Etag = v.Etag;
            data.Info = ObjectSerializationService.Deserialize(v.OtherDataBinary) as IBoardPostCollectionInfoSet;
            data.Stage = v.ChildrenLoadStage;
        }

        private void SetPostCollectionData(PostsTable table, PostModelStoreCollection data, ref BasicEntityInfo bi)
        {
            SetPostCollectionData(table.Views.PostCollectionLoadInfoView.Fetch(), data, ref bi);
        }

        private IBoardPostEntity LoadPostCollection(PostsTable table)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            var r = new PostModelStoreCollection();
            SetPostCollectionData(table, r, ref bi);
            return r;
        }

        private void SetThreadPreviewData(PostsTable.ViewValues.ThreadPreviewLoadInfoView v, PostModelStoreThreadPreview data, ref BasicEntityInfo bi)
        {
            SetPostCollectionData(v, data, ref bi);
            var counts = ReadThreadPreviewCounts(v.PreviewCounts);
            data.ImageCount = counts.imageCount;
            data.Omit = counts.omit;
            data.OmitImages = counts.omitImages;
            data.ReplyCount = counts.replyCount;
        }

        private void SetThreadPreviewData(PostsTable table, PostModelStoreThreadPreview data, ref BasicEntityInfo bi)
        {
            SetPostCollectionData(table.Views.ThreadPreviewLoadInfoView.Fetch(), data, ref bi);
        }

        private IBoardPostEntity LoadThreadPreview(PostsTable table)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            var r = new PostModelStoreThreadPreview();
            SetThreadPreviewData(table, r, ref bi);
            return r;
        }

        private void SetThreadCollectionData(PostsTable.ViewValues.PostCollectionLoadInfoView v, PostModelStoreThreadCollection data, ref BasicEntityInfo bi)
        {
            SetBareEntityData(v, data, ref bi);
            data.Etag = v.Etag;
            data.Info = ObjectSerializationService.Deserialize(v.OtherDataBinary) as IBoardPostCollectionInfoSet;
            data.Stage = v.ChildrenLoadStage;
        }

        private void SetThreadCollectionData(PostsTable table, PostModelStoreThreadCollection data, ref BasicEntityInfo bi)
        {
            SetBareEntityData(table.Views.PostCollectionLoadInfoView.Fetch(), data, ref bi);
        }

        private IBoardPostEntity LoadThreadCollection(PostsTable table)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            var r = new PostModelStoreThreadCollection();
            SetThreadCollectionData(table, r, ref bi);
            return r;
        }

        private int? GetPostCounterNumber(IEsentSession session, PostStoreEntityId directParentId, int sequenceNumber)
        {
            using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
            {
                var index = table.Indexes.InThreadPostLinkIndex;
                index.SetAsCurrentIndex();
                int cnt = 0;
                foreach (var v in index.EnumerateAsSequenceNumberView(index.CreateKey(directParentId.Id)))
                {
                    cnt++;
                    if (v.SequenceNumber == sequenceNumber)
                    {
                        return cnt;
                    }
                    if (v.SequenceNumber > sequenceNumber)
                    {
                        return null;
                    }
                }
                return null;
            }
        }

        private (ILink link, ILink parentLink, int sequenceId, string boardId, int? parentSequenceId) LoadEntityLinks(PostsTable.ViewValues.BasicLoadInfoView v, GenericPostStoreEntityType genEntityType)
        {
            var boardId = v.BoardId;
            var seqId = v.SequenceNumber;
            var parentSeqId = v.ParentSequenceNumber;
            ILink link, parentLink;
            ConstructLinksForBasicLoad(genEntityType, boardId, parentSeqId, seqId, out link, out parentLink);
            return (link, parentLink, seqId, boardId, parentSeqId);
        }

        private void ConstructLinksForBasicLoad(GenericPostStoreEntityType genEntityType, string boardId,
            int? parentSeqId, int seqId, out ILink link, out ILink parentLink)
        {
            switch (genEntityType)
            {
                case GenericPostStoreEntityType.Post:
                    link = new PostLink()
                    {
                        Engine = EngineId,
                        Board = boardId,
                        OpPostNum = parentSeqId ?? 0,
                        PostNum = seqId
                    };
                    parentLink = new ThreadLink()
                    {
                        Engine = EngineId,
                        Board = boardId,
                        OpPostNum = parentSeqId ?? 0
                    };
                    break;
                case GenericPostStoreEntityType.Catalog:
                    link = new CatalogLink()
                    {
                        Engine = EngineId,
                        Board = boardId,
                        SortMode = (BoardCatalogSort) seqId
                    };
                    parentLink = new BoardLink()
                    {
                        Engine = EngineId,
                        Board = boardId
                    };
                    break;
                case GenericPostStoreEntityType.Thread:
                    link = new ThreadLink()
                    {
                        Engine = EngineId,
                        Board = boardId,
                        OpPostNum = seqId
                    };
                    parentLink = new BoardLink()
                    {
                        Engine = EngineId,
                        Board = boardId
                    };
                    break;
                case GenericPostStoreEntityType.BoardPage:
                    link = new BoardPageLink()
                    {
                        Engine = EngineId,
                        Board = boardId,
                        Page = seqId
                    };
                    parentLink = new RootLink()
                    {
                        Engine = EngineId,
                    };
                    break;
                default:
                    link = null;
                    parentLink = new RootLink()
                    {
                        Engine = EngineId,
                    };
                    break;
            }
        }

        private (ILink link, ILink parentLink, int sequenceId, string boardId, int? parentSequenceId) LoadEntityLinks(PostsTable table, GenericPostStoreEntityType genEntityType)
        {
            var v = table.Views.LinkInfoView.Fetch();
            var boardId = v.BoardId;
            var seqId = v.SequenceNumber;
            var parentSeqId = v.ParentSequenceNumber;
            ILink link, parentLink;
            ConstructLinksForBasicLoad(genEntityType, boardId, parentSeqId, seqId, out link, out parentLink);
            return (link, parentLink, seqId, boardId, parentSeqId);
        }

        private (Guid? lastAccessEnty, DateTime? lastAccessUtc) GetLastAccess(IEsentSession session, PostStoreEntityId id)
        {
            DateTime? lastAccessUtc = null;
            Guid? lastAcessEntry = null;
            using (var accTable = OpenAccessLogTable(session, OpenTableGrbit.ReadOnly))
            {
                var index = accTable.Indexes.EntityIdAndAccessTimeIndex;
                index.SetAsCurrentIndex();
                Api.MakeKey(accTable.Session, accTable, id.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnEndLimit);
                if (Api.TrySeek(accTable.Session, accTable, SeekGrbit.SeekLE))
                {
                    var v = accTable.Views.AccessTimeAndId.Fetch();
                    lastAccessUtc = v.AccessTime;
                    lastAcessEntry = v.Id;
                }
            }
            return (lastAcessEntry, lastAccessUtc);
        }

        private ILink LoadLastPostOnServer(PostsTable table)
        {
            var v = table.Views.LastLinkInfoView.Fetch();
            var boardId = v.BoardId ?? "";
            var seqId = v.SequenceNumber;
            var id = v.LastPostLinkOnServer ?? seqId;
            return new PostLink() { Engine = EngineId, Board = boardId, OpPostNum = seqId, PostNum = id };
        }

        private ILink LoadThreadLastLoadedPost(IEsentSession session, PostStoreEntityId directParentId)
        {
            using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
            {
                table.Indexes.InThreadPostLinkIndex.SetAsCurrentIndex();
                Api.MakeKey(table.Session, table, directParentId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnEndLimit);
                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekLE))
                {
                    var links = LoadEntityLinks(table, GenericPostStoreEntityType.Post);
                    return links.link;
                }
            }
            return null;
        }

        private DateTimeOffset? FromUtcToOffset(DateTime? utcTime)
        {
            if (utcTime == null)
            {
                return null;
            }
            var utc = utcTime.Value;
            DateTimeOffset ofs = utc.ToLocalTime();
            return ofs;
        }

        private IBoardPostStoreAccessInfo LoadAccessInfo(IEsentSession session, PostsTable table)
        {
            var bareEntity = LoadBareEntity(table);
            DateTime? lastAccessUtc = null;
            Guid? lastAcessEntry = null;
            if (bareEntity.StoreId != null)
            {
                (lastAcessEntry, lastAccessUtc) = GetLastAccess(session, bareEntity.StoreId.Value);
            }
            DateTimeOffset? lastAccess = FromUtcToOffset(lastAccessUtc);
            var flags = table.Columns.Flags.Values.Where(g => g?.Value != null).ToHashSet(g => g?.Value ?? Guid.Empty);
            return new PostStoreAccessInfo()
            {
                LogEntryId = lastAcessEntry,
                Etag = table.Columns.Etag,
                Entity = bareEntity,
                AccessTime = lastAccess,
                IsArchived = flags.Any(f => f == PostCollectionFlags.IsArchived),
                IsFavorite = flags.Any(f => f == PostCollectionFlags.IsFavorite),
                LastPost = bareEntity.EntityType == PostStoreEntityType.Thread ? LoadLastPostOnServer(table) : null,
                LastLoadedPost = bareEntity.EntityType == PostStoreEntityType.Thread && bareEntity.StoreId != null ? LoadThreadLastLoadedPost(session, bareEntity.StoreId.Value) : null,
                NumberOfLoadedPosts = bareEntity.EntityType == PostStoreEntityType.Thread && bareEntity.StoreId != null ? CountDirectParent(session, bareEntity.StoreId.Value) : 0,
                NumberOfPosts = bareEntity.EntityType == PostStoreEntityType.Thread ? table.Columns.NumberOfPostsOnServer : null,
                NumberOfReadPosts = bareEntity.EntityType == PostStoreEntityType.Thread ? table.Columns.NumberOfReadPosts : null,
                LastDownload = FromUtcToOffset(table.Columns.LoadedTime),
                LastUpdate = bareEntity.EntityType == PostStoreEntityType.Thread ? FromUtcToOffset(table.Columns.LastServerUpdate) : null                
            };
        }

        private IBoardPostEntity LoadPost(IEsentSession session, ref LoadPostDataContext loadContext, PostStoreLoadMode loadMode)
        {
            switch (loadMode.EntityLoadMode)
            {
                case PostStoreEntityLoadMode.LinkOnly:
                    return LoadLinkOnly(loadContext.table);
                case PostStoreEntityLoadMode.EntityOnly:
                    return LoadBareEntity(loadContext.table);
                case PostStoreEntityLoadMode.Light:
                    return LoadPostLight(session, loadContext.table, loadMode.RetrieveCounterNumber);
                case PostStoreEntityLoadMode.Full:
                    return LoadPost(session, ref loadContext, loadMode.RetrieveCounterNumber);
                default:
                    return null;
            }
        }

        private IBoardPostEntity LoadPostCollection(IEsentSession session, ref LoadPostDataContext loadContext, PostStoreLoadMode loadMode)
        {
            switch (loadMode.EntityLoadMode)
            {
                case PostStoreEntityLoadMode.LinkOnly:
                    return LoadLinkOnly(loadContext.table);
                case PostStoreEntityLoadMode.EntityOnly:
                    return LoadBareEntity(loadContext.table);
                case PostStoreEntityLoadMode.Light:
                case PostStoreEntityLoadMode.Full:
                    return LoadPostCollection(loadContext.table);
                default:
                    return null;
            }
        }

        private IBoardPostEntity LoadThreadPreview(IEsentSession session, ref LoadPostDataContext loadContext, PostStoreLoadMode loadMode)
        {
            switch (loadMode.EntityLoadMode)
            {
                case PostStoreEntityLoadMode.LinkOnly:
                    return LoadLinkOnly(loadContext.table);
                case PostStoreEntityLoadMode.EntityOnly:
                    return LoadBareEntity(loadContext.table);
                case PostStoreEntityLoadMode.Light:
                case PostStoreEntityLoadMode.Full:
                    return LoadThreadPreview(loadContext.table);
                default:
                    return null;
            }
        }

        private IBoardPostEntity LoadThreadCollection(IEsentSession session, ref LoadPostDataContext loadContext, PostStoreLoadMode loadMode)
        {
            switch (loadMode.EntityLoadMode)
            {
                case PostStoreEntityLoadMode.LinkOnly:
                    return LoadLinkOnly(loadContext.table);
                case PostStoreEntityLoadMode.EntityOnly:
                    return LoadBareEntity(loadContext.table);
                case PostStoreEntityLoadMode.Light:
                case PostStoreEntityLoadMode.Full:
                    return LoadThreadCollection(loadContext.table);
                default:
                    return null;
            }
        }

        private IBoardPostEntity LoadBoardEntity(IEsentSession session, ref LoadPostDataContext loadContext, PostStoreLoadMode loadMode)
        {
            var entityType = (PostStoreEntityType) loadContext.table.Columns.EntityType;
            switch (entityType)
            {
                case PostStoreEntityType.Post:
                case PostStoreEntityType.ThreadPreviewPost:
                case PostStoreEntityType.CatalogPost:
                    return LoadPost(session, ref loadContext, loadMode);
                case PostStoreEntityType.Thread:
                case PostStoreEntityType.Catalog:
                    return LoadPostCollection(session, ref loadContext, loadMode);
                case PostStoreEntityType.ThreadPreview:
                    return LoadThreadPreview(session, ref loadContext, loadMode);
                case PostStoreEntityType.BoardPage:
                    return LoadThreadCollection(session, ref loadContext, loadMode);
                default:
                    return null;
            }
        }

        private List<(PostStoreEntityId id, int counter)> EnumEntityChildren(IEsentSession session, PostStoreEntityId entityId)
        {
            var children = new List<(PostStoreEntityId id, int counter)>();
            int counter = 0;
            using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
            {
                var index = table.Indexes.InThreadPostLinkIndex;
                index.SetAsCurrentIndex();
                foreach (var id in index.EnumerateAsRetrieveIdFromIndexView(index.CreateKey(entityId.Id)))
                {
                    var id1 = new PostStoreEntityId() { Id = id.Id };
                    counter++;
                    children.Add((id1, counter));
                }
            }
            return children;
        }

        private async ValueTask<Nothing> SetChildren(IBoardPostEntity entity)
        {
            if (entity == null)
            {
                return Nothing.Value;
            }

            ValueTask<List<IBoardPostEntity>> DoLoad(IList<(PostStoreEntityId id, int counter)> toLoad)
            {
                return OpenSession(session =>
                {
                    var result = new List<IBoardPostEntity>();
                    var loadContext = CreateLoadContext(session, true, true);
                    var loadContextToDispose = loadContext;
                    using (loadContextToDispose)
                    {
                        foreach (var id in toLoad)
                        {
                            Api.MakeKey(loadContext.table.Session, loadContext.table, id.id.Id, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(loadContext.table.Session, loadContext.table, SeekGrbit.SeekEQ))
                            {
                                var r = LoadBoardEntity(session, ref loadContext, FullLoadMode);
                                if (r is IBoardPostLight l)
                                {
                                    l.Counter = id.counter;
                                }
                                result.Add(r);
                            }
                        }
                    }
                    return result;
                });
            }

            var collection = entity as IBoardPostCollection;
            var threadCollection = entity as IBoardPageThreadCollection;
            if ((entity.EntityType == PostStoreEntityType.Thread || entity.EntityType == PostStoreEntityType.ThreadPreview || entity.EntityType == PostStoreEntityType.Catalog) 
                && entity is IPostModelStoreChildrenLoadStageInfo loadStage && (collection?.Posts != null || threadCollection?.Threads != null) 
                && entity.StoreId != null)
            {
                if (loadStage.Stage == ChildrenLoadStageId.Completed)
                {
                    var childrenIds = await OpenSession(session0 => EnumEntityChildren(session0, entity.StoreId.Value));

                    IList<IBoardPostEntity> loaded;
                    if (childrenIds.Count > 4)
                    {
                        var toLoad = childrenIds.DistributeToProcess(3);
                        var tasks = toLoad.Select(DoLoad).ToArray();
                        var taskResults = await CoreTaskHelper.WhenAllValueTasks(tasks);
                        loaded = taskResults.SelectMany(r => r).ToList();
                    }
                    else
                    {
                        loaded = await DoLoad(childrenIds);
                    }

                    if (collection?.Posts != null)
                    {
                        var toAdd = loaded.OfType<IBoardPost>().Deduplicate(p => p.Link, BoardLinkEqualityComparer.Instance).OrderBy(p => p.Link, BoardLinkComparer.Instance);
                        foreach (var item in toAdd)
                        {
                            collection.Posts.Add(item);
                        }
                    }
                    if (threadCollection?.Threads != null)
                    {
                        var toAdd = loaded.OfType<IThreadPreviewPostCollection>().Deduplicate(p => p.Link, BoardLinkEqualityComparer.Instance).OrderBy(p => p.Link, BoardLinkComparer.Instance);
                        foreach (var item in toAdd)
                        {
                            threadCollection.Threads.Add(item);
                        }
                    }
                }
            }

            return Nothing.Value;
        }

        private async Task<IList<IBoardPostEntity>> LoadEntities(IList<PostStoreEntityId> ids, PostStoreLoadMode mode)
        {
            ValueTask<List<IBoardPostEntity>> DoLoad(IList<PostStoreEntityId> toLoad)
            {
                return OpenSession(session =>
                {
                    var result = new List<IBoardPostEntity>();
                    var loadContext = CreateLoadContext(session, true, true);
                    var loadContextToDispose = loadContext;
                    using (loadContextToDispose)
                    {
                        foreach (var id in toLoad)
                        {
                            Api.MakeKey(loadContext.table.Session, loadContext.table, id.Id, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(loadContext.table.Session, loadContext.table, SeekGrbit.SeekEQ))
                            {
                                var r = LoadBoardEntity(session, ref loadContext, mode);
                                result.Add(r);
                            }
                        }
                    }
                    return result;
                });
            }

            if (ids == null) throw new ArgumentNullException(nameof(ids));
            mode = mode ?? DefaultLoadMode;

            IList<IBoardPostEntity> loaded;

            if (ids.Count > 4)
            {
                var toLoad = ids.DistributeToProcess(3);
                var tasks = toLoad.Select(DoLoad).ToArray();
                var taskResults = await CoreTaskHelper.WhenAllValueTasks(tasks);
                loaded = taskResults.SelectMany(r => r).ToList();
            }
            else
            {
                loaded = await DoLoad(ids);
            }

            if (mode.EntityLoadMode == PostStoreEntityLoadMode.Full)
            {
                await FillChildrenInLoadResult(loaded);
            }

            return loaded;
        }

        private async Task FillChildrenInLoadResult(IList<IBoardPostEntity> loaded)
        {
            foreach (var e in loaded)
            {
                if (e is IBoardPostCollection)
                {
                    await SetChildren(e);
                }
                if (e is IBoardPageThreadCollection tpc)
                {
                    await SetChildren(e);
                    if (tpc.Threads?.Count > 0)
                    {
                        foreach (var e2 in tpc.Threads)
                        {
                            await SetChildren(e2);
                        }
                    }
                }
            }
        }

        private async Task<IList<IBoardPostEntity>> LoadEntities(IList<(PostStoreEntityId id, int counter)> ids, PostStoreLoadMode mode)
        {
            ValueTask<List<IBoardPostEntity>> DoLoad(IList<(PostStoreEntityId id, int counter)> toLoad)
            {
                return OpenSession(session =>
                {
                    var result = new List<IBoardPostEntity>();
                    var loadContext = CreateLoadContext(session, true, true);
                    var loadContextToDispose = loadContext;
                    using (loadContextToDispose)
                    {
                        foreach (var id in toLoad)
                        {
                            Api.MakeKey(loadContext.table.Session, loadContext.table, id.id.Id, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(loadContext.table.Session, loadContext.table, SeekGrbit.SeekEQ))
                            {
                                var r = LoadBoardEntity(session, ref loadContext, mode);
                                if (r is IBoardPostLight l)
                                {
                                    l.Counter = id.counter;
                                }
                                result.Add(r);
                            }
                        }
                    }
                    return result;
                });
            }

            if (ids == null) throw new ArgumentNullException(nameof(ids));
            mode = (mode ?? DefaultLoadMode).Clone();
            mode.RetrieveCounterNumber = false;

            IList<IBoardPostEntity> loaded;

            if (ids.Count > 4)
            {
                var toLoad = ids.DistributeToProcess(3);
                var tasks = toLoad.Select(DoLoad).ToArray();
                var taskResults = await CoreTaskHelper.WhenAllValueTasks(tasks);
                loaded = taskResults.SelectMany(r => r).ToList();
            }
            else
            {
                loaded = await DoLoad(ids);
            }

            if (mode.EntityLoadMode == PostStoreEntityLoadMode.Full)
            {
                await FillChildrenInLoadResult(loaded);
            }

            return loaded;
        }

        /// <summary>
        /// Режим загрузки по умолчанию.
        /// </summary>
        protected static readonly PostStoreLoadMode DefaultLoadMode = new PostStoreLoadMode()
        {
            EntityLoadMode = PostStoreEntityLoadMode.EntityOnly,
            RetrieveCounterNumber = false
        };
        /// <summary>
        /// Режим загрузки по умолчанию.
        /// </summary>
        protected static readonly PostStoreLoadMode FullLoadMode = new PostStoreLoadMode()
        {
            EntityLoadMode = PostStoreEntityLoadMode.Full,
            RetrieveCounterNumber = false
        };
    }
}