using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.ModelStorage.UnitTests;
using Imageboard10.Core.Utility;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Хранилище постов.
    /// </summary>
    public partial class PostModelStore
    {
        protected async Task<IList<PostStoreEntityId>> DoDeleteEntitiesList(IEsentSession session, IEnumerable<PostStoreEntityId> toDelete)
        {
            var result = new List<PostStoreEntityId>();

            async Task Delete(PostStoreEntityId[] toDeletePart)
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.None))
                    {
                        using (var accTable = session.OpenTable(AccessLogTableName, OpenTableGrbit.None))
                        {
                            using (var mediaTable = session.OpenTable(MediaFilesTableName, OpenTableGrbit.None))
                            {
                                foreach (var id in toDeletePart)
                                {
                                    Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                                    if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                                    {
                                        Api.JetDelete(table.Session, table);
                                        result.Add(id);
                                    }
                                    Api.JetSetCurrentIndex(accTable.Session, accTable.Table, GetIndexName(AccessLogTableName, nameof(AccessLogIndexes.EntityId)));
                                    Api.MakeKey(accTable.Session, accTable, id.Id, MakeKeyGrbit.NewKey);
                                    if (Api.TrySeek(accTable.Session, accTable, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                                    {
                                        do
                                        {
                                            Api.JetDelete(accTable.Session, accTable);
                                        } while (Api.TryMoveNext(accTable.Session, accTable));
                                    }
                                    Api.JetSetCurrentIndex(mediaTable.Session, mediaTable.Table, GetIndexName(MediaFilesTableName, nameof(MediaFilesIndexes.EntityReferences)));
                                    Api.MakeKey(mediaTable.Session, mediaTable, id.Id, MakeKeyGrbit.NewKey);
                                    if (Api.TrySeek(mediaTable.Session, mediaTable, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                                    {
                                        do
                                        {
                                            Api.JetDelete(mediaTable.Session, mediaTable);
                                        } while (Api.TryMoveNext(mediaTable.Session, mediaTable));
                                    }
                                }
                            }
                        }
                    }
                    return true;
                }, 1);
            }

            var split = toDelete.SplitSet(20).Select(s => s.ToArray());
            foreach (var part in split)
            {
                await Delete(part);
            }
            return result;
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

        /// <summary>
        /// Получить информацию о треде или каталоге из ссылки.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Информация о посте.</returns>
        protected virtual (string boardId, int sequenceId) ExtractBoardPageLinkData(ILink link)
        {
            switch (link)
            {
                case BoardPageLink l:
                    return (l.Board, l.Page);
                case BoardLink l:
                    return (l.Board, 0);
                default:
                    throw new ArgumentException($"Невозможно определить информацию о странице доски {link.GetLinkHash()}");
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
            yield return UnitTestStoreFlags.ShouldFail;
        }

        /// <summary>
        /// Получить информацию для поиска.
        /// </summary>
        /// <param name="entityType">Тип сущности.</param>
        /// <param name="link">Ссылка.</param>
        /// <returns>Результат.</returns>
        protected virtual (string boardId, int sequenceId)? ExtractLinkKey(PostStoreEntityType entityType, ILink link)
        {
            if (link is IEngineLink el)
            {
                if (string.Equals(el.Engine, EngineId, StringComparison.OrdinalIgnoreCase))
                {
                    switch (entityType)
                    {
                        case PostStoreEntityType.Post:
                        case PostStoreEntityType.ThreadPreviewPost:
                        case PostStoreEntityType.CatalogPost:
                            switch (link)
                            {
                                case PostLink l:
                                    return (l.Board, l.PostNum);
                                case ThreadLink l:
                                    return (l.Board, l.OpPostNum);
                            }
                            break;
                        case PostStoreEntityType.Thread:
                        case PostStoreEntityType.ThreadPreview:
                            switch (link)
                            {
                                case ThreadLink l:
                                    return (l.Board, l.OpPostNum);
                            }
                            break;
                        case PostStoreEntityType.Catalog:
                            switch (link)
                            {
                                case CatalogLink l:
                                    return (l.Board, (int)l.SortMode);
                            }
                            break;
                        case PostStoreEntityType.BoardPage:
                            switch (link)
                            {
                                case BoardPageLink l:
                                    return (l.Board, l.Page);
                                case BoardLink l:
                                    return (l.Board, 0);
                            }
                            break;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Общий вид сущности
        /// </summary>
        protected enum GenericPostStoreEntityType
        {
            /// <summary>
            /// Пост.
            /// </summary>
            Post,
            /// <summary>
            /// Тред.
            /// </summary>
            Thread,
            /// <summary>
            /// Каталог.
            /// </summary>
            Catalog,
            /// <summary>
            /// Страница доски.
            /// </summary>
            BoardPage,
            /// <summary>
            /// Неизвестен.
            /// </summary>
            Unknown
        }

        /// <summary>
        /// Получить обобщённый тип сущности.
        /// </summary>
        /// <param name="entityType">Тип сущности.</param>
        /// <returns>Обобщённый тип сущности.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected GenericPostStoreEntityType ToGenericEntityType(PostStoreEntityType entityType)
        {
            switch (entityType)
            {
                case PostStoreEntityType.BoardPage:
                    return GenericPostStoreEntityType.BoardPage;
                case PostStoreEntityType.Thread:
                case PostStoreEntityType.ThreadPreview:
                    return GenericPostStoreEntityType.Thread;
                case PostStoreEntityType.Catalog:
                    return GenericPostStoreEntityType.Catalog;
                case PostStoreEntityType.Post:
                case PostStoreEntityType.ThreadPreviewPost:
                case PostStoreEntityType.CatalogPost:
                    return GenericPostStoreEntityType.Post;
                default:
                    return GenericPostStoreEntityType.Unknown;
            }
        }

        private static class GenericEntityTypesMapping
        {
            public static readonly PostStoreEntityType[] BoardPage = { PostStoreEntityType.BoardPage };
            public static readonly PostStoreEntityType[] Thread = { PostStoreEntityType.Thread, PostStoreEntityType.ThreadPreview };
            public static readonly PostStoreEntityType[] Catalog = { PostStoreEntityType.Catalog };
            public static readonly PostStoreEntityType[] Post = { PostStoreEntityType.Post, PostStoreEntityType.CatalogPost, PostStoreEntityType.ThreadPreviewPost };
            public static readonly PostStoreEntityType[] Unknown = { };
        }

        /// <summary>
        /// Получить типы сущности.
        /// </summary>
        /// <param name="entityType">Обобщённый сущности.</param>
        /// <returns>Типы сущности.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected PostStoreEntityType[] ToEntityTypes(GenericPostStoreEntityType entityType)
        {
            switch (entityType)
            {
                case GenericPostStoreEntityType.BoardPage:
                    return GenericEntityTypesMapping.BoardPage;
                case GenericPostStoreEntityType.Catalog:
                    return GenericEntityTypesMapping.Catalog;
                case GenericPostStoreEntityType.Thread:
                    return GenericEntityTypesMapping.Thread;
                case GenericPostStoreEntityType.Post:
                    return GenericEntityTypesMapping.Post;
                default:
                    return GenericEntityTypesMapping.Unknown;
            }
        }

        /// <summary>
        /// Получить информацию для поиска.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Результат.</returns>
        protected virtual (string boardId, int sequenceId, GenericPostStoreEntityType entityType)? ExtractLinkKey(ILink link)
        {
            if (link is IEngineLink el)
            {
                if (string.Equals(el.Engine, EngineId, StringComparison.OrdinalIgnoreCase))
                {
                    switch (link)
                    {
                        case PostLink l:
                            return (l.Board, l.PostNum, GenericPostStoreEntityType.Post);
                        case ThreadLink l:
                            return (l.Board, l.OpPostNum, GenericPostStoreEntityType.Thread);
                        case CatalogLink l:
                            return (l.Board, (int)l.SortMode, GenericPostStoreEntityType.Catalog);
                        case BoardPageLink l:
                            return (l.Board, l.Page, GenericPostStoreEntityType.BoardPage);
                        case BoardLink l:
                            return (l.Board, 0, GenericPostStoreEntityType.BoardPage);
                    }
                }
            }
            return null;
        }

        private class PostStoreEntityIdSearchResult : IPostStoreEntityIdSearchResult
        {
            public ILink Link { get; set; }
            public PostStoreEntityId Id { get; set; }
            public PostStoreEntityType EntityType { get; set; }
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
            return FindAllChildren(table, new[] { parent });
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
            await OpenSessionAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.Updatable))
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
                }, 1, CommitTransactionGrbit.LazyFlush);
                return Nothing.Value;
            });
        }

        private class LinkWithStoreId : ILinkWithStoreId
        {
            public PostStoreEntityId Id { get; set; }
            public ILink Link { get; set; }
        }

        /// <summary>
        /// Сформировать ссылку.
        /// </summary>
        /// <param name="entityType">Тип сущности.</param>
        /// <param name="boardId">Идентификатор доски.</param>
        /// <param name="sequenceId">Номер в последовательности.</param>
        /// <param name="parentSequenceId">Родительский номер в последовательности.</param>
        /// <returns></returns>
        protected virtual ILink ConstructLink(PostStoreEntityType entityType, string boardId, int sequenceId, int? parentSequenceId)
        {
            switch (entityType)
            {
                case PostStoreEntityType.Post:
                case PostStoreEntityType.CatalogPost:
                case PostStoreEntityType.ThreadPreviewPost:
                    return new PostLink()
                    {
                        Engine = EngineId,
                        Board = boardId,
                        OpPostNum = parentSequenceId ?? 0,
                        PostNum = sequenceId
                    };
                case PostStoreEntityType.Thread:
                case PostStoreEntityType.ThreadPreview:
                    return new ThreadLink()
                    {
                        Engine = EngineId,
                        Board = boardId,
                        OpPostNum = sequenceId
                    };
                case PostStoreEntityType.BoardPage:
                    return new BoardPageLink()
                    {
                        Engine = EngineId,
                        Board = boardId,
                        Page = sequenceId
                    };
                case PostStoreEntityType.Catalog:
                    return new CatalogLink()
                    {
                        Engine = EngineId,
                        Board = boardId,
                        SortMode = (BoardCatalogSort)sequenceId
                    };
                default:
                    return null;
            }
        }

        private (PostStoreEntityType entityType, string boardId, int sequenceId, int? parentSequenceId) ExtractLinkData(EsentTable table, IDictionary<string, JET_COLUMNID> colids)
        {
            return
                (
                    entityType: (PostStoreEntityType)(Api.RetrieveColumnAsByte(table.Session, table.Table, colids[ColumnNames.EntityType]) ?? 0),
                    boardId: Api.RetrieveColumnAsString(table.Session, table, colids[ColumnNames.BoardId]),
                    sequenceId: Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.SequenceNumber]) ?? 0,
                    parentSequenceId: Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.ParentSequenceNumber])
                );
        }

        private ILink GetLinkAtCurrentPosition(EsentTable table, IDictionary<string, JET_COLUMNID> colids)
        {
            (var entityType, var boardId, var sequenceId, var parentSequenceId) = ExtractLinkData(table, colids);
            return ConstructLink(entityType, boardId, sequenceId, parentSequenceId);
        }

        private int CountDirectParentWithFlag(EsentTable table, PostStoreEntityId directParentId, Guid flag)
        {
            Api.JetSetCurrentIndex(table.Session, table.Table, GetIndexName(TableName, nameof(Indexes.DirectParentFlags)));
            Api.MakeKey(table.Session, table, directParentId.Id, MakeKeyGrbit.NewKey);
            Api.MakeKey(table.Session, table, flag, MakeKeyGrbit.None);
            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
            {
                int r;
                Api.JetIndexRecordCount(table.Session, table, out r, int.MaxValue);
                return r;
            }
            return 0;
        }

        private int CountDirectParent(EsentTable table, PostStoreEntityId directParentId)
        {
            Api.JetSetCurrentIndex(table.Session, table.Table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
            Api.MakeKey(table.Session, table, directParentId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnStartLimit);
            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekGE))
            {
                Api.MakeKey(table.Session, table, directParentId.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnEndLimit);
                if (Api.TrySetIndexRange(table.Session, table, SetIndexRangeGrbit.RangeUpperLimit))
                {
                    int r;
                    Api.JetIndexRecordCount(table.Session, table, out r, int.MaxValue);
                    return r;
                }
            }
            return 0;
        }

        private int CountDirectParentWithFlag(IEsentSession session, PostStoreEntityId directParentId, Guid flag)
        {
            using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
            {
                return CountDirectParentWithFlag(table, directParentId, flag);
            }
        }

        private int CountDirectParent(IEsentSession session, PostStoreEntityId directParentId)
        {
            using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
            {
                return CountDirectParent(table, directParentId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GotoEntityId(EsentTable table, PostStoreEntityId id)
        {
            Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
            return Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ);
        }
    }
}