﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        protected async Task DoDeleteEntitiesList(IEsentSession session, IEnumerable<Guid> toDelete)
        {
            async Task Delete(Guid[] toDeletePart)
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

        private bool SeekExistingPostInThread(EsentTable table, Guid directParent, int postId)
        {
            Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
            Api.MakeKey(table.Session, table, directParent, MakeKeyGrbit.NewKey);
            Api.MakeKey(table.Session, table, postId, MakeKeyGrbit.None);
            return Api.TrySeek(table.Session, table.Table, SeekGrbit.SeekEQ);
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
            Guid SavePost(EsentTable table, IBoardPost post, Guid[] parents, Guid directParent, IDictionary<string, JET_COLUMNID> colids)
            {
                if (post == null) throw new ArgumentNullException(nameof(post));
                CheckLinkEngine(post.Link);
                (var boardId, var threadId, var postId) = ExtractPostLinkData(post.Link);

                var newId = Guid.NewGuid();
                var exists = SeekExistingPostInThread(table, directParent, postId);
                using (var update = new Update(table.Session, table.Table, exists ? JET_prep.Replace : JET_prep.Insert))
                {
                    var toUpdate = new List<ColumnValue>();

                    if (!exists)
                    {
                        toUpdate.Add(new GuidColumnValue()
                        {
                            Value = newId,
                            Columnid = colids[ColumnNames.Id]
                        });
                        for (var i = 0; i < parents.Length; i++)
                        {
                            toUpdate.Add(new GuidColumnValue()
                            {
                                Columnid = colids[ColumnNames.ParentId],
                                ItagSequence = 0,
                                Value = parents[i],
                                SetGrbit = SetColumnGrbit.UniqueMultiValues
                            });
                        }
                        toUpdate.Add(new GuidColumnValue()
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
                    if (post.Likes != null)
                    {
                        toUpdate.Add(new Int32ColumnValue()
                        {
                            Value = post.Likes.Likes,
                            Columnid = colids[ColumnNames.Likes]
                        });
                        toUpdate.Add(new Int32ColumnValue()
                        {
                            Value = post.Likes.Dislikes,
                            Columnid = colids[ColumnNames.Dislikes]
                        });
                    }
                    toUpdate.Add(new BytesColumnValue()
                    {
                        Value = ObjectSerializationService.SerializeToBytes(post.Comment),
                        Columnid = colids[ColumnNames.Document]
                    });
                    foreach (var qp in post.Comment.GetQuotes().OfType<PostLink>().Where(l => l.OpPostNum == threadId).Select(l => l.PostNum).Distinct())
                    {
                        toUpdate.Add(new Int32ColumnValue()
                        {
                            Value = qp,
                            Columnid = colids[ColumnNames.QuotedPosts]
                        });
                    }
                    Api.SetColumns(table.Session, table.Table, toUpdate.ToArray());
                    toUpdate.Add(new DateTimeColumnValue()
                    {
                        Value = post.LoadedTime.UtcDateTime,
                        Columnid = colids[ColumnNames.LoadedTime]
                    });
                    if (post.Poster != null)
                    {
                        toUpdate.Add(new StringColumnValue()
                        {
                            Value = post.Poster.Name,
                            Columnid = colids[ColumnNames.PosterName]
                        });
                    }
                    update.Save();
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

                var addedEntities = new List<Guid>();

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

        private IEnumerable<(Guid id, Guid parentId)> FindAllChildren(EsentTable table, IEnumerable<Guid> parents)
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
                        var cid = Api.RetrieveColumnAsGuid(table.Session, table.Table, colid, RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                        if (cid.HasValue)
                        {
                            yield return (cid.Value, id);
                        }
                    } while (Api.TryMoveNext(table.Session, table.Table));
                }
            }
        }

        private IEnumerable<(Guid id, Guid parentId)> FindAllChildren(EsentTable table, Guid parent)
        {
            return FindAllChildren(table, new [] {parent});
        }

        private IEnumerable<Guid> FindAllParents(EsentTable table)
        {
            var colid = Api.GetTableColumnid(table.Session, table.Table, ColumnNames.ParentId);
            Api.JetSetCurrentIndex(table.Session, table.Table, GetIndexName(TableName, nameof(Indexes.ParentId)));
            if (Api.TryMoveFirst(table.Session, table))
            {
                do
                {
                    var id = Api.RetrieveColumnAsGuid(table.Session, table, colid);
                    if (id != null)
                    {
                        yield return id.Value;
                    }
                } while (Api.TryMove(table.Session, table, JET_Move.Next, MoveGrbit.MoveKeyNE));
            }
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
                        Api.JetSetCurrentIndex(table.Session, table.Table, GetIndexName(TableName, nameof(Indexes.ParentId)));
                        foreach (var child in FindAllChildren(table, orphanParents))
                        {
                            if (!toDelete.ContainsKey(child.parentId))
                            {
                                toDelete[child.parentId] = new List<Guid>();
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