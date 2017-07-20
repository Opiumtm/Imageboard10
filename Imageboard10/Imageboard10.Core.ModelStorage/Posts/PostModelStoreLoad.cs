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
            public EsentTable table;
            public IDictionary<string, JET_COLUMNID> colids;
            public EsentTable? mediaTable;
            public IDictionary<string, JET_COLUMNID> mediaColids;
            public EsentTable? quotesTable;
            public IDictionary<string, JET_COLUMNID> quoteColids;

            public void Dispose()
            {
                table.Dispose();
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
                r.table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly);
                r.colids = r.table.GetColumnDictionary();
                if (openMedia)
                {
                    r.mediaTable = session.OpenTable(MediaFilesTableName, OpenTableGrbit.ReadOnly);
                    r.mediaColids = r.mediaTable.Value.GetColumnDictionary();
                    Api.JetSetCurrentIndex(r.mediaTable.Value.Session, r.mediaTable.Value, GetIndexName(MediaFilesTableName, nameof(MediaFilesIndexes.Sequences)));
                }
                if (openQuotes)
                {
                    r.quotesTable = session.OpenTable(TableName, OpenTableGrbit.ReadOnly);
                    r.quoteColids = r.quotesTable.Value.GetColumnDictionary();
                    Api.JetSetCurrentIndex(r.quotesTable.Value.Session, r.quotesTable.Value, GetIndexName(TableName, nameof(Indexes.QuotedPosts)));
                }
            }
            catch
            {
                r.Dispose();
                throw;
            }
            return r;
        }

        private List<ILink> LoadQuotesForPost(EsentTable quotesTable, IDictionary<string, JET_COLUMNID> quoteColids, bool setIndex, ref BasicEntityInfo bi)
        {
            var r = new HashSet<ILink>(BoardLinkEqualityComparer.Instance);
            if (bi.parentEntityId != null && bi.parentSequenceId != null)
            {
                if (setIndex)
                {
                    Api.JetSetCurrentIndex(quotesTable.Session, quotesTable, GetIndexName(TableName, nameof(Indexes.QuotedPosts)));
                }
                Api.MakeKey(quotesTable.Session, quotesTable, bi.parentEntityId.Value.Id, MakeKeyGrbit.NewKey);
                Api.MakeKey(quotesTable.Session, quotesTable, bi.sequenceId, MakeKeyGrbit.None);
                if (Api.TrySeek(quotesTable.Session, quotesTable, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                {
                    do
                    {
                        var seqId = Api.RetrieveColumnAsInt32(quotesTable.Session, quotesTable, quoteColids[ColumnNames.SequenceNumber]);
                        if (seqId != null)
                        {
                            r.Add(new PostLink()
                            {
                                Engine = EngineId,
                                Board = bi.boardId,
                                OpPostNum = bi.parentSequenceId.Value,
                                PostNum = seqId.Value
                            });
                        }
                    } while (Api.TryMoveNext(quotesTable.Session, quotesTable));
                }
            }
            return r.OrderBy(l => l, BoardLinkComparer.Instance).ToList();
        }

        private List<IPostMedia> LoadPostMedia(EsentTable mediaTable, IDictionary<string, JET_COLUMNID> mediaColids, ref BasicEntityInfo bi, bool setIndex)
        {
            var r = new List<IPostMedia>();
            if (setIndex)
            {
                Api.JetSetCurrentIndex(mediaTable.Session, mediaTable, GetIndexName(MediaFilesTableName, nameof(MediaFilesIndexes.Sequences)));
            }
            Api.MakeKey(mediaTable.Session, mediaTable, bi.entityId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnStartLimit);
            if (Api.TrySeek(mediaTable.Session, mediaTable.Table, SeekGrbit.SeekGE))
            {
                Api.MakeKey(mediaTable.Session, mediaTable, bi.entityId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnEndLimit);
                if (Api.TrySetIndexRange(mediaTable.Session, mediaTable, SetIndexRangeGrbit.RangeUpperLimit))
                {
                    do
                    {
                        var m = ObjectSerializationService.Deserialize(Api.RetrieveColumn(mediaTable.Session, mediaTable, mediaColids[MediaFilesColumnNames.MediaData])) as IPostMedia;
                        if (m != null)
                        {
                            r.Add(m);
                        }
                    } while (Api.TryMoveNext(mediaTable.Session, mediaTable));
                }
            }
            return r;
        }

        private void LoadBasicInfo(EsentTable table, IDictionary<string, JET_COLUMNID> colids, ref BasicEntityInfo bi)
        {
            bi.entityType = (PostStoreEntityType)(Api.RetrieveColumnAsByte(table.Session, table, colids[ColumnNames.EntityType]) ?? 0);
            bi.genEntityType = ToGenericEntityType(bi.entityType);
            (bi.link, bi.parentLink, bi.sequenceId, bi.boardId, bi.parentSequenceId) = LoadEntityLinks(table, colids, bi.genEntityType);
            var dirParent = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.DirectParentId]);
            bi.entityId = new PostStoreEntityId() { Id = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.Id]) ?? -1 };
            bi.parentEntityId = dirParent != null ? (PostStoreEntityId?)(new PostStoreEntityId() { Id = dirParent.Value }) : null;
        }

        private IBoardPostEntity LoadLinkOnly(EsentTable table, IDictionary<string, JET_COLUMNID> colids)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            LoadBasicInfo(table, colids, ref bi);
            return new PostModelStoreBareEntityLink()
            {
                EntityType = bi.entityType,
                Link = bi.link,
                ParentLink = bi.parentLink,
                StoreId = bi.entityId,
                StoreParentId = bi.parentEntityId
            };
        }

        private void SetBareEntityData(EsentTable table, IDictionary<string, JET_COLUMNID> colids, PostModelStoreBareEntity data, ref BasicEntityInfo bi)
        {
            LoadBasicInfo(table, colids, ref bi);
            data.EntityType = bi.entityType;
            data.Link = bi.link;
            data.ParentLink = bi.parentLink;
            data.Thumbnail = LoadThumbnail(table, colids);
            data.Subject = Api.RetrieveColumnAsString(table.Session, table, colids[ColumnNames.Subject]);
            data.StoreId = bi.entityId;
            data.StoreParentId = bi.parentEntityId;
        }

        private IBoardPostEntity LoadBareEntity(EsentTable table, IDictionary<string, JET_COLUMNID> colids)
        {
            var r = new PostModelStoreBareEntity();
            BasicEntityInfo bi = default(BasicEntityInfo);
            SetBareEntityData(table, colids, r, ref bi);
            return r;
        }

        private void SetPostLightData(IEsentSession session, EsentTable table, IDictionary<string, JET_COLUMNID> colids, bool getPostCount, PostModelStorePostLight data, ref BasicEntityInfo bi)
        {
            SetBareEntityData(table, colids, data, ref bi);
            data.BoardSpecificDate = Api.RetrieveColumnAsString(table.Session, table, colids[ColumnNames.BoardSpecificDate]);
            data.Counter = getPostCount && bi.parentEntityId != null ? GetPostCounterNumber(session, bi.parentEntityId.Value, bi.sequenceId) ?? 0 : 0;
            data.Date = FromUtcToOffset(Api.RetrieveColumnAsDateTime(table.Session, table.Table, colids[ColumnNames.Date])) ?? DateTimeOffset.MinValue;
            data.Flags = EnumMultivalueColumn<GuidColumnValue>(table, colids[ColumnNames.Flags]).Where(g => g?.Value != null).Select(g => g.Value.Value).Distinct().ToList();
            data.TagsSet = EnumMultivalueColumn<StringColumnValue>(table, colids[ColumnNames.ThreadTags])
                .Where(t => !string.IsNullOrEmpty(t?.Value))
                .Select(t => t.Value)
                .Distinct()
                .OrderBy(t => t, StringComparer.CurrentCulture)
                .ToArray();
            data.LLikes = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.Likes]);
            data.LDislikes = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.Dislikes]);
        }

        private IBoardPostEntity LoadPostLight(IEsentSession session, EsentTable table, IDictionary<string, JET_COLUMNID> colids, bool getPostCount)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            var r = new PostModelStorePostLight();
            SetPostLightData(session, table, colids, getPostCount, r, ref bi);
            return r;
        }

        private void SetPostData(IEsentSession session, ref LoadPostDataContext loadContext, bool getPostCount, PostModelStorePost data, ref BasicEntityInfo bi)
        {
            SetPostLightData(session, loadContext.table, loadContext.colids, getPostCount, data, ref bi);
            var table = loadContext.table;
            var colids = loadContext.colids;
            var posterName = Api.RetrieveColumnAsString(table.Session, table, colids[ColumnNames.PosterName]);
            var otherData = DeserializeDataContract<PostOtherData>(Api.RetrieveColumn(table.Session, table, colids[ColumnNames.OtherDataBinary]));
            otherData?.FillPostData(data, LinkSerialization, posterName);
            data.Comment = ObjectSerializationService.Deserialize(Api.RetrieveColumn(table.Session, table, colids[ColumnNames.Document])) as IPostDocument;
            data.LoadedTime = FromUtcToOffset(Api.RetrieveColumnAsDateTime(table.Session, table, colids[ColumnNames.LoadedTime])) ?? DateTimeOffset.MinValue;
            data.MediaFiles = LoadPostMedia(loadContext.mediaTable ?? throw new InvalidOperationException("Таблица медиафайлов не открыта"), loadContext.mediaColids, ref bi, false);
            data.Quotes = LoadQuotesForPost(loadContext.quotesTable ?? throw new InvalidOperationException("Таблица цитат не открыта"), loadContext.quoteColids, false, ref bi);
        }

        private IBoardPostEntity LoadPost(IEsentSession session, ref LoadPostDataContext loadContext, bool getPostCount)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            var r = new PostModelStorePost();
            SetPostData(session, ref loadContext, getPostCount, r, ref bi);
            return r;
        }

        private void SetPostCollectionData(EsentTable table, IDictionary<string, JET_COLUMNID> colids, PostModelStoreCollection data, ref BasicEntityInfo bi)
        {
            SetBareEntityData(table, colids, data, ref bi);
            data.Etag = Api.RetrieveColumnAsString(table.Session, table, colids[ColumnNames.Etag]);
            data.Info = ObjectSerializationService.Deserialize(Api.RetrieveColumn(table.Session, table, colids[ColumnNames.OtherDataBinary])) as IBoardPostCollectionInfoSet;
            data.Stage = Api.RetrieveColumnAsByte(table.Session, table, colids[ColumnNames.ChildrenLoadStage]) ?? 0;
        }

        private IBoardPostEntity LoadPostCollection(EsentTable table, IDictionary<string, JET_COLUMNID> colids)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            var r = new PostModelStoreCollection();
            SetPostCollectionData(table, colids, r, ref bi);
            return r;
        }

        private void SetThreadPreviewData(EsentTable table, IDictionary<string, JET_COLUMNID> colids, PostModelStoreThreadPreview data, ref BasicEntityInfo bi)
        {
            SetPostCollectionData(table, colids, data, ref bi);
            var counts = ReadThreadPreviewCounts(Api.RetrieveColumn(table.Session, table, colids[ColumnNames.PreviewCounts]));
            data.ImageCount = counts.imageCount;
            data.Omit = counts.omit;
            data.OmitImages = counts.omitImages;
            data.ReplyCount = counts.replyCount;
        }

        private IBoardPostEntity LoadThreadPreview(EsentTable table, IDictionary<string, JET_COLUMNID> colids)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            var r = new PostModelStoreThreadPreview();
            SetThreadPreviewData(table, colids, r, ref bi);
            return r;
        }

        private void SetThreadCollectionData(EsentTable table, IDictionary<string, JET_COLUMNID> colids, PostModelStoreThreadCollection data, ref BasicEntityInfo bi)
        {
            SetBareEntityData(table, colids, data, ref bi);
            data.Etag = Api.RetrieveColumnAsString(table.Session, table, colids[ColumnNames.Etag]);
            data.Info = ObjectSerializationService.Deserialize(Api.RetrieveColumn(table.Session, table, colids[ColumnNames.OtherDataBinary])) as IBoardPostCollectionInfoSet;
            data.Stage = Api.RetrieveColumnAsByte(table.Session, table, colids[ColumnNames.ChildrenLoadStage]) ?? 0;
        }

        private IBoardPostEntity LoadThreadCollection(EsentTable table, IDictionary<string, JET_COLUMNID> colids)
        {
            BasicEntityInfo bi = default(BasicEntityInfo);
            var r = new PostModelStoreThreadCollection();
            SetThreadCollectionData(table, colids, r, ref bi);
            return r;
        }

        private int? GetPostCounterNumber(IEsentSession session, PostStoreEntityId directParentId, int sequenceNumber)
        {
            using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
            {
                var colid = table.GetColumnid(ColumnNames.SequenceNumber);
                int cnt = 0;
                Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
                Api.MakeKey(table.Session, table, directParentId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnStartLimit);
                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekGE))
                {
                    Api.MakeKey(table.Session, table, directParentId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnEndLimit);
                    if (Api.TrySetIndexRange(table.Session, table, SetIndexRangeGrbit.RangeUpperLimit))
                    {
                        do
                        {
                            cnt++;
                            var seqId = Api.RetrieveColumnAsInt32(table.Session, table, colid, RetrieveColumnGrbit.RetrieveFromIndex) ?? 0;
                            if (seqId == sequenceNumber)
                            {
                                return cnt;
                            }
                            if (seqId > sequenceNumber)
                            {
                                return null;
                            }
                        } while (Api.TryMoveNext(table.Session, table));
                    }
                }
                return null;
            }
        }

        private IPostMediaWithSize LoadThumbnail(EsentTable table, IDictionary<string, JET_COLUMNID> colids)
        {
            var bytes = Api.RetrieveColumn(table.Session, table, colids[ColumnNames.Thumbnail]);
            return ObjectSerializationService.Deserialize(bytes) as IPostMediaWithSize;
        }

        private (ILink link, ILink parentLink, int sequenceId, string boardId, int? parentSequenceId) LoadEntityLinks(EsentTable table, IDictionary<string, JET_COLUMNID> colids, GenericPostStoreEntityType genEntityType)
        {
            var boardId = Api.RetrieveColumnAsString(table.Session, table, colids[ColumnNames.BoardId]) ?? "";
            var seqId = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.SequenceNumber]) ?? 0;
            var parentSeqId = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.ParentSequenceNumber]);
            ILink link, parentLink;
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
                        SortMode = (BoardCatalogSort)seqId
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
            return (link, parentLink, seqId, boardId, parentSeqId);
        }

        private (Guid? lastAccessEnty, DateTime? lastAccessUtc) GetLastAccess(IEsentSession session, PostStoreEntityId id)
        {
            DateTime? lastAccessUtc = null;
            Guid? lastAcessEntry = null;
            using (var accTable = session.OpenTable(AccessLogTableName, OpenTableGrbit.ReadOnly))
            {
                Api.JetSetCurrentIndex(accTable.Session, accTable, GetIndexName(AccessLogTableName, nameof(AccessLogIndexes.EntityIdAndAccessTime)));
                Api.MakeKey(accTable.Session, accTable, id.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnEndLimit);
                if (Api.TrySeek(accTable.Session, accTable, SeekGrbit.SeekLE))
                {
                    lastAccessUtc = Api.RetrieveColumnAsDateTime(accTable.Session, accTable, accTable.GetColumnid(AccessLogColumnNames.AccessTime));
                    lastAcessEntry = Api.RetrieveColumnAsGuid(accTable.Session, accTable, accTable.GetColumnid(AccessLogColumnNames.Id));
                }
            }
            return (lastAcessEntry, lastAccessUtc);
        }

        private ILink LoadLastPostOnServer(EsentTable table, IDictionary<string, JET_COLUMNID> colids)
        {
            var boardId = Api.RetrieveColumnAsString(table.Session, table, colids[ColumnNames.BoardId]) ?? "";
            var seqId = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.SequenceNumber]) ?? 0;
            var id = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.LastPostLinkOnServer]) ?? seqId;
            return new PostLink() { Engine = EngineId, Board = boardId, OpPostNum = seqId, PostNum = id };
        }

        private ILink LoadThreadLastLoadedPost(IEsentSession session, PostStoreEntityId directParentId)
        {
            using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
            {
                Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
                Api.MakeKey(table.Session, table, directParentId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnEndLimit);
                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekLE))
                {
                    var links = LoadEntityLinks(table, table.GetColumnDictionary(), GenericPostStoreEntityType.Post);
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

        private IBoardPostStoreAccessInfo LoadAccessInfo(IEsentSession session, EsentTable table, IDictionary<string, JET_COLUMNID> colids)
        {
            var bareEntity = LoadBareEntity(table, colids);
            DateTime? lastAccessUtc = null;
            Guid? lastAcessEntry = null;
            if (bareEntity.StoreId != null)
            {
                (lastAcessEntry, lastAccessUtc) = GetLastAccess(session, bareEntity.StoreId.Value);
            }
            DateTimeOffset? lastAccess = FromUtcToOffset(lastAccessUtc);
            var flags = EnumMultivalueColumn<GuidColumnValue>(table, colids[ColumnNames.Flags]).Where(g => g?.Value != null).ToHashSet(g => g?.Value ?? Guid.Empty);
            return new PostStoreAccessInfo()
            {
                LogEntryId = lastAcessEntry,
                Etag = Api.RetrieveColumnAsString(table.Session, table, colids[ColumnNames.Etag]),
                Entity = bareEntity,
                AccessTime = lastAccess,
                IsArchived = flags.Any(f => f == PostCollectionFlags.IsArchived),
                IsFavorite = flags.Any(f => f == PostCollectionFlags.IsFavorite),
                LastPost = bareEntity.EntityType == PostStoreEntityType.Thread ? LoadLastPostOnServer(table, colids) : null,
                LastLoadedPost = bareEntity.EntityType == PostStoreEntityType.Thread && bareEntity.StoreId != null ? LoadThreadLastLoadedPost(session, bareEntity.StoreId.Value) : null,
                NumberOfLoadedPosts = bareEntity.EntityType == PostStoreEntityType.Thread && bareEntity.StoreId != null ? CountDirectParent(session, bareEntity.StoreId.Value) : 0,
                NumberOfPosts = bareEntity.EntityType == PostStoreEntityType.Thread ? Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.NumberOfPostsOnServer]) : null,
                NumberOfReadPosts = bareEntity.EntityType == PostStoreEntityType.Thread ? Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.NumberOfReadPosts]) : null,
                LastDownload = FromUtcToOffset(Api.RetrieveColumnAsDateTime(table.Session, table, colids[ColumnNames.LoadedTime])),
                LastUpdate = bareEntity.EntityType == PostStoreEntityType.Thread ? FromUtcToOffset(Api.RetrieveColumnAsDateTime(table.Session, table, colids[ColumnNames.LastServerUpdate])) : null                
            };
        }

        private IBoardPostEntity LoadPost(IEsentSession session, ref LoadPostDataContext loadContext, PostStoreLoadMode loadMode)
        {
            switch (loadMode.EntityLoadMode)
            {
                case PostStoreEntityLoadMode.LinkOnly:
                    return LoadLinkOnly(loadContext.table, loadContext.colids);
                case PostStoreEntityLoadMode.EntityOnly:
                    return LoadBareEntity(loadContext.table, loadContext.colids);
                case PostStoreEntityLoadMode.Light:
                    return LoadPostLight(session, loadContext.table, loadContext.colids, loadMode.RetrieveCounterNumber);
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
                    return LoadLinkOnly(loadContext.table, loadContext.colids);
                case PostStoreEntityLoadMode.EntityOnly:
                    return LoadBareEntity(loadContext.table, loadContext.colids);
                case PostStoreEntityLoadMode.Light:
                case PostStoreEntityLoadMode.Full:
                    return LoadPostCollection(loadContext.table, loadContext.colids);
                default:
                    return null;
            }
        }

        private IBoardPostEntity LoadThreadPreview(IEsentSession session, ref LoadPostDataContext loadContext, PostStoreLoadMode loadMode)
        {
            switch (loadMode.EntityLoadMode)
            {
                case PostStoreEntityLoadMode.LinkOnly:
                    return LoadLinkOnly(loadContext.table, loadContext.colids);
                case PostStoreEntityLoadMode.EntityOnly:
                    return LoadBareEntity(loadContext.table, loadContext.colids);
                case PostStoreEntityLoadMode.Light:
                case PostStoreEntityLoadMode.Full:
                    return LoadThreadPreview(loadContext.table, loadContext.colids);
                default:
                    return null;
            }
        }

        private IBoardPostEntity LoadThreadCollection(IEsentSession session, ref LoadPostDataContext loadContext, PostStoreLoadMode loadMode)
        {
            switch (loadMode.EntityLoadMode)
            {
                case PostStoreEntityLoadMode.LinkOnly:
                    return LoadLinkOnly(loadContext.table, loadContext.colids);
                case PostStoreEntityLoadMode.EntityOnly:
                    return LoadBareEntity(loadContext.table, loadContext.colids);
                case PostStoreEntityLoadMode.Light:
                case PostStoreEntityLoadMode.Full:
                    return LoadThreadCollection(loadContext.table, loadContext.colids);
                default:
                    return null;
            }
        }

        private IBoardPostEntity LoadBoardEntity(IEsentSession session, ref LoadPostDataContext loadContext, PostStoreLoadMode loadMode)
        {
            var entityType = (PostStoreEntityType) (Api.RetrieveColumnAsByte(loadContext.table.Session, loadContext.table, loadContext.colids[ColumnNames.EntityType]) ?? 0);
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
            using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
            {
                var idCol = table.GetColumnid(ColumnNames.Id);
                Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
                Api.MakeKey(table.Session, table, entityId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnStartLimit);
                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekGE))
                {
                    Api.MakeKey(table.Session, table, entityId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnEndLimit);
                    if (Api.TrySetIndexRange(table.Session, table, SetIndexRangeGrbit.RangeUpperLimit))
                    {
                        do
                        {
                            var id = Api.RetrieveColumnAsInt32(table.Session, table, idCol, RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                            if (id != null)
                            {
                                var id1 = new PostStoreEntityId() {Id = id.Value};
                                counter++;
                                children.Add((id1, counter));
                            }
                        } while (Api.TryMoveNext(table.Session, table));
                    }
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