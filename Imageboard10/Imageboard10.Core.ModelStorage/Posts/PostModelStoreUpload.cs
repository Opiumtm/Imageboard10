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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetIntMultivalue(EsentTable table, JET_COLUMNID colid, PostStoreEntityId[] values)
        {
            if (values.Length == 0)
            {
                return;
            }
            JET_SETCOLUMN[] columns = new JET_SETCOLUMN[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                var bytes = BitConverter.GetBytes(values[i].Id);
                columns[i] = new JET_SETCOLUMN()
                {
                    pvData = bytes,
                    cbData = bytes.Length,
                    columnid = colid,
                    itagSequence = i + 1,
                    ibLongValue = 0,
                    ibData = 0
                };
            }
            Api.JetSetColumns(table.Session, table, columns, columns.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetIntMultivalue(EsentTable table, JET_COLUMNID colid, int[] values)
        {
            if (values.Length == 0)
            {
                return;
            }
            JET_SETCOLUMN[] columns = new JET_SETCOLUMN[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                var bytes = BitConverter.GetBytes(values[i]);
                columns[i] = new JET_SETCOLUMN()
                {
                    pvData = bytes,
                    cbData = bytes.Length,
                    columnid = colid,
                    itagSequence = i + 1,
                    ibLongValue = 0,
                    ibData = 0
                };
            }
            Api.JetSetColumns(table.Session, table, columns, columns.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetGuidMultivalue(EsentTable table, JET_COLUMNID colid, Guid[] values)
        {
            if (values.Length == 0)
            {
                return;
            }
            JET_SETCOLUMN[] columns = new JET_SETCOLUMN[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                var bytes = values[i].ToByteArray();
                columns[i] = new JET_SETCOLUMN()
                {
                    pvData = bytes,
                    cbData = bytes.Length,
                    columnid = colid,
                    itagSequence = i + 1,
                    ibLongValue = 0,
                    ibData = 0
                };
            }
            Api.JetSetColumns(table.Session, table, columns, columns.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetStringMultivalue(EsentTable table, JET_COLUMNID colid, string[] values)
        {
            if (values.Length == 0)
            {
                return;
            }
            JET_SETCOLUMN[] columns = new JET_SETCOLUMN[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                var bytes = Encoding.Unicode.GetBytes(values[i]);
                columns[i] = new JET_SETCOLUMN()
                {
                    pvData = bytes,
                    cbData = bytes.Length,
                    columnid = colid,
                    itagSequence = i + 1,
                    ibLongValue = 0,
                    ibData = 0
                };
            }
            Api.JetSetColumns(table.Session, table, columns, columns.Length);
        }

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

            PreSerializedPostData PreSerialize(EsentTable table, IBoardPost post, PostStoreEntityId directParent, bool parentExists)
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

            void SetPostFields(EsentTable table, IBoardPost post, PostStoreEntityId[] parents, PostStoreEntityId directParent, IDictionary<string, JET_COLUMNID> colids, ref PostStoreEntityId newId, bool exists, string boardId, int threadId, int postId, PreSerializedPostData preSerialized)
            {
                using (var update = new Update(table.Session, table.Table, exists ? JET_prep.Replace : JET_prep.Insert))
                {
                    if (!exists)
                    {
                        SetIntMultivalue(table, colids[ColumnNames.ParentId], parents);
                        Api.SetColumn(table.Session, table, colids[ColumnNames.DirectParentId], directParent.Id);
                        Api.SetColumn(table.Session, table, colids[ColumnNames.EntityType], (byte)post.EntityType);
                        Api.SetColumn(table.Session, table, colids[ColumnNames.DataLoaded], true);
                        Api.SetColumn(table.Session, table, colids[ColumnNames.ChildrenLoadStage], ChildrenLoadStageId.NotStarted);
                        Api.SetColumn(table.Session, table, colids[ColumnNames.BoardId], boardId, Encoding.Unicode);
                        Api.SetColumn(table.Session, table, colids[ColumnNames.ParentSequenceNumber], threadId);
                        Api.SetColumn(table.Session, table, colids[ColumnNames.SequenceNumber], postId);
                        newId = new PostStoreEntityId()
                        {
                            Id = Api.RetrieveColumnAsInt32(table.Session, table, colids[ColumnNames.Id], RetrieveColumnGrbit.RetrieveCopy) ?? -1
                        };
                    }
                    Api.SetColumn(table.Session, table, colids[ColumnNames.Subject], post.Subject, Encoding.Unicode);
                    Api.SetColumn(table.Session, table, colids[ColumnNames.Thumbnail], preSerialized.Thumbnail);
                    Api.SetColumn(table.Session, table, colids[ColumnNames.Date], post.Date.UtcDateTime);
                    Api.SetColumn(table.Session, table, colids[ColumnNames.BoardSpecificDate], post.BoardSpecificDate, Encoding.Unicode);
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
                    List<Guid> toSet = new List<Guid>();
                    foreach (var f in (post.Flags ?? new List<Guid>()).Where(serverFlags.Contains).Concat(keepFlags).Distinct())
                    {
                        if (f == UnitTestStoreFlags.ShouldFail)
                        {
                            throw new UnitTestStoreException();
                        }
                        toSet.Add(f);
                    }
                    SetGuidMultivalue(table, colids[ColumnNames.Flags], toSet.ToArray());
                    if (exists)
                    {
                        ClearMultiValue(table, colids[ColumnNames.ThreadTags]);
                    }
                    if (post.Tags?.Tags != null && post.Tags.Tags.Count > 0)
                    {
                        SetStringMultivalue(table, colids[ColumnNames.ThreadTags], post.Tags.Tags.Where(t => !string.IsNullOrEmpty(t)).ToArray());
                    }
                    var likes = post.Likes?.Likes;
                    var dislikes = post.Likes?.Dislikes;
                    if (likes != null)
                    {
                        Api.SetColumn(table.Session, table, colids[ColumnNames.Likes], likes.Value);
                    }
                    else
                    {
                        Api.SetColumn(table.Session, table, colids[ColumnNames.Likes], null);
                    }
                    if (dislikes != null)
                    {
                        Api.SetColumn(table.Session, table, colids[ColumnNames.Dislikes], dislikes.Value);
                    }
                    else
                    {
                        Api.SetColumn(table.Session, table, colids[ColumnNames.Dislikes], null);
                    }

                    Api.SetColumn(table.Session, table, colids[ColumnNames.Document], preSerialized.Document);
                    if (exists)
                    {
                        ClearMultiValue(table, colids[ColumnNames.QuotedPosts]);
                    }
                    SetIntMultivalue(table, colids[ColumnNames.QuotedPosts], preSerialized.Quotes);
                    Api.SetColumn(table.Session, table, colids[ColumnNames.LoadedTime], post.LoadedTime.UtcDateTime);
                    Api.SetColumn(table.Session, table, colids[ColumnNames.PosterName], post.Poster?.Name, Encoding.Unicode);
                    var onServerCount = (post as IBoardPostOnServerCounter)?.OnServerCounter;
                    if (onServerCount != null)
                    {
                        Api.SetColumn(table.Session, table, colids[ColumnNames.OnServerSequenceCounter], onServerCount.Value);
                    }
                    else
                    {
                        Api.SetColumn(table.Session, table, colids[ColumnNames.OnServerSequenceCounter], null);
                    }
                    Api.SetColumn(table.Session, table, colids[ColumnNames.OtherDataBinary], preSerialized.OtherData);
                    update.Save();
                }
            }

            void SetPostMedia(EsentTable mediaTable, IDictionary<string, JET_COLUMNID> mediaColids, PostStoreEntityId[] parents, PostStoreEntityId newId, int postId, bool exists, PreSerializedPostData preSerialized)
            {
                if (exists)
                {
                    Api.JetSetCurrentIndex(mediaTable.Session, mediaTable.Table, GetIndexName(MediaFilesTableName, nameof(MediaFilesIndexes.EntityReferences)));
                    Api.MakeKey(mediaTable.Session, mediaTable.Table, newId.Id, MakeKeyGrbit.NewKey);
                    if (Api.TrySeek(mediaTable.Session, mediaTable.Table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                    {
                        do
                        {
                            Api.JetDelete(mediaTable.Session, mediaTable.Table);
                        } while (Api.TryMoveNext(mediaTable.Session, mediaTable));
                    }
                }
                if (preSerialized.Media != null && preSerialized.Media.Length > 0)
                {
                    for (var i = 0; i < preSerialized.Media.Length; i++)
                    {
                        using (var update = new Update(mediaTable.Session, mediaTable, JET_prep.Insert))
                        {
                            var parentInts = new PostStoreEntityId[parents.Length + 1];
                            parentInts[0] = newId;
                            for (var j = 0; j < parents.Length; j++)
                            {
                                parentInts[j + 1] = parents[j];
                            }
                            SetIntMultivalue(mediaTable, mediaColids[MediaFilesColumnNames.EntityReferences], parentInts);
                            Api.SetColumn(mediaTable.Session, mediaTable, mediaColids[MediaFilesColumnNames.SequenceNumber], CreateMediaSequenceId(postId, i));
                            Api.SetColumn(mediaTable.Session, mediaTable, mediaColids[MediaFilesColumnNames.MediaData], preSerialized.Media[i]);
                            update.Save();
                        }
                    }
                }
            }

            PreSeekPost PreSeekPost(EsentTable table, IBoardPost post, PostStoreEntityId directParent, bool parentExists)
            {
                var postLink = post?.Link;
                CheckLinkEngine(postLink);
                (var boardId, var threadId, var postId) = ExtractPostLinkData(postLink);
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
                else
                {
                    return new PreSeekPost()
                    {
                        Exists = false,
                        ThreadId = threadId,
                        BoardId = boardId,
                        FoundId = new PostStoreEntityId() { Id = -1 },
                        PostId = postId
                    };
                }
            }

            (PostStoreEntityId id, bool exists) SavePost(EsentTable table, EsentTable mediaTable, IBoardPost post, PostStoreEntityId[] parents, PostStoreEntityId directParent, IDictionary<string, JET_COLUMNID> colids, IDictionary<string, JET_COLUMNID> mediaColids, PreSerializedPostData preSerialized)
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
                    Api.JetSetCurrentIndex(table.Session, table, null);
                    Api.MakeKey(table.Session, table.Table, newId.Id, MakeKeyGrbit.NewKey);
                    if (!Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                    {
                        throw new InvalidOperationException($"Сущность с идентификатором {newId.Id} не найдена");
                    }
                }

                SetPostFields(table, post, parents, directParent, colids, ref newId, exists, boardId, threadId, postId, preSerialized);

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
                SetPostMedia(mediaTable, mediaColids, parents, newId, postId, exists, preSerialized);
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
                                using (var table = session.OpenTable(TableName, OpenTableGrbit.Updatable))
                                {
                                    var colids = Api.GetColumnDictionary(table.Session, table);
                                    using (var mediaTable = session.OpenTable(MediaFilesTableName, OpenTableGrbit.Updatable))
                                    {
                                        var mediaColids = Api.GetColumnDictionary(mediaTable.Session, mediaTable);
                                        foreach (var post in toSave)
                                        {
                                            var pre = PreSerialize(table, post, collectionId, exists);
                                            var r = SavePost(table, mediaTable, post, parents, collectionId, colids, mediaColids, pre);
                                            if (!r.exists)
                                            {
                                                toAdd.Add(r.id);
                                            }
                                        }
                                    }
                                }
                                return (true, toAdd);
                            }, 2, CommitTransactionGrbit.LazyFlush);
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
                                await OpenSessionAsync(async session =>
                                {
                                    await session.RunInTransaction(() =>
                                    {
                                        using (var table = session.OpenTable(TableName, OpenTableGrbit.Updatable))
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
                                    }, 2, CommitTransactionGrbit.LazyFlush);
                                    return Nothing.Value;
                                });
                            }
                        }
                        else
                        {
                            foreach (var part in children.SplitSet(50))
                            {
                                var partArr = part.ToArray();
                                await OpenSessionAsync(async session =>
                                {
                                    await session.RunInTransaction(() =>
                                    {
                                        using (var table = session.OpenTable(TableName, OpenTableGrbit.Updatable))
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
                                    }, 2, CommitTransactionGrbit.LazyFlush);
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

                var collectionId = await OpenSessionAsync(async session =>
                {
                    return await session.RunInTransaction(() =>
                    {
                        PostStoreEntityId newId;
                        (var boardId, var sequenceId) = ExtractCatalogOrThreadLinkData(collection2.Link);

                        using (var table = session.OpenTable(TableName, OpenTableGrbit.Updatable))
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
                    }, 2, CommitTransactionGrbit.LazyFlush);
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
                        await OpenSessionAsync(async session =>
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
                var collectionId = await OpenSessionAsync(async session =>
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
                    }, 2, CommitTransactionGrbit.LazyFlush);
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
                    progress.Report(new OperationProgress() { Progress = null, Message = progressMessage, OperationId = progressId });

                    PostStoreEntityId addedEntity = new PostStoreEntityId() { Id = -1 };

                    if (collection.EntityType == PostStoreEntityType.Catalog || collection.EntityType == PostStoreEntityType.Thread)
                    {
                        addedEntity = await SaveCatalogOrThread(token, progress, null, true, addedEntities);
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