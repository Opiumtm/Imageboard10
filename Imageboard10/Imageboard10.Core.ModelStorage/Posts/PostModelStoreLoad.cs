using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class PostModelStore
    {
        private IBoardPostEntity LoadBareEntity(EsentTable table, IDictionary<string, JET_COLUMNID> colids)
        {
            var entityType = (PostStoreEntityType) (Api.RetrieveColumnAsByte(table.Session, table, colids[ColumnNames.EntityType]) ?? 0);
            var genEntityType = ToGenericEntityType(entityType);
            var links = LoadEntityLinks(table, colids, genEntityType);
            var dirParent = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.DirectParentId]);
            return new PostModelStoreBareEntity()
            {
                EntityType = entityType,
                Link = links.link,
                ParentLink = links.parentLik,
                Thumbnail = LoadThumbnail(table, colids),
                Subject = Api.RetrieveColumnAsString(table.Session, table, colids[ColumnNames.Subject]),
                StoreId = new PostStoreEntityId() { Id = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.Id]) ?? -1},
                StoreParentId = dirParent != null ? (PostStoreEntityId?)(new PostStoreEntityId() { Id = dirParent.Value }) : null
            };
        }

        private EsentTable GetPostNumberSortTable(IEsentSession session, PostStoreEntityId directParentId, out JET_COLUMNID colid)
        {
            var columns = new[]
            {
                new JET_COLUMNDEF() { coltyp = JET_coltyp.Long, grbit = ColumndefGrbit.TTKey },
            };
            var columnids = new JET_COLUMNID[columns.Length];
            JET_TABLEID tableid;
            Api.JetOpenTempTable(session.Session, columns, columns.Length, TempTableGrbit.None, out tableid, columnids);
            var tempTable = new EsentTable(session.Session, tableid);
            try
            {
                using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                {
                    var seqcolid = table.GetColumnid(ColumnNames.SequenceNumber);
                    Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
                    Api.MakeKey(table.Session, table, directParentId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnStartLimit);
                    if (Api.TrySeek(table.Session, table, SeekGrbit.SeekGE))
                    {
                        Api.MakeKey(table.Session, table, directParentId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnEndLimit);
                        if (Api.TrySetIndexRange(table.Session, table, SetIndexRangeGrbit.RangeUpperLimit))
                        {
                            do
                            {
                                var seqId = Api.RetrieveColumnAsInt32(table.Session, table, seqcolid, RetrieveColumnGrbit.RetrieveFromIndex);
                                if (seqId != null)
                                {
                                    using (var update = tempTable.Update(JET_prep.Insert))
                                    {
                                        Api.SetColumn(tempTable.Session, tempTable, columnids[0], seqId.Value);
                                        update.Save();
                                    }
                                }
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                }
                colid = columnids[0];
                return tempTable;
            }
            catch
            {
                tempTable.Dispose();
                throw;
            }
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

        private (ILink link, ILink parentLik) LoadEntityLinks(EsentTable table, IDictionary<string, JET_COLUMNID> colids, GenericPostStoreEntityType genEntityType)
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
            return (link, parentLink);
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
    }
}