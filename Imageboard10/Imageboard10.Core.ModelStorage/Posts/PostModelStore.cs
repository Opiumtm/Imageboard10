using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
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

        protected async Task DoDeleteEntitiesList(IEsentSession session, IEnumerable<long> toDelete)
        {
            async Task Delete(long[] toDeletePart)
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                    {
                        foreach (var id in toDeletePart)
                        {
                            Api.MakeKey(table.Session, table, id, MakeKeyGrbit.NewKey);
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

        private bool SeekExistingPostInThread(EsentTable table, long directParent, int postId, out long id)
        {
            Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
            Api.MakeKey(table.Session, table, directParent, MakeKeyGrbit.NewKey);
            Api.MakeKey(table.Session, table, postId, MakeKeyGrbit.None);
            var r = Api.TrySeek(table.Session, table.Table, SeekGrbit.SeekEQ);
            if (r)
            {
                var id1 = Api.RetrieveColumnAsInt64(table.Session, table.Table, Api.GetTableColumnid(table.Session, table.Table, ColumnNames.Id), RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                if (id1 == null)
                {
                    throw new InvalidOperationException($"Невозможно получить первичный ключ для {EngineId}:{directParent}->{postId}");
                }
                id = id1.Value;
            }
            else
            {
                id = -1;
            }
            return r;
        }

        public IAsyncOperation<IBoardPostEntity> Load(long id, PostStoreLoadMode mode)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostEntity>> Load(IList<long> ids, PostStoreLoadMode mode)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostEntity>> Load(long? parentId, int skip, int? count, PostStoreLoadMode mode)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<long>> GetChildren(long collectionId, int skip, int? count)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<int> GetCollectionSize(long collectionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<int> GetTotalSize(PostStoreEntityType type)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<long> FindEntity(PostStoreEntityType type, ILink link)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IPostStoreEntityIdSearchResult>> FindEntities(long? parentId, IList<ILink> links)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IBoardPostStoreAccessInfo> GetAccessInfo(long id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostStoreAccessInfo>> GetAccessInfos(IList<long> ids)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostStoreAccessInfo>> GetAllAccessInfos()
        {
            throw new NotImplementedException();
        }

        public IAsyncAction Touch(long id, DateTimeOffset? accessTime)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<string> GetEtag(long id)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction UpdateEtag(long id, string etag)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction SetCollectionUpdateInfo(IBoardPostCollectionUpdateInfo updateInfo)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction SetReadPostsCount(long id, int readPosts)
        {
            throw new NotImplementedException();
        }

        IAsyncOperationWithProgress<long, OperationProgress> IBoardPostStore.SaveCollection(IBoardPostEntity collection, BoardPostCollectionUpdateMode replace,
            PostStoreStaleDataClearPolicy cleanupPolicy)
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

        public IAsyncOperation<IList<IBoardPostLikes>> LoadLikes(IList<long> ids)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction UpdateFlags(IList<FlagUpdateAction> flags)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<Guid>> LoadFlags(long id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<long>> GetPostQuotes(long id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<PostStoreEntityType> GetCollectionType(long collectionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<int> GetMediaCount(long id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IPostMedia>> GetPostMedia(long id, int skip, int? count)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IPostDocument> GetDocument(long id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<long>> Delete(IList<long> ids)
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
        /// Сохранить коллекцию.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        /// <param name="replace">Режим обновления постов.</param>
        /// <param name="cleanupPolicy">Политика зачистки старых данных. Если null - не производить зачистку.</param>
        /// <returns>Идентификатор коллекции.</returns>
        public IAsyncOperationWithProgress<Guid, OperationProgress> SaveCollection(IBoardPostEntity collection, BoardPostCollectionUpdateMode replace, PostStoreStaleDataClearPolicy cleanupPolicy)
        {
            long CreateMediaSequenceId(int postId, int mediaCount)
            {
                long a = postId;
                long b = mediaCount;
                return a * 1000 + b;
            }

            long SavePost(EsentTable table, EsentTable mediaTable, IBoardPost post, long[] parents, long directParent, IDictionary<string, JET_COLUMNID> colids, IDictionary<string, JET_COLUMNID> mediaColids)
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
                            toUpdate.Add(new Int64ColumnValue()
                            {
                                Columnid = colids[ColumnNames.ParentId],
                                ItagSequence = 0,
                                Value = parents[i],
                                SetGrbit = SetColumnGrbit.UniqueMultiValues
                            });
                        }
                        toUpdate.Add(new Int64ColumnValue()
                        {
                            Value = directParent,
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
                        newId = Api.RetrieveColumnAsInt64(table.Session, table, colids[ColumnNames.Id], RetrieveColumnGrbit.RetrieveCopy) ?? -1;
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
                    if (exists)
                    {
                        ClearMultiValue(table, colids[ColumnNames.Flags]);
                    }
                    if (post.Flags != null)
                    {
                        foreach (var f in post.Flags.Distinct())
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
                    Api.MakeKey(mediaTable.Session, mediaTable, newId, MakeKeyGrbit.NewKey);
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
                                columns.Add(new Int64ColumnValue()
                                {
                                    Value = newId,
                                    Columnid = mediaColids[MediaFilesColumnNames.EntityReferences],
                                    SetGrbit = SetColumnGrbit.UniqueMultiValues,
                                    ItagSequence = 0
                                });
                                for (var j = 0; j < parents.Length; j++)
                                {
                                    columns.Add(new Int64ColumnValue()
                                    {
                                        Value = parents[j],
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

            async Task<Guid> Do(CancellationToken token, IProgress<OperationProgress> progress)
            {                
                CheckModuleReady();
                await WaitForTablesInitialize();

                if (collection == null) throw new ArgumentNullException(nameof(collection));

                if (!AllowedToAdd.Contains(collection.EntityType))
                {
                    throw new ArgumentException($"Нельзя напрямую загружать в базу сущности типа {collection.EntityType}");
                }

                var addedEntities = new List<long>();

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

                    Guid addedEntity = Guid.Empty;

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

            Func<CancellationToken, IProgress<OperationProgress>, Task<Guid>> fdo = Do;
            return AsyncInfo.Run(fdo);
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

        public IAsyncOperation<IList<long>> QueryByFlags(PostStoreEntityType type, long? parentId, IList<Guid> havingFlags, IList<Guid> notHavingFlags)
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

        private IEnumerable<(long id, long parentId)> FindAllChildren(EsentTable table, IEnumerable<long> parents)
        {
            Api.JetSetCurrentIndex(table.Session, table.Table, GetIndexName(TableName, nameof(Indexes.ParentId)));
            var colid = Api.GetTableColumnid(table.Session, table, ColumnNames.Id);
            foreach (var id in parents.Distinct())
            {
                Api.MakeKey(table.Session, table, id, MakeKeyGrbit.NewKey);
                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                {
                    do
                    {
                        var cid = Api.RetrieveColumnAsInt64(table.Session, table.Table, colid, RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                        if (cid.HasValue)
                        {
                            yield return (cid.Value, id);
                        }
                    } while (Api.TryMoveNext(table.Session, table.Table));
                }
            }
        }

        private IEnumerable<(long id, long parentId)> FindAllChildren(EsentTable table, long parent)
        {
            return FindAllChildren(table, new [] {parent});
        }

        private IEnumerable<long> FindAllParents(EsentTable table)
        {
            var colid = Api.GetTableColumnid(table.Session, table.Table, ColumnNames.ParentId);
            Api.JetSetCurrentIndex(table.Session, table.Table, GetIndexName(TableName, nameof(Indexes.ParentId)));
            if (Api.TryMoveFirst(table.Session, table))
            {
                do
                {
                    var id = Api.RetrieveColumnAsInt64(table.Session, table, colid);
                    if (id != null)
                    {
                        yield return id.Value;
                    }
                } while (Api.TryMove(table.Session, table, JET_Move.Next, MoveGrbit.MoveKeyNE));
            }
        }

        private async Task DoClearUnfinishedData()
        {
            var toDelete = new Dictionary<long, List<long>>();
            var orphanParents = new HashSet<long>();
            await QueryReadonly(session =>
            {
                using (var parTable = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                {
                    using (var idTable = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        foreach (var id in FindAllParents(parTable))
                        {
                            Api.MakeKey(idTable.Session, idTable.Table, id, MakeKeyGrbit.NewKey);
                            if (!Api.TrySeek(idTable.Session, idTable.Table, SeekGrbit.SeekEQ))
                            {
                                orphanParents.Add(id);
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
                            var id = Api.RetrieveColumnAsInt64(incTable.Session, incTable.Table, colIds[ColumnNames.Id], RetrieveColumnGrbit.RetrieveFromIndex);
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
                        foreach (var child in FindAllChildren(table, orphanParents))
                        {
                            if (!toDelete.ContainsKey(child.parentId))
                            {
                                toDelete[child.parentId] = new List<long>();
                            }
                            toDelete[child.parentId].Add(child.id);
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
                            await DoDeleteEntitiesList(session, children);
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