using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;
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

            void SetPostFields(EsentTable table, IBoardPost post, PostStoreEntityId[] parents, PostStoreEntityId directParent, IDictionary<string, JET_COLUMNID> colids, ref PostStoreEntityId newId, bool exists, string boardId, int threadId, int postId)
            {
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
                        var flags = EnumMultivalueColumn<GuidColumnValue>(table, colids[ColumnNames.Flags])
                            .Where(g => g.Value != null && !serverFlags.Contains(g.Value.Value) && g.Value.Value != BoardPostFlags.IsDeletedOnServer)
                            .Select(g => g.Value.Value);
                        foreach (var f in flags)
                        {
                            keepFlags.Add(f);
                        }
                        ClearMultiValue(table, colids[ColumnNames.Flags]);
                    }
                    foreach (var f in (post.Flags ?? new List<Guid>()).Where(serverFlags.Contains).Concat(keepFlags).Distinct())
                    {
                        if (f == UnitTestStoreFlags.ShouldFail)
                        {
                            throw new UnitTestStoreException();
                        }
                        toUpdate.Add(new GuidColumnValue()
                        {
                            Columnid = colids[ColumnNames.Flags],
                            ItagSequence = 0,
                            Value = f,
                            SetGrbit = SetColumnGrbit.UniqueMultiValues
                        });
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
            }

            void SetPostMedia(EsentTable mediaTable, IDictionary<string, JET_COLUMNID> mediaColids, IBoardPost post, PostStoreEntityId[] parents, PostStoreEntityId newId, int postId, bool exists)
            {
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
                                    Columnid = mediaColids[MediaFilesColumnNames.SequenceNumber],
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
            }

            (PostStoreEntityId id, bool exists) SavePost(EsentTable table, EsentTable mediaTable, IBoardPost post, PostStoreEntityId[] parents, PostStoreEntityId directParent, IDictionary<string, JET_COLUMNID> colids, IDictionary<string, JET_COLUMNID> mediaColids)
            {
                if (post == null) throw new ArgumentNullException(nameof(post));
                CheckLinkEngine(post.Link);
                (var boardId, var threadId, var postId) = ExtractPostLinkData(post.Link);
                if (post.EntityType != PostStoreEntityType.Post && post.EntityType != PostStoreEntityType.ThreadPreviewPost && post.EntityType != PostStoreEntityType.CatalogPost)
                {
                    throw new InvalidOperationException($"Неправильный тип сущности поста {post.EntityType}");
                }

                var exists = SeekExistingEntityInSequence(table, directParent, postId, out var newId);
                if (post.Flags?.Any(f => f == UnitTestStoreFlags.AlwaysInsert) ?? false)
                {
                    exists = false;
                }
                if (exists)
                {
                    Api.JetSetCurrentIndex(table.Session, table, null);
                    Api.MakeKey(table.Session, table.Table, newId.Id, MakeKeyGrbit.NewKey);
                    if (!Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                    {
                        throw new InvalidOperationException($"Сущность с идентификатором {newId.Id} не найдена");
                    }
                }

                SetPostFields(table, post, parents, directParent, colids, ref newId, exists, boardId, threadId, postId);

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
                SetPostMedia(mediaTable, mediaColids, post, parents, newId, postId, exists);
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

            void SetPostCollectionFields<T>(IEsentSession session, EsentTable table, IDictionary<string, JET_COLUMNID> colids, PostStoreEntityId? directParent, ref PostStoreEntityId newId, bool exists, T collection2, string boardId,
                int sequenceId)
                where T : class, IBoardPostEntity, IBoardPostCollectionInfoEnabled
            {
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
                    Api.JetSetCurrentIndex(table.Session, table, null);
                    Api.MakeKey(table.Session, table, newId.Id, MakeKeyGrbit.NewKey);
                    if (!Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                    {
                        throw new InvalidOperationException("Не найден тред, для которого производится слияние постов (seek)");
                    }
                    var childMode = Api.RetrieveColumnAsByte(table.Session, table, colids[ColumnNames.ChildrenLoadStage]);
                    if (childMode != ChildrenLoadStageId.Completed)
                    {
                        throw new InvalidOperationException("Нельзя добавлять посты в ещё не загруженный тред");
                    }
                    var toUpdate = new List<ColumnValue>();
                    var collection3 = collection2 as IBoardPostCollection;
                    var lastPost = collection3?.Posts?.OrderByDescending(ExtractPostNumber).FirstOrDefault();
                    if (lastPost != null)
                    {
                        toUpdate.Add(new DateTimeColumnValue()
                        {
                            Value = lastPost.Date.UtcDateTime,
                            Columnid = colids[ColumnNames.Date]
                        });
                        toUpdate.Add(new StringColumnValue()
                        {
                            Value = lastPost.BoardSpecificDate,
                            Columnid = colids[ColumnNames.BoardSpecificDate]
                        });
                        toUpdate.Add(new DateTimeColumnValue()
                        {
                            Value = lastPost.LoadedTime.UtcDateTime,
                            Columnid = colids[ColumnNames.LoadedTime]
                        });
                        toUpdate.Add(new DateTimeColumnValue()
                        {
                            Value = lastPost.LoadedTime.UtcDateTime,
                            Columnid = colids[ColumnNames.LastServerUpdate]
                        });
                        if (lastPost.Link is PostLink pl)
                        {
                            toUpdate.Add(new Int32ColumnValue()
                            {
                                Value = pl.PostNum,
                                Columnid = colids[ColumnNames.LastPostLinkOnServer]
                            });
                        }
                    }
                    int delta = 0;
                    if (collection3?.Posts != null && collection3.Posts.Count > 0)
                    {
                        using (var postTable = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                        {
                            Api.JetSetCurrentIndex(postTable.Session, postTable.Table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
                            foreach (var p in collection3.Posts)
                            {
                                if (p.Link is PostLink pl)
                                {
                                    var seqId = pl.PostNum;
                                    Api.MakeKey(postTable.Session, postTable.Table, newId.Id, MakeKeyGrbit.NewKey);
                                    Api.MakeKey(postTable.Session, postTable.Table, seqId, MakeKeyGrbit.None);
                                    if (!Api.TrySeek(postTable.Session, postTable, SeekGrbit.SeekEQ))
                                    {
                                        delta++;
                                    }
                                }
                            }
                        }
                    }
                    if (delta > 0)
                    {
                        var onServerCount = Api.RetrieveColumnAsInt32(table.Session, table.Table, colids[ColumnNames.NumberOfPostsOnServer]) ?? 0;
                        toUpdate.Add(new Int32ColumnValue()
                        {
                            Value = onServerCount + delta,
                            Columnid = colids[ColumnNames.NumberOfPostsOnServer]
                        });
                    }
                    if (collection2 is IBoardPostCollectionEtagEnabled ee)
                    {
                        toUpdate.Add(new StringColumnValue()
                        {
                            Value = ee.Etag,
                            Columnid = colids[ColumnNames.Etag]
                        });
                    }
                    if (toUpdate.Count > 0)
                    {
                        using (var update = new Update(table.Session, table, JET_prep.Replace))
                        {
                            Api.SetColumns(table.Session, table.Table, toUpdate.ToArray());
                            update.Save();
                        }
                    }
                }
                else
                {
                    if (exists)
                    {
                        Api.JetSetCurrentIndex(table.Session, table, null);
                        Api.MakeKey(table.Session, table.Table, newId.Id, MakeKeyGrbit.NewKey);
                        if (!Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            throw new InvalidOperationException($"Сущность с идентификатором {newId.Id} не найдена");
                        }
                    }
                    using (var update = new Update(table.Session, table, exists ? JET_prep.Replace : JET_prep.Insert))
                    {
                        var toUpdate = new List<ColumnValue>();
                        if (!exists)
                        {
                            if (directParent != null)
                            {
                                toUpdate.Add(new Int32ColumnValue()
                                {
                                    Columnid = colids[ColumnNames.ParentId],
                                    ItagSequence = 0,
                                    Value = directParent.Value.Id,
                                    SetGrbit = SetColumnGrbit.UniqueMultiValues
                                });
                                toUpdate.Add(new Int32ColumnValue()
                                {
                                    Value = directParent.Value.Id,
                                    Columnid = colids[ColumnNames.DirectParentId]
                                });
                            }
                            toUpdate.Add(new ByteColumnValue()
                            {
                                Value = (byte)collection2.EntityType,
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
                                Value = sequenceId,
                                Columnid = colids[ColumnNames.SequenceNumber]
                            });
                            newId = new PostStoreEntityId()
                            {
                                Id = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.Id], RetrieveColumnGrbit.RetrieveCopy) ?? -1
                            };
                        }
                        toUpdate.Add(new StringColumnValue()
                        {
                            Value = collection2.Subject,
                            Columnid = colids[ColumnNames.Subject]
                        });
                        toUpdate.Add(new BytesColumnValue()
                        {
                            Value = ObjectSerializationService.SerializeToBytes(collection2.Thumbnail),
                            Columnid = colids[ColumnNames.Thumbnail]
                        });
                        var collection3 = collection2 as IBoardPostCollection;
                        if ((collection2.EntityType == PostStoreEntityType.Thread || collection2.EntityType == PostStoreEntityType.ThreadPreview) && collection3 != null)
                        {
                            var lastPost = collection3.Posts?.OrderByDescending(ExtractPostNumber).FirstOrDefault();
                            var firstPost = collection3.Posts?.OrderBy(ExtractPostNumber).FirstOrDefault();
                            if (lastPost != null)
                            {
                                toUpdate.Add(new DateTimeColumnValue()
                                {
                                    Value = lastPost.Date.UtcDateTime,
                                    Columnid = colids[ColumnNames.Date]
                                });
                                toUpdate.Add(new StringColumnValue()
                                {
                                    Value = lastPost.BoardSpecificDate,
                                    Columnid = colids[ColumnNames.BoardSpecificDate]
                                });
                                toUpdate.Add(new DateTimeColumnValue()
                                {
                                    Value = lastPost.LoadedTime.UtcDateTime,
                                    Columnid = colids[ColumnNames.LoadedTime]
                                });
                                toUpdate.Add(new DateTimeColumnValue()
                                {
                                    Value = lastPost.LoadedTime.UtcDateTime,
                                    Columnid = colids[ColumnNames.LastServerUpdate]
                                });
                                if (lastPost.Link is PostLink pl)
                                {
                                    toUpdate.Add(new Int32ColumnValue()
                                    {
                                        Value = pl.PostNum,
                                        Columnid = colids[ColumnNames.LastPostLinkOnServer]
                                    });
                                }
                            }
                            if (firstPost != null)
                            {
                                if (exists)
                                {
                                    ClearMultiValue(table, colids[ColumnNames.ThreadTags]);
                                }
                                if (firstPost.Tags?.Tags != null && firstPost.Tags.Tags.Count > 0)
                                {
                                    foreach (var t in firstPost.Tags.Tags)
                                    {
                                        toUpdate.Add(new StringColumnValue()
                                        {
                                            Value = t,
                                            Columnid = colids[ColumnNames.ThreadTags]
                                        });
                                    }
                                }
                            }
                        }
                        var keepFlags = new List<Guid>();
                        if (exists)
                        {
                            var flags = EnumMultivalueColumn<GuidColumnValue>(table, colids[ColumnNames.Flags])
                                .Where(g => g.Value != null && !serverFlags.Contains(g.Value.Value))
                                .Select(g => g.Value.Value);
                            foreach (var f in flags)
                            {
                                keepFlags.Add(f);
                            }
                            ClearMultiValue(table, colids[ColumnNames.Flags]);
                        }
                        var infoFlags = ExtractInfo<T, IBoardPostCollectionInfoFlags>(collection2)?.Flags ?? new List<Guid>();
                        foreach (var f in infoFlags.Where(serverFlags.Contains).Concat(keepFlags).Distinct())
                        {
                            if (f == UnitTestStoreFlags.ShouldFail)
                            {
                                throw new UnitTestStoreException();
                            }
                            toUpdate.Add(new GuidColumnValue()
                            {
                                Columnid = colids[ColumnNames.Flags],
                                ItagSequence = 0,
                                Value = f,
                                SetGrbit = SetColumnGrbit.UniqueMultiValues
                            });
                        }
                        toUpdate.Add(new BytesColumnValue()
                        {
                            Value = ObjectSerializationService.SerializeToBytes(collection2.Info),
                            Columnid = colids[ColumnNames.OtherDataBinary]
                        });

                        if (collection2.EntityType == PostStoreEntityType.ThreadPreview && collection2 is IThreadPreviewPostCollection collection4)
                        {
                            toUpdate.Add(new BytesColumnValue()
                            {
                                Value = WriteThreadPreviewCounts(collection4),
                                Columnid = colids[ColumnNames.PreviewCounts]
                            });
                        }

                        if (collection2.EntityType == PostStoreEntityType.Thread)
                        {
                            toUpdate.Add(new Int32ColumnValue()
                            {
                                Value = collection3?.Posts?.Count ?? 0,
                                Columnid = colids[ColumnNames.NumberOfPostsOnServer]
                            });
                        }

                        if (collection2 is IBoardPostCollectionEtagEnabled ee)
                        {
                            toUpdate.Add(new StringColumnValue()
                            {
                                Value = ee.Etag,
                                Columnid = colids[ColumnNames.Etag]
                            });
                        }

                        Api.SetColumns(table.Session, table.Table, toUpdate.ToArray());
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
                    foreach (var p in collection2.Posts.SplitSet(20))
                    {
                        token.ThrowIfCancellationRequested();
                        var toSave = p.ToArray();

                        await UpdateAsync(async session =>
                        {
                            var saved = await session.RunInTransaction(() =>
                            {
                                var toAdd = new List<PostStoreEntityId>();
                                using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                                {
                                    var colids = Api.GetColumnDictionary(table.Session, table);
                                    using (var mediaTable = session.OpenTable(MediaFilesTableName, OpenTableGrbit.DenyWrite))
                                    {
                                        var mediaColids = Api.GetColumnDictionary(mediaTable.Session, mediaTable);
                                        foreach (var post in toSave)
                                        {
                                            if (post != null)
                                            {
                                                var r = SavePost(table, mediaTable, post, parents, collectionId, colids, mediaColids);
                                                if (!r.exists)
                                                {
                                                    toAdd.Add(r.id);
                                                }
                                            }
                                        }
                                    }
                                }
                                return (true, toAdd);
                            });
                            foreach (var id in saved)
                            {
                                addedEntities.Add(id);
                            }
                            return Nothing.Value;
                        });

                        savedCount += toSave.Length;
                        if (reportProgress && allCount > 0.01)
                        {
                            progress?.Report(new OperationProgress()
                            {
                                Progress = savedCount / allCount,
                                Message = progressMessage,
                                OperationId = progressId
                            });
                        }
                    }
                }

                await SetEntityChildrenLoadStatus(collectionId, ChildrenLoadStageId.Completed);
            }

            async Task CleanChildPostsOnReplace(IBoardPostCollection collection2, PostStoreEntityId collectionId, bool exists)
            {
                if (replace != BoardPostCollectionUpdateMode.Merge && exists)
                {
                    var children = await QueryReadonly(session =>
                    {
                        using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
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
                        if (replace == BoardPostCollectionUpdateMode.Replace || collection2.EntityType != PostStoreEntityType.Thread)
                        {
                            foreach (var part in children.SplitSet(50))
                            {
                                var partArr = part.ToArray();
                                await UpdateAsync(async session =>
                                {
                                    await session.RunInTransaction(() =>
                                    {
                                        using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                                        {
                                            Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
                                            foreach (var id in partArr)
                                            {
                                                Api.MakeKey(table.Session, table, collectionId.Id, MakeKeyGrbit.NewKey);
                                                Api.MakeKey(table.Session, table, id, MakeKeyGrbit.None);
                                                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                                                {
                                                    Api.JetDelete(table.Session, table);
                                                }
                                            }
                                        }

                                        return true;
                                    });
                                    return Nothing.Value;
                                });
                            }
                        }
                        else
                        {
                            foreach (var part in children.SplitSet(50))
                            {
                                var partArr = part.ToArray();
                                await UpdateAsync(async session =>
                                {
                                    await session.RunInTransaction(() =>
                                    {
                                        using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                                        {
                                            var colid = Api.GetTableColumnid(table.Session, table.Table, ColumnNames.Flags);
                                            Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
                                            foreach (var id in partArr)
                                            {
                                                Api.MakeKey(table.Session, table, collectionId.Id, MakeKeyGrbit.NewKey);
                                                Api.MakeKey(table.Session, table, id, MakeKeyGrbit.None);
                                                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                                                {
                                                    var haveFlag = EnumMultivalueColumn<GuidColumnValue>(table, colid).Any(f => f.Value == BoardPostFlags.IsDeletedOnServer);
                                                    if (!haveFlag)
                                                    {
                                                        using (var update = new Update(table.Session, table, JET_prep.Replace))
                                                        {
                                                            Api.SetColumns(table.Session, table, new GuidColumnValue()
                                                            {
                                                                Value = BoardPostFlags.IsDeletedOnServer,
                                                                Columnid = colid,
                                                                ItagSequence = 0
                                                            });
                                                            update.Save();
                                                        }
                                                    }
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

            async Task<PostStoreEntityId> SaveCatalogOrThread(CancellationToken token, IProgress<OperationProgress> progress, PostStoreEntityId? directParent, bool reportProgress, ConcurrentBag<PostStoreEntityId> addedEntities)
            {
                token.ThrowIfCancellationRequested();
                var collection2 = collection as IBoardPostCollection;
                if (collection2 == null)
                {
                    throw new ArgumentException("Неравильный тип объекта для треда или каталога", nameof(collection));
                }

                bool exists = false;

                var collectionId = await UpdateAsync(async session =>
                {
                    return await session.RunInTransaction(() =>
                    {
                        PostStoreEntityId newId;
                        (var boardId, var sequenceId) = ExtractCatalogOrThreadLinkData(collection2.Link);

                        using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                        {
                            var colids = Api.GetColumnDictionary(table.Session, table);
                            if (collection2.EntityType == PostStoreEntityType.ThreadPreview)
                            {
                                if (!(collection is IThreadPreviewPostCollection))
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

                            SetPostCollectionFields(session, table, colids, directParent, ref newId, exists, collection2, boardId, sequenceId);

                            return (true, newId);
                        }
                    });
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
                    var children = await QueryReadonly(session =>
                    {
                        using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
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
                        await QueryReadonly(async session =>
                        {
                            await session.Run(() =>
                            {
                                using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                                {
                                    using (var ct = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                                    {
                                        Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
                                        var colid = Api.GetTableColumnid(table.Session, table, ColumnNames.Id);
                                        foreach (var id in children)
                                        {
                                            Api.MakeKey(table.Session, table, collectionId.Id, MakeKeyGrbit.NewKey);
                                            Api.MakeKey(table.Session, table, id, MakeKeyGrbit.None);
                                            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                                            {
                                                var iid = Api.RetrieveColumnAsInt32(table.Session, table, colid, RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                                                if (iid != null)
                                                {
                                                    toDelete.Add(iid.Value);
                                                    foreach (var c in FindAllChildren(ct, new PostStoreEntityId {Id = iid.Value}).Where(c => c.parentId.Id == iid.Value))
                                                    {
                                                        toDelete.Add(c.id.Id);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            });
                            return Nothing.Value;
                        });
                        await UpdateAsync(async session =>
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
                    await SaveCatalogOrThread(token, progress, collectionId, false, addedEntities);

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
                var collectionId = await UpdateAsync(async session =>
                {
                    return await session.RunInTransaction(() =>
                    {
                        PostStoreEntityId newId;
                        (var boardId, var sequenceId) = ExtractBoardPageLinkData(collection2.Link);

                        using (var table = session.OpenTable(TableName, OpenTableGrbit.DenyWrite))
                        {
                            var colids = Api.GetColumnDictionary(table.Session, table);
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

                            SetPostCollectionFields(session, table, colids, null, ref newId, exists, collection2, boardId, sequenceId);

                            return (true, newId);
                        }
                    });
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
                        await UpdateAsync(async session =>
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
                    progress.Report(new OperationProgress() { Progress = null, Message = progressMessage, OperationId = progressId });

                    PostStoreEntityId addedEntity = new PostStoreEntityId() { Id = -1 };

                    if (collection.EntityType == PostStoreEntityType.Catalog || collection.EntityType == PostStoreEntityType.Thread)
                    {
                        addedEntity = await SaveCatalogOrThread(token, progress, null, true, addedEntities);
                    } else if (collection.EntityType == PostStoreEntityType.BoardPage)
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

    }
}