using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.ModelStorage.UnitTests;
using Imageboard10.Core.Tasks;
using Imageboard10.Core.Utility;
using Imageboard10.ModuleInterface;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Хранилище постов.
    /// </summary>
    public partial class PostModelStore
    {
        /// <summary>
        /// Сохранить коллекцию.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        /// <param name="replace">Режим обновления постов.</param>
        /// <param name="cleanupPolicy">Политика зачистки старых данных. Если null - не производить зачистку.</param>
        /// <param name="backgroundFinished">Завершена фоновая операция.</param>
        /// <returns>Идентификатор коллекции.</returns>
        public IAsyncOperationWithProgress<PostStoreEntityId, OperationProgress> SaveCollection(IBoardPostEntity collection, BoardPostCollectionUpdateMode replace, PostStoreStaleDataClearPolicy cleanupPolicy, BoardPostStoreBackgroundFinishedCallback backgroundFinished)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            var serverFlags = new HashSet<Guid>(ServerFlags());
            const string progressMessage = "Сохранение постов в базу";
            const string progressId = "ESENT";

            long CreateMediaSequenceId(int postId, int mediaCount)
            {
                long a = postId;
                long b = mediaCount;
                return a * 1000 + b;
            }

            PreSerializedPostData PreSerialize(PostsTable table, IBoardPost post, PostStoreEntityId directParent, bool parentExists)
            {
                var preSeek = PreSeekPost(table, post, directParent, parentExists);
                var otherData = new PostOtherData()
                {
                    Email = post?.Email,
                    Hash = post?.Hash,
                    UniqueId = post?.UniqueId,
                    Icon = post?.Icon != null ? new PostOtherDataIcon()
                    {
                        Description = post.Icon.Description,
                        ImageLink = LinkSerialization.Serialize(post.Icon.ImageLink)
                    } : null,
                    Country = post?.Country != null ? new PostOtherDataCountry()
                    {
                        ImageLink = LinkSerialization.Serialize(post.Country.ImageLink)
                    } : null,
                    Poster = post?.Poster != null ? new PostOtherDataPoster()
                    {
                        NameColor = post.Poster.NameColor,
                        Tripcode = post.Poster.Tripcode,
                        NameColorStr = post.Poster.NameColorStr
                    } : null
                };
                return new PreSerializedPostData()
                {
                    PreSeek = preSeek,
                    LinkHash = post?.Link?.GetLinkHash(),
                    Document = ObjectSerializationService.SerializeToBytes(post?.Comment),
                    Quotes = post?.Comment != null ? post.Comment.GetQuotes().OfType<PostLink>().Where(l => l.OpPostNum == preSeek.ThreadId).Select(l => l.PostNum).Distinct().ToArray() : new int[0],
                    OtherData = SerializeDataContract(otherData),
                    Media = post?.MediaFiles != null ? post.MediaFiles.Select(ObjectSerializationService.SerializeToBytes).ToArray() : new byte[0][],
                    Thumbnail = ObjectSerializationService.SerializeToBytes(post?.Thumbnail)
                };
            }

            void SetPostFields(PostsTable table, IBoardPost post, PostStoreEntityId[] parents, PostStoreEntityId directParent, ref PostStoreEntityId newId, bool exists, string boardId, int threadId, int postId, PreSerializedPostData preSerialized)
            {
                using (var update = exists ? table.Update.CreateUpdate() : table.Insert.CreateUpdate())
                {
                    if (!exists)
                    {
                        var idv = new PostsTable.ViewValues.PostDataIdentityUpdateView()
                        {
                            ParentId = parents.Select(p => new Int32ColumnValue() { Value = p.Id, SetGrbit = SetColumnGrbit.UniqueMultiValues}).ToArray(),
                            DirectParentId = directParent.Id,
                            EntityType = (byte)post.EntityType,
                            DataLoaded = true,
                            ChildrenLoadStage = ChildrenLoadStageId.NotStarted,
                            BoardId = boardId,
                            ParentSequenceNumber = threadId,
                            SequenceNumber = postId
                        };
                        table.Insert.PostDataIdentityUpdateView.Set(ref idv, true);
                        newId = new PostStoreEntityId()
                        {
                            Id = table.Columns.Id_AutoincrementValue
                        };
                    }
                    var setData = new PostsTable.ViewValues.PostDataUpdateView()
                    {
                        Subject = post.Subject,
                        Thumbnail = preSerialized.Thumbnail,
                        Date = post.Date.UtcDateTime,
                        BoardSpecificDate = post.BoardSpecificDate,
                        Likes = post.Likes?.Likes,
                        Dislikes = post.Likes?.Dislikes,
                        Document = preSerialized.Document,
                        QuotedPosts = preSerialized.Quotes.Distinct().Select(q => new Int32ColumnValue() { Value = q}).ToArray(),
                        LoadedTime = post.LoadedTime.UtcDateTime,
                        PosterName = post.Poster?.Name,
                        OnServerSequenceCounter = (post as IBoardPostOnServerCounter)?.OnServerCounter,
                        OtherDataBinary = preSerialized.OtherData
                    };

                    if (post is IBoardPostEntityWithSequence opc)
                    {
                        setData.ThreadPreviewSequence = opc.OnPageSequence;
                    }
                    else
                    {
                        setData.ThreadPreviewSequence = null;
                    }

                    var keepFlags = new List<Guid>();
                    if (exists)
                    {
                        var flags = table.Columns.Flags.Values
                            .Where(g => g.Value != null && !serverFlags.Contains(g.Value.Value) && g.Value.Value != BoardPostFlags.IsDeletedOnServer)
                            .Select(g => g.Value.Value);
                        foreach (var f in flags)
                        {
                            keepFlags.Add(f);
                        }
                    }
                    List<Guid> toSet = new List<Guid>();
                    foreach (var f in (post.Flags ?? new List<Guid>()).Where(serverFlags.Contains).Concat(keepFlags).Distinct())
                    {
                        if (f == UnitTestStoreFlags.ShouldFail)
                        {
                            throw new UnitTestStoreException();
                        }
                        if (f == UnitTestStoreFlags.ShouldFailWithoutCleanup)
                        {
                            throw new UnitTestStoreExceptionWithoutCleanup();
                        }
                        toSet.Add(f);
                    }
                    setData.Flags = toSet.Select(id => new GuidColumnValue() { Value = id, SetGrbit = SetColumnGrbit.UniqueMultiValues}).ToArray();
                    if (post.Tags?.Tags != null && post.Tags.Tags.Count > 0)
                    {
                        setData.ThreadTags = post.Tags.Tags.Where(t => !string.IsNullOrEmpty(t)).Distinct()
                            .Select(t => new StringColumnValue() {Value = t})
                            .ToArray();
                    }
                    else
                    {
                        setData.ThreadTags = new StringColumnValue[0];
                    }
                    if (exists)
                    {
                        table.Update.PostDataUpdateView.Set(ref setData);
                    }
                    else
                    {
                        table.Insert.PostDataUpdateView.Set(ref setData, true);
                    }
                    update.Save();
                }
            }

            void SetPostMedia(MediaFilesTable mediaTable, PostStoreEntityId[] parents, PostStoreEntityId newId, int postId, bool exists, PreSerializedPostData preSerialized)
            {
                if (exists)
                {
                    var index = mediaTable.Indexes.EntityReferencesIndex;
                    foreach (var _ in index.Enumerate(index.CreateKey(newId.Id)))
                    {
                        mediaTable.DeleteCurrentRow();
                    }
                }
                if (preSerialized.Media != null && preSerialized.Media.Length > 0)
                {
                    for (var i = 0; i < preSerialized.Media.Length; i++)
                    {
                        var references = new Int32ColumnValue[1 + parents.Length];
                        references[0] = new Int32ColumnValue()
                        {
                            Value = newId.Id,
                        };
                        for (var j = 0; j < parents.Length; j++)
                        {
                            references[j+1] = new Int32ColumnValue() { Value = parents[j].Id };
                        }
                        mediaTable.Insert.InsertAsInsertView(new MediaFilesTable.ViewValues.InsertView()
                        {
                            EntityReferences = references,
                            SequenceNumber = CreateMediaSequenceId(postId, i),
                            MediaData = preSerialized.Media[i]
                        });
                    }
                }
            }

            PreSeekPost PreSeekPost(PostsTable table, IBoardPost post, PostStoreEntityId directParent, bool parentExists)
            {
                var postLink = post?.Link;
                CheckLinkEngine(postLink);
                (var boardId, var threadId, var postId) = ExtractPostLinkData(postLink);
                // ReSharper disable once PossibleNullReferenceException
                if (post.EntityType != PostStoreEntityType.Post && post.EntityType != PostStoreEntityType.ThreadPreviewPost && post.EntityType != PostStoreEntityType.CatalogPost)
                {
                    throw new InvalidOperationException($"Неправильный тип сущности поста {post.EntityType}");
                }
                if (parentExists)
                {
                    var exists = SeekExistingEntityInSequence(table, directParent, postId, out var newId);
                    return new PreSeekPost()
                    {
                        Exists = exists,
                        ThreadId = threadId,
                        BoardId = boardId,
                        FoundId = newId,
                        PostId = postId
                    };
                }
                return new PreSeekPost()
                {
                    Exists = false,
                    ThreadId = threadId,
                    BoardId = boardId,
                    FoundId = new PostStoreEntityId() { Id = -1 },
                    PostId = postId
                };
            }

            (PostStoreEntityId id, bool exists) SavePost(PostsTable table, MediaFilesTable mediaTable, IBoardPost post, PostStoreEntityId[] parents, PostStoreEntityId directParent, PreSerializedPostData preSerialized)
            {
                if (post == null) throw new ArgumentNullException(nameof(post));

                var boardId = preSerialized.PreSeek.BoardId;
                var threadId = preSerialized.PreSeek.ThreadId;
                var postId = preSerialized.PreSeek.PostId;
                var newId = preSerialized.PreSeek.FoundId;
                var exists = preSerialized.PreSeek.Exists;


                if (post.Flags?.Any(f => f == UnitTestStoreFlags.AlwaysInsert) ?? false)
                {
                    exists = false;
                }
                if (exists)
                {
                    table.Indexes.PrimaryIndex.SetAsCurrentIndex();
                    if (!GotoEntityId(table, newId))
                    {
                        throw new InvalidOperationException($"Сущность с идентификатором {newId.Id} не найдена");
                    }
                }

                SetPostFields(table, post, parents, directParent, ref newId, exists, boardId, threadId, postId, preSerialized);

                SetPostMedia(mediaTable, parents, newId, postId, exists, preSerialized);
                return (newId, exists);
            }

            TInfo ExtractInfo<T, TInfo>(T collection2) where TInfo : class, IBoardPostCollectionInfo where T : IBoardPostCollectionInfoEnabled
            {
                return collection2.Info?.Items?.Where(i => i?.GetInfoInterfaceTypes()?.Any(t => t == typeof(TInfo)) ?? false)?.Select(i => i as TInfo)?.FirstOrDefault(i => i != null);
            }

            int ExtractPostNumber(IBoardPost post)
            {
                if (post?.Link is PostLink pl)
                {
                    return pl.PostNum;
                }
                return 0;
            }

            int ExtractThreadNumber(IThreadPreviewPostCollection thread)
            {
                var post = thread?.Posts?.OrderBy(ExtractPostNumber)?.FirstOrDefault();
                if (post?.Link is PostLink pl)
                {
                    return pl.OpPostNum;
                }
                return 0;
            }

            void SetPostCollectionFields<T>(IEsentSession session, PostsTable table, PostStoreEntityId? directParent, ref PostStoreEntityId newId, bool exists, T collection2, string boardId,
                int sequenceId)
                where T : class, IBoardPostEntity, IBoardPostCollectionInfoEnabled
            {
                var columns = table.Columns;
                if (replace == BoardPostCollectionUpdateMode.Merge)
                {
                    if (collection2.EntityType != PostStoreEntityType.Thread)
                    {
                        throw new InvalidOperationException($"Нельзя сливать посты в сущностях данного типа {collection2.EntityType}");
                    }
                    if (!exists)
                    {
                        throw new InvalidOperationException("Не найден тред, для которого производится слияние постов");
                    }
                    table.Indexes.PrimaryIndex.SetAsCurrentIndex();
                    if (!GotoEntityId(table, newId))
                    {
                        throw new InvalidOperationException("Не найден тред, для которого производится слияние постов (seek)");
                    }
                    var childMode = table.Columns.ChildrenLoadStage;
                    if (childMode != ChildrenLoadStageId.Completed)
                    {
                        throw new InvalidOperationException("Нельзя добавлять посты в ещё не загруженный тред");
                    }
                    using (var update = table.Update.CreateUpdate())
                    {
                        var collection3 = collection2 as IBoardPostCollection;
                        var lastPost = collection3?.Posts?.OrderByDescending(ExtractPostNumber)?.FirstOrDefault();
                        if (lastPost != null)
                        {
                            columns.LoadedTime = lastPost.LoadedTime.UtcDateTime;
                            columns.LastServerUpdate = lastPost.Date.UtcDateTime;
                            if (lastPost.Link is PostLink pl)
                            {
                                columns.LastPostLinkOnServer = pl.PostNum;
                            }
                        }
                        int delta = 0;
                        if (collection3?.Posts != null && collection3.Posts.Count > 0)
                        {
                            using (var postTable = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                            {
                                var index = postTable.Indexes.InThreadPostLinkIndex;
                                index.SetAsCurrentIndex();
                                foreach (var p in collection3.Posts)
                                {
                                    if (p.Link is PostLink pl)
                                    {
                                        var seqId = pl.PostNum;
                                        if (!index.Find(index.CreateKey(newId.Id, seqId)))
                                        {
                                            delta++;
                                        }
                                    }
                                }
                            }
                        }
                        var onServerCount = table.Columns.NumberOfPostsOnServer ?? 0;
                        if (delta > 0)
                        {
                            onServerCount += delta;
                        }
                        columns.NumberOfPostsOnServer = onServerCount;
                        if (collection2 is IBoardPostCollectionEtagEnabled ee)
                        {
                            columns.Etag = ee.Etag;
                        }
                        update.Save();
                    }
                }
                else
                {
                    if (exists)
                    {
                        table.Indexes.PrimaryIndex.SetAsCurrentIndex();
                        if (!GotoEntityId(table, newId))
                        {
                            throw new InvalidOperationException($"Сущность с идентификатором {newId.Id} не найдена");
                        }
                    }
                    using (var update = exists ? table.Update.CreateUpdate() : table.Insert.CreateUpdate())
                    {
                        if (!exists)
                        {
                            var identityData = new PostsTable.ViewValues.PostDataIdentityUpdateView()
                            {
                                DirectParentId = directParent?.Id,
                                ParentId = directParent != null ? new [] { new Int32ColumnValue() { Value = directParent.Value.Id, SetGrbit = SetColumnGrbit.UniqueMultiValues } } : new Int32ColumnValue[0],
                                EntityType = (byte)collection2.EntityType,
                                DataLoaded = true,
                                ChildrenLoadStage = ChildrenLoadStageId.NotStarted,
                                BoardId = boardId,
                                SequenceNumber = sequenceId
                            };
                            table.Insert.PostDataIdentityUpdateView.Set(ref identityData, true);
                            newId = new PostStoreEntityId()
                            {
                                Id = table.Columns.Id_AutoincrementValue
                            };
                        }
                        columns.Subject = collection2.Subject;
                        columns.Thumbnail = ObjectSerializationService.SerializeToBytes(collection2.Thumbnail);
                        var collection3 = collection2 as IBoardPostCollection;
                        if (collection2 is IBoardPostEntityWithSequence opc)
                        {
                            columns.ThreadPreviewSequence = opc.OnPageSequence;
                        }
                        if ((collection2.EntityType == PostStoreEntityType.Thread || collection2.EntityType == PostStoreEntityType.ThreadPreview) && collection3 != null)
                        {
                            var lastPost = collection3.Posts?.OrderByDescending(ExtractPostNumber).FirstOrDefault();
                            var firstPost = collection3.Posts?.OrderBy(ExtractPostNumber).FirstOrDefault();
                            if (lastPost != null)
                            {
                                columns.LoadedTime = lastPost.LoadedTime.UtcDateTime;
                                columns.LastServerUpdate = lastPost.Date.UtcDateTime;
                                if (lastPost.Link is PostLink pl)
                                {
                                    columns.LastPostLinkOnServer = pl.PostNum;
                                }
                                else
                                {
                                    columns.LastPostLinkOnServer = null;
                                }
                            }
                            else
                            {
                                columns.LoadedTime = null;
                                columns.LastServerUpdate = null;
                            }
                            if (firstPost != null)
                            {
                                columns.Date = firstPost.Date.UtcDateTime;
                                columns.BoardSpecificDate = firstPost.BoardSpecificDate;
                                if (firstPost.Tags?.Tags != null && firstPost.Tags.Tags.Count > 0)
                                {
                                    columns.ThreadTags.SetValues(firstPost.Tags.Tags.Distinct().Select(t => new StringColumnValue()
                                    {
                                        Value = t
                                    }).ToArray(), !exists);
                                }
                                else
                                {
                                    if (exists)
                                    {
                                        columns.ThreadTags.Clear();
                                    }
                                }
                            }
                            else
                            {
                                columns.Date = null;
                                columns.BoardSpecificDate = null;
                                if (exists)
                                {
                                    columns.ThreadTags.Clear();
                                }
                            }
                        }
                        var keepFlags = new List<Guid>();
                        if (exists)
                        {
                            var flags = columns.Flags.Values
                                .Where(g => g.Value != null && !serverFlags.Contains(g.Value.Value))
                                .Select(g => g.Value.Value);
                            foreach (var f in flags)
                            {
                                keepFlags.Add(f);
                            }
                        }
                        var infoFlags = ExtractInfo<T, IBoardPostCollectionInfoFlags>(collection2)?.Flags ?? new List<Guid>();
                        var toSetFlags = new HashSet<Guid>();
                        foreach (var f in infoFlags.Where(serverFlags.Contains).Concat(keepFlags).Distinct())
                        {
                            if (f == UnitTestStoreFlags.ShouldFail)
                            {
                                throw new UnitTestStoreException();
                            }
                            if (f == UnitTestStoreFlags.ShouldFailWithoutCleanup)
                            {
                                throw new UnitTestStoreExceptionWithoutCleanup();
                            }
                            toSetFlags.Add(f);
                        }
                        columns.Flags.SetValues(toSetFlags.Select(f => new GuidColumnValue()
                        {
                            Value = f,
                            SetGrbit = SetColumnGrbit.UniqueMultiValues
                        }).ToArray(), !exists);
                        columns.OtherDataBinary = ObjectSerializationService.SerializeToBytes(collection2.Info);

                        if (collection2.EntityType == PostStoreEntityType.ThreadPreview && collection2 is IThreadPreviewPostCollection collection4)
                        {
                            columns.PreviewCounts = WriteThreadPreviewCounts(collection4);
                        }
                        else
                        {
                            columns.PreviewCounts = null;
                        }

                        if (collection2.EntityType == PostStoreEntityType.Thread)
                        {
                            columns.NumberOfPostsOnServer = collection3?.Posts?.Count ?? 0;
                        }
                        else
                        {
                            columns.NumberOfPostsOnServer = null;
                        }

                        if (collection2 is IBoardPostCollectionEtagEnabled ee)
                        {
                            columns.Etag = ee.Etag;
                        }
                        else
                        {
                            columns.Etag = null;
                        }

                        update.Save();
                    }
                }
            }

            async Task SaveCollectionPosts(IBoardPostCollection collection2, PostStoreEntityId? directParent, PostStoreEntityId collectionId, bool exists, bool reportProgress, IProgress<OperationProgress> progress, CancellationToken token, ConcurrentBag<PostStoreEntityId> addedEntities)
            {
                if (!exists)
                {
                    await SetEntityChildrenLoadStatus(collectionId, ChildrenLoadStageId.Started);
                }
                if (collection2.Posts != null)
                {
                    double allCount = collection2.Posts.Count;
                    double savedCount = 0;
                    object savedCountLock = new object();
                    if (reportProgress)
                    {
                        progress?.Report(new OperationProgress()
                        {
                            Progress = 0.0,
                            Message = progressMessage,
                            OperationId = progressId
                        });
                    }
                    var parents1 = new HashSet<int>();
                    if (directParent != null)
                    {
                        parents1.Add(directParent.Value.Id);
                    }
                    parents1.Add(collectionId.Id);
                    var parents = parents1.Select(p => new PostStoreEntityId() { Id = p }).ToArray();

                    var toProcess = collection2.Posts.SplitSetRandomized(10, 30).Select(s => s.ToArray()).DistributeToProcess(UploadParallelism);

                    await ParallelizeOnSessions(toProcess, async (session, processPosts) =>
                    {
                        foreach (var p in processPosts)
                        {
                            token.ThrowIfCancellationRequested();
                            var toSave = p.ToArray();

                            var saved = await session.RunInTransaction(() =>
                            {
                                var toAdd = new List<PostStoreEntityId>();
                                using (var table = OpenPostsTable(session, OpenTableGrbit.Updatable))
                                {
                                    using (var mediaTable = OpenMediaFilesTable(session, OpenTableGrbit.Updatable))
                                    {
                                        foreach (var post in toSave)
                                        {
                                            var pre = PreSerialize(table, post, collectionId, exists);
                                            var r = SavePost(table, mediaTable, post, parents, collectionId, pre);
                                            if (!r.exists)
                                            {
                                                toAdd.Add(r.id);
                                            }
                                        }
                                    }
                                }
                                return (true, toAdd);
                            }, 2);
                            foreach (var id in saved)
                            {
                                addedEntities.Add(id);
                            }

                            double curSaved = 0.0;
                            lock (savedCountLock)
                            {
                                savedCount += toSave.Length;
                                curSaved = savedCount;
                            }
                            if (reportProgress && allCount > 0.01)
                            {
                                CoreTaskHelper.RunUnawaitedTask(() =>
                                {
                                    progress?.Report(new OperationProgress()
                                    {
                                        Progress = curSaved / allCount,
                                        Message = progressMessage,
                                        OperationId = progressId
                                    });
                                });
                            }
                        }
                        return Nothing.Value;
                    });
                }

                await SetEntityChildrenLoadStatus(collectionId, ChildrenLoadStageId.Completed);
            }

            async Task CleanChildPostsOnReplace(IBoardPostCollection collection2, PostStoreEntityId collectionId, bool exists)
            {
                if (replace != BoardPostCollectionUpdateMode.Merge && exists)
                {
                    var children = await OpenSession(session =>
                    {
                        using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                        {
                            return new HashSet<int>(FindAllChildrenSeqNums(table, collectionId).Where(c => c.parentId.Id == collectionId.Id).Select(c => c.sequenceId));
                        }
                    });
                    foreach (var postId in (collection2.Posts ?? new List<IBoardPost>()).Select(ExtractPostNumber))
                    {
                        children.Remove(postId);
                    }
                    if (children.Count > 0)
                    {
                        var isDelete = replace == BoardPostCollectionUpdateMode.Replace || collection2.EntityType != PostStoreEntityType.Thread;
                        foreach (var part in children.SplitSet(50))
                        {
                            var partArr = part.ToArray();
                            await OpenSessionAsync(async session =>
                            {
                                await session.RunInTransaction(() =>
                                {
                                    using (var table = OpenPostsTable(session, OpenTableGrbit.Updatable))
                                    {
                                        var index = table.Indexes.InThreadPostLinkIndex;
                                        index.SetAsCurrentIndex();
                                        foreach (var id in partArr)
                                        {
                                            foreach (var _ in index.Enumerate(index.CreateKey(collectionId.Id, id)))
                                            {
                                                if (isDelete)
                                                {
                                                    table.DeleteCurrentRow();
                                                }
                                                else
                                                {
                                                    var haveFlag = table.Columns.Flags.Enumerate().Any(f => f.Value == BoardPostFlags.IsDeletedOnServer);
                                                    if (!haveFlag)
                                                    {
                                                        using (var update = table.Update.CreateUpdate())
                                                        {
                                                            table.Columns.Flags.Add(new GuidColumnValue()
                                                            {
                                                                Value = BoardPostFlags.IsDeletedOnServer
                                                            });
                                                            update.Save();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    return true;
                                }, 2);
                                return Nothing.Value;
                            });
                        }
                    }
                }
            }

            async Task<PostStoreEntityId> SaveCatalogOrThread(IBoardPostEntity entity, CancellationToken token, IProgress<OperationProgress> progress, PostStoreEntityId? directParent, bool reportProgress, ConcurrentBag<PostStoreEntityId> addedEntities)
            {
                token.ThrowIfCancellationRequested();
                var collection2 = entity as IBoardPostCollection;
                if (collection2 == null)
                {
                    throw new ArgumentException("Неравильный тип объекта для треда или каталога", nameof(entity));
                }

                bool exists = false;

                var collectionId = await OpenSessionAsync(async session =>
                {
                    return await session.RunInTransaction(() =>
                    {
                        PostStoreEntityId newId;
                        (var boardId, var sequenceId) = ExtractCatalogOrThreadLinkData(collection2.Link);

                        using (var table = OpenPostsTable(session, OpenTableGrbit.Updatable))
                        {
                            if (collection2.EntityType == PostStoreEntityType.ThreadPreview)
                            {
                                if (!(entity is IThreadPreviewPostCollection))
                                {
                                    throw new ArgumentException("Неправильный тип объекта превью треда", nameof(directParent));
                                }
                                if (directParent == null)
                                {
                                    throw new ArgumentException("Для превью треда должен быть указан идентификатор родительской сущности", nameof(directParent));
                                }
                                exists = SeekExistingEntityInSequence(table, directParent.Value, sequenceId, out newId);
                            }
                            else if (collection2.EntityType == PostStoreEntityType.Thread || collection2.EntityType == PostStoreEntityType.Catalog)
                            {
                                if (directParent != null)
                                {
                                    throw new ArgumentException("Для треда или каталога не должен быть указан идентификатор родительской сущности", nameof(directParent));
                                }
                                exists = SeekExistingEntityOnBoard(table, collection2.EntityType, boardId, sequenceId, out newId);
                            }
                            else
                            {
                                throw new InvalidOperationException($"Неправильный тип сущности для сохранения {collection2.EntityType}");
                            }

                            if (ExtractInfo<IBoardPostCollection, IBoardPostCollectionInfoFlags>(collection2)?.Flags?.Any(f => f == UnitTestStoreFlags.AlwaysInsert) ?? false)
                            {
                                exists = false;
                            }

                            SetPostCollectionFields(session, table, directParent, ref newId, exists, collection2, boardId, sequenceId);

                            return (true, newId);
                        }
                    }, 2);
                });

                await CleanChildPostsOnReplace(collection2, collectionId, exists);

                if (!exists)
                {
                    addedEntities.Add(collectionId);
                }

                await SaveCollectionPosts(collection2, directParent, collectionId, exists, reportProgress, progress, token, addedEntities);

                return collectionId;
            }

            async Task CleanChildThreadsOnReplace(IBoardPageThreadCollection collection2, PostStoreEntityId collectionId, bool exists)
            {
                if (exists)
                {
                    var children = await OpenSession(session =>
                    {
                        using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                        {
                            return new HashSet<int>(FindAllChildrenSeqNums(table, collectionId).Where(c => c.parentId.Id == collectionId.Id).Select(c => c.sequenceId));
                        }
                    });
                    foreach (var postId in (collection2.Threads ?? new List<IThreadPreviewPostCollection>()).Select(ExtractThreadNumber))
                    {
                        children.Remove(postId);
                    }
                    if (children.Count > 0)
                    {
                        var toDelete = new HashSet<int>();
                        await OpenSessionAsync(async session =>
                        {
                            await session.Run(() =>
                            {
                                using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                                {
                                    using (var ct = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                                    {
                                        var index = table.Indexes.InThreadPostLinkIndex;
                                        index.SetAsCurrentIndex();
                                        foreach (var id in children)
                                        {
                                            if (index.Find(index.CreateKey(collectionId.Id, id)))
                                            {
                                                var iid = index.Views.RetrieveIdFromIndexView.Fetch();
                                                toDelete.Add(iid.Id);
                                                foreach (var c in FindAllChildren(ct, new PostStoreEntityId {Id = iid.Id}).Where(c => c.parentId.Id == iid.Id))
                                                {
                                                    toDelete.Add(c.id.Id);
                                                }
                                            }
                                        }
                                    }
                                }
                            });
                            return Nothing.Value;
                        });
                        await OpenSessionAsync(async session =>
                        {
                            await DoDeleteEntitiesList(session, toDelete.Select(id => new PostStoreEntityId() { Id = id }));
                            return Nothing.Value;
                        });
                    }
                }
            }

            async Task SaveThreadPreviews(IBoardPageThreadCollection collection2, PostStoreEntityId collectionId, IProgress<OperationProgress> progress, bool exists, CancellationToken token, ConcurrentBag<PostStoreEntityId> addedEntities)
            {
                if (!exists)
                {
                    await SetEntityChildrenLoadStatus(collectionId, ChildrenLoadStageId.Started);
                }

                var toAdd = collection2.Threads ?? new List<IThreadPreviewPostCollection>();

                progress?.Report(new OperationProgress()
                {
                    Progress = 0.0,
                    Message = progressMessage,
                    OperationId = progressId
                });

                double cnt = toAdd.Count;

                for (var i = 0; i < toAdd.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    await SaveCatalogOrThread(toAdd[i], token, progress, collectionId, false, addedEntities);

                    double p = i + 1;
                    progress?.Report(new OperationProgress()
                    {
                        Progress = p / cnt,
                        Message = progressMessage,
                        OperationId = progressId
                    });
                }

                await SetEntityChildrenLoadStatus(collectionId, ChildrenLoadStageId.Completed);
            }

            async Task<PostStoreEntityId> SaveBoardPage(CancellationToken token, IProgress<OperationProgress> progress, bool reportProgress,
                ConcurrentBag<PostStoreEntityId> addedEntities)
            {
                token.ThrowIfCancellationRequested();
                var collection2 = collection as IBoardPageThreadCollection;
                if (collection2 == null)
                {
                    throw new ArgumentException("Неравильный тип объекта для страницы доски", nameof(collection));
                }

                if (replace == BoardPostCollectionUpdateMode.Merge)
                {
                    throw new InvalidOperationException("Нельзя сливать посты для данного вида сущности");
                }

                bool exists = false;
                var collectionId = await OpenSessionAsync(async session =>
                {
                    return await session.RunInTransaction(() =>
                    {
                        (var boardId, var sequenceId) = ExtractBoardPageLinkData(collection2.Link);

                        using (var table = OpenPostsTable(session, OpenTableGrbit.None))
                        {
                            PostStoreEntityId newId;
                            if (collection2.EntityType == PostStoreEntityType.BoardPage)
                            {
                                exists = SeekExistingEntityOnBoard(table, collection2.EntityType, boardId, sequenceId, out newId);
                            }
                            else
                            {
                                throw new InvalidOperationException($"Неправильный тип сущности для сохранения {collection2.EntityType}");
                            }

                            if (ExtractInfo<IBoardPageThreadCollection, IBoardPostCollectionInfoFlags>(collection2)?.Flags?.Any(f => f == UnitTestStoreFlags.AlwaysInsert) ?? false)
                            {
                                exists = false;
                            }

                            SetPostCollectionFields(session, table, null, ref newId, exists, collection2, boardId, sequenceId);

                            return (true, newId);
                        }
                    }, 2);
                });

                await CleanChildThreadsOnReplace(collection2, collectionId, exists);

                if (!exists)
                {
                    addedEntities.Add(collectionId);
                }

                await SaveThreadPreviews(collection2, collectionId, progress, exists, token, addedEntities);

                return collectionId;
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

                var addedEntities = new ConcurrentBag<PostStoreEntityId>();

                async Task DoCleanupOnError()
                {
                    try
                    {
                        await OpenSessionAsync(async session =>
                        {
                            await DoDeleteEntitiesList(session, addedEntities.ToArray());
                            return Nothing.Value;
                        });
                        backgroundFinished?.Invoke(null);
                    }
                    catch (Exception e)
                    {
                        backgroundFinished?.Invoke(e);
                        GlobalErrorHandler?.SignalError(e);
                    }
                }

                async Task DoCleanStaleData()
                {
                    try
                    {
                        await ClearStaleData(cleanupPolicy);
                        backgroundFinished?.Invoke(null);
                    }
                    catch (Exception e)
                    {
                        backgroundFinished?.Invoke(e);
                        GlobalErrorHandler?.SignalError(e);
                    }
                }

                try
                {
                    progress.Report(new OperationProgress() {Progress = null, Message = progressMessage, OperationId = progressId});

                    PostStoreEntityId addedEntity = new PostStoreEntityId() {Id = -1};

                    if (collection.EntityType == PostStoreEntityType.Catalog || collection.EntityType == PostStoreEntityType.Thread)
                    {
                        addedEntity = await SaveCatalogOrThread(collection, token, progress, null, true, addedEntities);
                    }
                    else if (collection.EntityType == PostStoreEntityType.BoardPage)
                    {
                        addedEntity = await SaveBoardPage(token, progress, true, addedEntities);
                    }

                    if (cleanupPolicy != null)
                    {
                        CoreTaskHelper.RunUnawaitedTaskAsync(DoCleanStaleData);
                    }
                    else
                    {
                        backgroundFinished?.Invoke(null);
                    }

                    return addedEntity;
                }
                catch (UnitTestStoreExceptionWithoutCleanup)
                {
                    // Не очищать данные.
                    throw;
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

        private async Task<IList<KeyValuePair<ILink, PostStoreEntityId>>> UploadBareEntities(IEnumerable<IBoardPostEntity> list)
        {
            async ValueTask<IList<KeyValuePair<ILink, PostStoreEntityId>>> Do(IEsentSession session, IList<IBoardPostEntity[]> arr)
            {
                var result = new List<KeyValuePair<ILink, PostStoreEntityId>>();
                foreach(var toUpdate in arr)
                {
                    var arr2 = toUpdate;
                    var ta = await session.RunInTransaction(() =>
                    {
                        var result2 = new List<KeyValuePair<ILink, PostStoreEntityId>>();

                        using (var table = OpenPostsTable(session, OpenTableGrbit.None))
                        {
                            var index = table.Indexes.TypeAndPostIdIndex;
                            index.SetAsCurrentIndex();
                            foreach (var entity in arr2)
                            {
                                string boardId;
                                int sequenceId;
                                if (entity.EntityType == PostStoreEntityType.Thread || entity.EntityType == PostStoreEntityType.Catalog)
                                {
                                    (boardId, sequenceId) = ExtractCatalogOrThreadLinkData(entity.Link);
                                } else if (entity.EntityType == PostStoreEntityType.BoardPage)
                                {
                                    (boardId, sequenceId) = ExtractBoardPageLinkData(entity.Link);
                                }
                                else
                                {
                                    throw new ArgumentException($"Неправильный тип сущности для добавления в минимальном виде {entity.EntityType}");
                                }
                                if (entity.StoreId == null && !index.Find(index.CreateKey((byte) entity.EntityType, boardId, sequenceId)))
                                {
                                    int id;
                                    using (var update = table.Insert.CreateUpdate())
                                    {
                                        table.Insert.PostDataBareEntityInsertView.Set(
                                            new PostsTable.ViewValues.PostDataBareEntityInsertView()
                                            {
                                                BoardId = boardId,
                                                ChildrenLoadStage = ChildrenLoadStageId.NotStarted,
                                                DataLoaded = false,
                                                EntityType = (byte) entity.EntityType,
                                                Thumbnail =
                                                    ObjectSerializationService.SerializeToBytes(entity.Thumbnail),
                                                SequenceNumber = sequenceId,
                                                DirectParentId = null,
                                                ParentId = new Int32ColumnValue[0],
                                                Subject = entity.Subject,
                                                ParentSequenceNumber = null

                                            }, true);
                                        id = table.Columns.Id_AutoincrementValue;
                                        update.Save();
                                    }
                                    result2.Add(new KeyValuePair<ILink, PostStoreEntityId>(entity.Link, new PostStoreEntityId() { Id = id }));
                                }
                                else if (entity.StoreId == null)
                                {
                                    result2.Add(new KeyValuePair<ILink, PostStoreEntityId>(entity.Link, new PostStoreEntityId()
                                    {
                                        Id = index.Views.RetrieveIdFromIndexView.Fetch().Id
                                    }));
                                }
                                else
                                {
                                    result2.Add(new KeyValuePair<ILink, PostStoreEntityId>(entity.Link, entity.StoreId.Value));
                                }
                            }
                        }

                        return (true, result2);
                    }, 2);
                    result.AddRange(ta);
                }
                return result;
            }

            var r = await ParallelizeOnSessions(
                list.SplitSet(30).Select(s => s.ToArray()).DistributeToProcess(UploadParallelism), Do);

            return r.SelectMany(s => s).Where(s => s.Key != null).Deduplicate(s => s.Key, BoardLinkEqualityComparer.Instance).ToList();
        }
    }

    internal struct PreSerializedPostData
    {
        public string LinkHash;
        public byte[] Document;
        public byte[][] Media;
        public byte[] OtherData;
        public int[] Quotes;
        public byte[] Thumbnail;
        public PreSeekPost PreSeek;
    }

    internal struct PreSeekPost
    {
        public string BoardId;
        public int ThreadId;
        public int PostId;
        public bool Exists;
        public PostStoreEntityId FoundId;
    }
}