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
using Imageboard10.Core.Models.Posts;
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.None))
                    {
                        using (var accTable = OpenAccessLogTable(session, OpenTableGrbit.None))
                        {
                            using (var mediaTable = OpenMediaFilesTable(session, OpenTableGrbit.None))
                            {
                                foreach (var id in toDeletePart)
                                {
                                    if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(id.Id)))
                                    {
                                        table.DeleteCurrentRow();
                                        result.Add(id);
                                    }
                                    accTable.Indexes.EntityIdIndex.SetAsCurrentIndex();
                                    foreach (var _ in accTable.Indexes.EntityIdIndex.Enumerate(accTable.Indexes.EntityIdIndex.CreateKey(id.Id)))
                                    {
                                        accTable.DeleteCurrentRow();
                                    }
                                    mediaTable.Indexes.EntityReferencesIndex.SetAsCurrentIndex();
                                    foreach (var _ in mediaTable.Indexes.EntityReferencesIndex.Enumerate(mediaTable.Indexes.EntityReferencesIndex.CreateKey(id.Id)))
                                    {
                                        mediaTable.DeleteCurrentRow();
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

        private bool SeekExistingEntityInSequence(PostsTable table, PostStoreEntityId directParent, int postId, out PostStoreEntityId id)
        {
            var index = table.Indexes.InThreadPostLinkIndex;
            index.SetAsCurrentIndex();
            var r = index.Find(index.CreateKey(directParent.Id, postId));
            if (!r)
            {
                id = new PostStoreEntityId() { Id = -1 };
            }
            else
            {
                id = new PostStoreEntityId()
                {
                    Id = index.Views.RetrieveIdFromIndexView.Fetch().Id
                };
            }
            return r;
        }

        private bool SeekExistingEntityOnBoard(PostsTable table, PostStoreEntityType entityType, string boardId, int sequenceId, out PostStoreEntityId id)
        {
            var index = table.Indexes.TypeAndPostIdIndex;
            index.SetAsCurrentIndex();
            var r = index.Find(index.CreateKey((byte)entityType, boardId, sequenceId));
            if (!r)
            {
                id = new PostStoreEntityId() { Id = -1 };
            }
            else
            {
                id = new PostStoreEntityId()
                {
                    Id = index.Views.RetrieveIdFromIndexView.Fetch().Id
                };
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

        private IEnumerable<(PostStoreEntityId id, PostStoreEntityId parentId)> FindAllChildren(PostsTable table, IEnumerable<PostStoreEntityId> parents)
        {
            var index = table.Indexes.ParentIdIndex;
            index.SetAsCurrentIndex();
            foreach (var id in parents.Distinct())
            {
                foreach (var cid in index.EnumerateAsRetrieveIdFromIndexView(index.CreateKey(id.Id)))
                {
                    yield return (new PostStoreEntityId() {Id = cid.Id}, id);
                }
            }
        }

        private IEnumerable<(int sequenceId, PostStoreEntityId parentId)> FindAllChildrenSeqNums(PostsTable table, IEnumerable<PostStoreEntityId> parents)
        {
            var index = table.Indexes.ParentIdIndex;
            index.SetAsCurrentIndex();
            foreach (var id in parents.Distinct())
            {
                foreach (var _ in index.Enumerate(index.CreateKey(id.Id)))
                {
                    yield return (table.Columns.SequenceNumber, id);
                }
            }
        }

        private IEnumerable<(PostStoreEntityId id, PostStoreEntityId parentId)> FindAllChildren(PostsTable table, PostStoreEntityId parent)
        {
            return FindAllChildren(table, new[] { parent });
        }

        private IEnumerable<(int sequenceId, PostStoreEntityId parentId)> FindAllChildrenSeqNums(PostsTable table, PostStoreEntityId parent)
        {
            return FindAllChildrenSeqNums(table, new[] { parent });
        }

        private IEnumerable<PostStoreEntityId> FindAllParents(PostsTable table)
        {
            var index = table.Indexes.ParentIdIndex;
            index.SetAsCurrentIndex();
            return index.EnumerateUniqueAsRetrieveIdFromIndexView().Select(item => new PostStoreEntityId() { Id = item.Id});
        }

        private async Task SetEntityChildrenLoadStatus(PostStoreEntityId id, byte status)
        {
            await OpenSessionAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = OpenPostsTable(session, OpenTableGrbit.Updatable))
                    {
                        var index = table.Indexes.PrimaryIndex;
                        if (index.Find(index.CreateKey(id.Id)))
                        {
                            table.Update.UpdateAsChildrenLoadStageView(new PostsTable.ViewValues.ChildrenLoadStageView() { ChildrenLoadStage = status });
                        }
                    }
                    return true;
                }, 1);
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

        private (PostStoreEntityType entityType, string boardId, int sequenceId, int? parentSequenceId) ExtractLinkData(PostsTable table)
        {
            return
                (
                    entityType: (PostStoreEntityType)table.Columns.EntityType,
                    boardId: table.Columns.BoardId,
                    sequenceId: table.Columns.SequenceNumber,
                    parentSequenceId: table.Columns.ParentSequenceNumber
                );
        }

        private ILink GetLinkAtCurrentPosition(PostsTable table)
        {
            (var entityType, var boardId, var sequenceId, var parentSequenceId) = ExtractLinkData(table);
            return ConstructLink(entityType, boardId, sequenceId, parentSequenceId);
        }

        private int CountDirectParentWithFlag(PostsTable table, PostStoreEntityId directParentId, Guid flag)
        {
            var index = table.Indexes.DirectParentFlagsIndex;
            index.SetAsCurrentIndex();
            return index.GetIndexRecordCount(index.CreateKey(directParentId.Id, flag));
        }

        private int CountDirectParent(PostsTable table, PostStoreEntityId directParentId)
        {
            var index = table.Indexes.InThreadPostLinkIndex;
            index.SetAsCurrentIndex();
            return index.GetIndexRecordCount(index.CreateKey(directParentId.Id));
        }

        private int CountDirectParentWithFlag(IEsentSession session, PostStoreEntityId directParentId, Guid flag)
        {
            using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
            {
                return CountDirectParentWithFlag(table, directParentId, flag);
            }
        }

        private int CountDirectParent(IEsentSession session, PostStoreEntityId directParentId)
        {
            using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
            {
                return CountDirectParent(table, directParentId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GotoEntityId(PostsTable table, PostStoreEntityId id)
        {
            return table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(id.Id));
        }

        private async ValueTask<PostStoreEntityId[]> DoQueryByFlags(IEsentSession session, PostStoreEntityType type, PostStoreEntityId? parentId, IList<Guid> havingFlags)
        {
            return await session.Run(() =>
            {
                var found = new List<HashSet<int>>();

                if (havingFlags != null && havingFlags.Count > 0)
                {
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        foreach (var flag in havingFlags)
                        {
                            if (parentId != null)
                            {
                                var index = table.Indexes.DirectParentFlagsIndex;
                                index.SetAsCurrentIndex();
                                found.Add(index.EnumerateAsRetrieveIdFromIndexView(index.CreateKey(parentId.Value.Id, flag)).Select(id => id.Id).ToHashSet());
                            }
                            else
                            {
                                var index = table.Indexes.TypeFlagsIndex;
                                index.SetAsCurrentIndex();
                                found.Add(index.EnumerateAsRetrieveIdFromIndexView(index.CreateKey((byte)type, flag)).Select(id => id.Id).ToHashSet());
                            }
                        }
                    }
                }


                if (found.Count == 0)
                {
                    goto CancelLabel;
                }

                var r = found[0];
                foreach (var f in found.Skip(1))
                {
                    r.IntersectWith(f);
                }

                return r.Select(id => new PostStoreEntityId() { Id = id }).ToArray();

                CancelLabel:
                return new PostStoreEntityId[0];
            });
        }

        private async ValueTask<List<PostStoreEntityId>> FilterByFlags(PostStoreEntityType type, PostStoreEntityId? parentId, IList<Guid> havingFlags, IList<Guid> notHavingFlags, IEsentSession session)
        {
            var bookmarks = await DoQueryByFlags(session, type, parentId, havingFlags);

            if (bookmarks == null || bookmarks.Length == 0)
            {
                return new List<PostStoreEntityId>();
            }

            return await session.Run(() =>
            {
                var result = new List<PostStoreEntityId>();

                var nhs = notHavingFlags != null ? new HashSet<Guid>(notHavingFlags) : new HashSet<Guid>();

                using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                {
                    foreach (var bm in bookmarks)
                    {
                        if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(bm.Id)))
                        {
                            if (nhs.Count > 0)
                            {
                                if (table.Columns.Flags.Values.All(v => v?.Value == null || !nhs.Contains(v?.Value ?? Guid.Empty)))
                                {
                                    result.Add(new PostStoreEntityId() { Id = table.Columns.Id });
                                }
                            }
                            else
                            {
                                result.Add(new PostStoreEntityId() { Id = table.Columns.Id });
                            }
                        }
                    }
                }
                return result;
            });
        }

        private async Task CleanChildren(ICollection<PostStoreEntityId> parents)
        {
            var toDelete = await OpenSession(session =>
            {
                var r = new HashSet<PostStoreEntityId>(PostStoreEntityIdEqualityComparer.Instance);
                using (var table = OpenPostsTable(session, OpenTableGrbit.Updatable))
                {
                    var index = table.Indexes.ParentIdIndex;
                    index.SetAsCurrentIndex();
                    foreach (var p in parents)
                    {
                        if (index.Find(index.CreateKey(p.Id)))
                        {
                            r.Add(new PostStoreEntityId() {Id = index.Views.RetrieveIdFromIndexView.Fetch().Id});
                        }
                    }
                    table.Indexes.PrimaryIndex.SetAsCurrentIndex();
                    foreach (var p in parents)
                    {
                        if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(p.Id)))
                        {
                            table.Update.UpdateAsChildrenLoadStageView(new PostsTable.ViewValues.ChildrenLoadStageView() {ChildrenLoadStage = ChildrenLoadStageId.NotStarted});
                        }
                    }
                }
                return r;
            });
            await OpenSessionAsync(async session => await DoDeleteEntitiesList(session, toDelete));
        }
    }
}