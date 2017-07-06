using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.Tasks;
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

        private bool SeekExistingPostInThread(EsentTable table, PostStoreEntityId directParent, int postId, out PostStoreEntityId id)
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
                    throw new InvalidOperationException($"Невозможно получить первичный ключ для {EngineId}:{directParent}->{postId}");
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

        /// <summary>
        /// Сохранить коллекцию.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        /// <param name="replace">Режим обновления постов.</param>
        /// <param name="cleanupPolicy">Политика зачистки старых данных. Если null - не производить зачистку.</param>
        /// <returns>Идентификатор коллекции.</returns>
        public IAsyncOperationWithProgress<PostStoreEntityId, OperationProgress> SaveCollection(IBoardPostEntity collection, BoardPostCollectionUpdateMode replace, PostStoreStaleDataClearPolicy cleanupPolicy)
        {
            var serverFlags = new HashSet<Guid>(ServerFlags());

            long CreateMediaSequenceId(int postId, int mediaCount)
            {
                long a = postId;
                long b = mediaCount;
                return a * 1000 + b;
            }

            PostStoreEntityId SavePost(EsentTable table, EsentTable mediaTable, IBoardPost post, PostStoreEntityId[] parents, PostStoreEntityId directParent, IDictionary<string, JET_COLUMNID> colids, IDictionary<string, JET_COLUMNID> mediaColids)
            {
                if (post == null) throw new ArgumentNullException(nameof(post));
                CheckLinkEngine(post.Link);
                (var boardId, var threadId, var postId) = ExtractPostLinkData(post.Link);

                var exists = SeekExistingPostInThread(table, directParent, postId, out var newId);
                using (var update = new Update(table.Session, table.Table, exists ? JET_prep.Replace : JET_prep.Insert))
                {
                    var toUpdate = new List<ColumnValue>();

                    if (!exists)
                    {
                        for (var i = 0; i < parents.Length; i++)
                        {
                            toUpdate.Add(new Int32ColumnValue()
                            {
                                Columnid = colids[ColumnNames.ParentId],
                                ItagSequence = 0,
                                Value = parents[i].Id,
                                SetGrbit = SetColumnGrbit.UniqueMultiValues
                            });
                        }
                        toUpdate.Add(new Int32ColumnValue()
                        {
                            Value = directParent.Id,
                            Columnid = colids[ColumnNames.DirectParentId]
                        });
                        toUpdate.Add(new ByteColumnValue()
                        {
                            Value = (byte)post.EntityType,
                            Columnid = colids[ColumnNames.EntityType]
                        });
                        toUpdate.Add(new BoolColumnValue()
                        {
                            Value = true,
                            Columnid = colids[ColumnNames.DataLoaded]
                        });
                        toUpdate.Add(new ByteColumnValue()
                        {
                            Value = ChildrenLoadStageId.NotStarted,
                            Columnid = colids[ColumnNames.ChildrenLoadStage]
                        });
                        toUpdate.Add(new StringColumnValue()
                        {
                            Value = boardId,
                            Columnid = colids[ColumnNames.BoardId]
                        });
                        toUpdate.Add(new Int32ColumnValue()
                        {
                            Value = threadId,
                            Columnid = colids[ColumnNames.ParentSequenceNumber]
                        });
                        toUpdate.Add(new Int32ColumnValue()
                        {
                            Value = postId,
                            Columnid = colids[ColumnNames.SequenceNumber]
                        });
                        newId = new PostStoreEntityId()
                        {
                            Id = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.Id], RetrieveColumnGrbit.RetrieveCopy) ?? -1
                        };
                    }
                    toUpdate.Add(new StringColumnValue()
                    {
                        Value = post.Subject,
                        Columnid = colids[ColumnNames.Subject]
                    });
                    toUpdate.Add(new BytesColumnValue()
                    {
                        Value = ObjectSerializationService.SerializeToBytes(post.Thumbnail),
                        Columnid = colids[ColumnNames.Thumbnail]
                    });
                    toUpdate.Add(new DateTimeColumnValue()
                    {
                        Value = post.Date.UtcDateTime,
                        Columnid = colids[ColumnNames.Date]
                    });
                    toUpdate.Add(new StringColumnValue()
                    {
                        Value = post.BoardSpecificDate,
                        Columnid = colids[ColumnNames.BoardSpecificDate]
                    });
                    var keepFlags = new List<Guid>();
                    if (exists)
                    {
                        var flags = EnumMultivalueColumn(table, colids[ColumnNames.Flags], () => new GuidColumnValue())
                            .OfType<GuidColumnValue>().Where(g => g.Value != null && !serverFlags.Contains(g.Value.Value))
                            .Select(g => g.Value.Value);
                        foreach (var f in flags)
                        {
                            keepFlags.Add(f);
                        }
                        ClearMultiValue(table, colids[ColumnNames.Flags]);
                    }
                    if (post.Flags != null)
                    {
                        foreach (var f in post.Flags.Where(serverFlags.Contains).Concat(keepFlags).Distinct())
                        {
                            toUpdate.Add(new GuidColumnValue()
                            {
                                Columnid = colids[ColumnNames.Flags],
                                ItagSequence = 0,
                                Value = f,
                                SetGrbit = SetColumnGrbit.UniqueMultiValues
                            });
                        }
                    }
                    if (exists)
                    {
                        ClearMultiValue(table, colids[ColumnNames.ThreadTags]);
                    }
                    if (post.Tags?.Tags != null && post.Tags.Tags.Count > 0)
                    {
                        foreach (var t in post.Tags.Tags)
                        {
                            toUpdate.Add(new StringColumnValue()
                            {
                                Value = t,
                                Columnid = colids[ColumnNames.ThreadTags]
                            });
                        }
                    }
                    toUpdate.Add(new Int32ColumnValue()
                    {
                        Value = post.Likes?.Likes,
                        Columnid = colids[ColumnNames.Likes]
                    });
                    toUpdate.Add(new Int32ColumnValue()
                    {
                        Value = post.Likes?.Dislikes,
                        Columnid = colids[ColumnNames.Dislikes]
                    });
                    toUpdate.Add(new BytesColumnValue()
                    {
                        Value = ObjectSerializationService.SerializeToBytes(post.Comment),
                        Columnid = colids[ColumnNames.Document]
                    });
                    if (exists)
                    {
                        ClearMultiValue(table, colids[ColumnNames.QuotedPosts]);
                    }
                    foreach (var qp in post.Comment.GetQuotes().OfType<PostLink>().Where(l => l.OpPostNum == threadId).Select(l => l.PostNum).Distinct())
                    {
                        toUpdate.Add(new Int32ColumnValue()
                        {
                            Value = qp,
                            Columnid = colids[ColumnNames.QuotedPosts]
                        });
                    }
                    toUpdate.Add(new DateTimeColumnValue()
                    {
                        Value = post.LoadedTime.UtcDateTime,
                        Columnid = colids[ColumnNames.LoadedTime]
                    });
                    toUpdate.Add(new StringColumnValue()
                    {
                        Value = post.Poster?.Name,
                        Columnid = colids[ColumnNames.PosterName]
                    });
                    var onServerCount = post as IBoardPostOnServerCounter;
                    toUpdate.Add(new Int32ColumnValue()
                    {
                        Value = onServerCount?.OnServerCounter,
                        Columnid = colids[ColumnNames.OnServerSequenceCounter]
                    });
                    var otherData = new PostOtherData()
                    {
                        Email = post.Email,
                        Hash = post.Hash,
                        UniqueId = post.UniqueId,
                        Icon = post.Icon != null ? new PostOtherDataIcon()
                        {
                            Description = post.Icon.Description,
                            ImageLink = LinkSerialization.Serialize(post.Icon.ImageLink)
                        } : null,
                        Country = post.Country != null ? new PostOtherDataCountry()
                        {
                            ImageLink = LinkSerialization.Serialize(post.Country.ImageLink)
                        } : null,
                        Poster = post.Poster != null ? new PostOtherDataPoster()
                        {
                            NameColor = post.Poster.NameColor,
                            Tripcode = post.Poster.Tripcode,
                            NameColorStr = post.Poster.NameColorStr
                        } : null
                    };
                    toUpdate.Add(new BytesColumnValue()
                    {
                        Value = SerializeDataContract(otherData),
                        Columnid = colids[ColumnNames.OtherDataBinary]
                    });

                    Api.SetColumns(table.Session, table.Table, toUpdate.ToArray());
                    update.Save();
                }

                if (exists)
                {
                    Api.JetSetCurrentIndex(mediaTable.Session, mediaTable, GetIndexName(MediaFilesTableName, nameof(MediaFilesIndexes.EntityReferences)));
                    Api.MakeKey(mediaTable.Session, mediaTable, newId.Id, MakeKeyGrbit.NewKey);
                    if (Api.TrySeek(mediaTable.Session, mediaTable, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                    {
                        do
                        {
                            Api.JetDelete(mediaTable.Session, mediaTable);
                        } while (Api.TryMoveNext(mediaTable.Session, mediaTable));
                    }
                }
                if (post.MediaFiles != null && post.MediaFiles.Count > 0)
                {
                    for (var i = 0; i < post.MediaFiles.Count; i++)
                    {
                        if (post.MediaFiles[i] != null)
                        {
                            using (var update = new Update(mediaTable.Session, mediaTable, JET_prep.Insert))
                            {
                                var columns = new List<ColumnValue>();
                                columns.Add(new Int32ColumnValue()
                                {
                                    Value = newId.Id,
                                    Columnid = mediaColids[MediaFilesColumnNames.EntityReferences],
                                    SetGrbit = SetColumnGrbit.UniqueMultiValues,
                                    ItagSequence = 0
                                });
                                for (var j = 0; j < parents.Length; j++)
                                {
                                    columns.Add(new Int32ColumnValue()
                                    {
                                        Value = parents[j].Id,
                                        Columnid = mediaColids[MediaFilesColumnNames.EntityReferences],
                                        SetGrbit = SetColumnGrbit.UniqueMultiValues,
                                        ItagSequence = 0
                                    });
                                }
                                columns.Add(new Int64ColumnValue()
                                {
                                    Value = CreateMediaSequenceId(postId, i),
                                    Columnid = mediaColids[MediaFilesColumnNames.MediaData],
                                });
                                columns.Add(new BytesColumnValue()
                                {
                                    Value = ObjectSerializationService.SerializeToBytes(post.MediaFiles[i]),
                                    Columnid = mediaColids[MediaFilesColumnNames.MediaData],
                                });
                                Api.SetColumns(mediaTable.Session, mediaTable, columns.ToArray());
                                update.Save();
                            }
                        }
                    }
                }
                return newId;
            }

            async Task<PostStoreEntityId> Do(CancellationToken token, IProgress<OperationProgress> progress)
            {                
                CheckModuleReady();
                await WaitForTablesInitialize();

                if (collection == null) throw new ArgumentNullException(nameof(collection));

                if (!AllowedToAdd.Contains(collection.EntityType))
                {
                    throw new ArgumentException($"Нельзя напрямую загружать в базу сущности типа {collection.EntityType}");
                }

                var addedEntities = new List<PostStoreEntityId>();

                async Task DoCleanupOnError()
                {
                    try
                    {
                        await UpdateAsync(async session =>
                        {
                            await DoDeleteEntitiesList(session, addedEntities);
                            return Nothing.Value;
                        });
                    }
                    catch (Exception e)
                    {
                        GlobalErrorHandler?.SignalError(e);
                    }
                }

                async Task DoCleanStaleData()
                {
                    try
                    {
                        await ClearStaleData(cleanupPolicy);
                    }
                    catch (Exception e)
                    {
                        GlobalErrorHandler?.SignalError(e);
                    }
                }

                const string progressMessage = "Сохранение постов в базу";
                const string progressId = "ESENT";

                try
                {
                    progress.Report(new OperationProgress() { Progress = null, Message = progressMessage, OperationId = progressId });

                    PostStoreEntityId addedEntity = new PostStoreEntityId() {Id = -1};

                    if (cleanupPolicy != null)
                    {
                        CoreTaskHelper.RunUnawaitedTaskAsync(DoCleanStaleData);
                    }

                    return addedEntity;
                }
                catch
                {
                    if (addedEntities.Count > 0)
                    {
                        CoreTaskHelper.RunUnawaitedTaskAsync(DoCleanupOnError);
                    }
                    throw;
                }
            }

            Func<CancellationToken, IProgress<OperationProgress>, Task<PostStoreEntityId>> fdo = Do;
            return AsyncInfo.Run(fdo);
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

        private IEnumerable<(PostStoreEntityId id, PostStoreEntityId parentId)> FindAllChildren(EsentTable table, PostStoreEntityId parent)
        {
            return FindAllChildren(table, new [] {parent});
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
    }
}