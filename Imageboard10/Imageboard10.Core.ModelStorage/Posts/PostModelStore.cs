﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Tasks;
using Imageboard10.Core.Utility;
using Imageboard10.ModuleInterface;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Хранилище постов.
    /// </summary>
    public partial class PostModelStore : ModelStorageBase<IBoardPostStore>, IBoardPostStore, IStaticModuleQueryFilter
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="engineId">Идентификатор движка.</param>
        /// <param name="uploadParallelism">Количество потоков для загрузки данных в базу.</param>
        public PostModelStore(string engineId, int uploadParallelism = 5)
        {
            EngineId = engineId ?? throw new ArgumentNullException(nameof(engineId));
            UploadParallelism = uploadParallelism;
        }

        /// <summary>
        /// Количество потоков для загрузки данных в базу.
        /// </summary>
        protected readonly int UploadParallelism;

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

        /// <summary>
        /// Получить ссылку на сущность.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Ссылка.</returns>
        public IAsyncOperation<ILink> GetEntityLink(PostStoreEntityId id)
        {
            async Task<ILink> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var colids = Api.GetColumnDictionary(table.Session, table);
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            return GetLinkAtCurrentPosition(table, colids);
                        }
                        return null;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить ссылки на сущности.
        /// </summary>
        /// <param name="ids">Идентификаторы.</param>
        /// <returns>Ссылки.</returns>
        public IAsyncOperation<IList<ILinkWithStoreId>> GetEntityLinks(PostStoreEntityId[] ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            var ids2 = ids.Select(i => i.Id).Distinct().ToArray();

            async Task<IList<ILinkWithStoreId>> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    var result = new List<ILinkWithStoreId>();
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var colids = Api.GetColumnDictionary(table.Session, table);

                        foreach (var id in ids2)
                        {
                            Api.MakeKey(table.Session, table, id, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                            {
                                result.Add(new LinkWithStoreId()
                                {
                                    Link = GetLinkAtCurrentPosition(table, colids),
                                    Id = new PostStoreEntityId() { Id = id }
                                });
                            }
                        }
                    }
                    return result;
                });
            }

            return Do().AsAsyncOperation();
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

        /// <summary>
        /// Получить дочерние сущности.
        /// </summary>
        /// <param name="collectionId">Идентификатор коллекции.</param>
        /// <param name="skip">Пропустить постов.</param>
        /// <param name="count">Сколько взять постов (максимально).</param>
        /// <returns>Идентификаторы сущностей.</returns>
        public IAsyncOperation<IList<PostStoreEntityId>> GetChildren(PostStoreEntityId collectionId, int skip, int? count)
        {
            async Task<IList<PostStoreEntityId>> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var result = new List<PostStoreEntityId>();
                        Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.InThreadPostLink)));
                        Api.MakeKey(table.Session, table, collectionId.Id, MakeKeyGrbit.NewKey);
                        var colid = Api.GetTableColumnid(table.Session, table, ColumnNames.Id);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            int counted = 0;
                            bool skipped = true;
                            if (skip > 0)
                            {
                                skipped = Api.TryMove(table.Session, table, (JET_Move) skip, MoveGrbit.None);
                            }
                            if (skipped)
                            {
                                do
                                {
                                    counted++;
                                    if (counted > count)
                                    {
                                        break;
                                    }
                                    var id = Api.RetrieveColumnAsInt32(table.Session, table, colid) ?? -1;
                                    result.Add(new PostStoreEntityId() { Id = id});
                                } while (Api.TryMoveNext(table.Session, table.Table));
                            }
                        }
                        return result;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Статус загрузки дочерних сущностей.
        /// </summary>
        /// <param name="collectionId">Коллекция.</param>
        /// <returns>Результат.</returns>
        public IAsyncOperation<bool> IsChildrenLoaded(PostStoreEntityId collectionId)
        {
            async Task<bool> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.MakeKey(table.Session, table, collectionId.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            var ls = Api.RetrieveColumnAsByte(table.Session, table, Api.GetTableColumnid(table.Session, table, ColumnNames.ChildrenLoadStage));
                            if (ls == ChildrenLoadStageId.Completed)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить количество постов в коллекции.
        /// </summary>
        /// <param name="collectionId">Идентификатор коллекции.</param>
        /// <returns>Количество постов.</returns>
        public IAsyncOperation<int> GetCollectionSize(PostStoreEntityId collectionId)
        {
            async Task<int> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.ParentId)));
                        Api.MakeKey(table.Session, table, collectionId.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            int cnt;
                            Api.JetIndexRecordCount(table.Session, table, out cnt, int.MaxValue);
                            return cnt;
                        }
                        return 0;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить общее количество сущностей в базе.
        /// </summary>
        /// <param name="type">Тип сущности.</param>
        /// <returns>Количество сущностей.</returns>
        public IAsyncOperation<int> GetTotalSize(PostStoreEntityType? type)
        {
            async Task<int> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        int cnt = 0;
                        if (type != null)
                        {
                            Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.Type)));
                            Api.MakeKey(table.Session, table, (byte)(type.Value), MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                            {
                                Api.JetIndexRecordCount(table.Session, table, out cnt, int.MaxValue);
                            }
                        }
                        else
                        {
                            Api.JetIndexRecordCount(table.Session, table, out cnt, int.MaxValue);
                        }
                        return cnt;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Найти коллекцию.
        /// </summary>
        /// <param name="type">Тип сущности.</param>
        /// <param name="link">Ссылка на коллекцию.</param>
        /// <returns>Коллекция.</returns>
        public IAsyncOperation<PostStoreEntityId?> FindEntity(PostStoreEntityType type, ILink link)
        {
            async Task<PostStoreEntityId?> Do()
            {
                var key = ExtractLinkKey(type, link) ?? throw new ArgumentException($"Невозможно определить информацию для поиска из ссылки {link?.GetLinkHash()}.", nameof(link));

                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession<PostStoreEntityId?>(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.TypeAndPostId)));
                        Api.MakeKey(table.Session, table, (byte)type, MakeKeyGrbit.NewKey);
                        Api.MakeKey(table.Session, table, key.boardId, Encoding.Unicode, MakeKeyGrbit.None);
                        Api.MakeKey(table.Session, table, key.sequenceId, MakeKeyGrbit.None);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            var id = Api.RetrieveColumnAsInt32(table.Session, table, Api.GetTableColumnid(table.Session, table, ColumnNames.Id), RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                            if (id != null)
                            {
                                return new PostStoreEntityId()
                                {
                                    Id = id.Value
                                };
                            }
                        }
                        return null;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Найти коллекции.
        /// </summary>
        /// <param name="parentId">Идентификатор родительской коллекции.</param>
        /// <param name="links">Ссылки.</param>
        /// <returns>Коллекции.</returns>
        public IAsyncOperation<IList<IPostStoreEntityIdSearchResult>> FindEntities(PostStoreEntityId? parentId, IList<ILink> links)
        {
            async Task<IList<IPostStoreEntityIdSearchResult>> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                if (links == null || links.Count == 0)
                {
                    return new List<IPostStoreEntityIdSearchResult>();
                }
                var toFind = new List<(string boardId, int sequenceId, PostStoreEntityType entityType, ILink link)>();
                foreach (var l in links.Distinct(BoardLinkEqualityComparer.Instance))
                {
                    var key1 = ExtractLinkKey(l);
                    if (key1 != null)
                    {
                        var etypes = ToEntityTypes(key1.Value.entityType);
                        foreach (var et in etypes)
                        {
                            toFind.Add((key1.Value.boardId, key1.Value.sequenceId, et, l));
                        }
                    }
                }
                return await OpenSession(session =>
                {
                    var result = new List<IPostStoreEntityIdSearchResult>();
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        var colids = Api.GetColumnDictionary(table.Session, table);
                        Api.JetSetCurrentIndex(table.Session, table, GetIndexName(TableName, nameof(Indexes.TypeAndPostId)));
                        foreach (var key in toFind)
                        {
                            Api.MakeKey(table.Session, table, (byte)key.entityType, MakeKeyGrbit.NewKey);
                            Api.MakeKey(table.Session, table, key.boardId, Encoding.Unicode, MakeKeyGrbit.None);
                            Api.MakeKey(table.Session, table, key.sequenceId, MakeKeyGrbit.None);
                            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                            {
                                do
                                {
                                    bool skip = false;
                                    if (parentId != null)
                                    {
                                        var parents = EnumMultivalueColumn<Int32ColumnValue>(table, colids[ColumnNames.ParentId]);
                                        // ReSharper disable once SimplifyLinqExpression
                                        if (!parents.Any(c => c.Value == parentId.Value.Id))
                                        {
                                            skip = true;
                                        }
                                    }
                                    var idV = Api.RetrieveColumnAsInt32(table.Session, table.Table, colids[ColumnNames.Id], RetrieveColumnGrbit.RetrieveFromPrimaryBookmark);
                                    if (idV == null)
                                    {
                                        skip = true;
                                    }
                                    if (!skip)
                                    {
                                        result.Add(new PostStoreEntityIdSearchResult()
                                        {
                                            EntityType = key.entityType,
                                            Link = key.link,
                                            Id = new PostStoreEntityId() { Id = idV.Value }
                                        });
                                    }
                                } while (Api.TryMoveNext(table.Session, table));
                            }
                        }
                    }
                    return result;
                });
            }

            return Do().AsAsyncOperation();
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

        /// <summary>
        /// Получить ETAG коллекции.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>ETAG.</returns>
        public IAsyncOperation<string> GetEtag(PostStoreEntityId id)
        {
            async Task<string> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            return Api.RetrieveColumnAsString(table.Session, table.Table, table.GetColumnid(ColumnNames.Etag), Encoding.Unicode);
                        }
                        return null;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Обновить ETAG.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="etag">ETAG.</param>
        public IAsyncAction UpdateEtag(PostStoreEntityId id, string etag)
        {
            async Task Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                await OpenSessionAsync(async session =>
                {
                    await session.RunInTransaction(() =>
                    {
                        using (var table = session.OpenTable(TableName, OpenTableGrbit.Updatable))
                        {
                            Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                            {
                                using (var update = table.Update(JET_prep.Replace))
                                {
                                    Api.SetColumn(table.Session, table, table.GetColumnid(ColumnNames.Etag), etag, Encoding.Unicode);
                                    update.Save();
                                }
                            }
                        }
                        return true;
                    }, 1, CommitTransactionGrbit.LazyFlush);
                    return Nothing.Value;
                });
            }

            return Do().AsAsyncAction();
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
        /// <param name="replace">Режим замены.</param>
        /// <param name="cleanupPolicy">Политика зачистки старых данных. Если null - не производить зачистку.</param>
        /// <returns>Идентификатор коллекции.</returns>
        public IAsyncOperationWithProgress<PostStoreEntityId, OperationProgress> SaveCollection(IBoardPostEntity collection, BoardPostCollectionUpdateMode replace, PostStoreStaleDataClearPolicy cleanupPolicy)
        {
            return SaveCollection(collection, replace, cleanupPolicy, null);
        }

        /// <summary>
        /// Загрузить информацию о коллекции.
        /// </summary>
        /// <param name="collectionId">Идентификатор коллекции.</param>
        /// <returns>Результат.</returns>
        public IAsyncOperation<IBoardPostCollectionInfoSet> LoadCollectionInfoSet(PostStoreEntityId collectionId)
        {
            async Task<IBoardPostCollectionInfoSet> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.MakeKey(table.Session, table, collectionId.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table.Table, SeekGrbit.SeekEQ))
                        {
                            var entityType = (PostStoreEntityType)(Api.RetrieveColumnAsByte(table.Session, table.Table, table.GetColumnid(ColumnNames.EntityType)) ?? 0);
                            var genEntityType = ToGenericEntityType(entityType);
                            if (genEntityType == GenericPostStoreEntityType.Thread ||
                                genEntityType == GenericPostStoreEntityType.Catalog ||
                                genEntityType == GenericPostStoreEntityType.BoardPage)
                            {
                                var bt = Api.RetrieveColumn(table.Session, table, table.GetColumnid(ColumnNames.OtherDataBinary));
                                return ObjectSerializationService.Deserialize(bt) as IBoardPostCollectionInfoSet;
                            }
                        }
                        return null;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        public IAsyncAction UpdateLikes(IList<IBoardPostLikesStoreInfo> likes)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IList<IBoardPostLikes>> LoadLikes(IList<PostStoreEntityId> ids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Обновить флаги сущности.
        /// </summary>
        /// <param name="flags">Флаги.</param>
        public IAsyncAction UpdateFlags(IList<FlagUpdateAction> flags)
        {
            async Task Do()
            {
                if (flags == null) throw new ArgumentNullException(nameof(flags));

                CheckModuleReady();
                await WaitForTablesInitialize();

                var byId = flags.ToLookup(f => f.Id, PostStoreEntityIdEqualityComparer.Instance);

                await OpenSessionAsync(async session =>
                {
                    await session.RunInTransaction(() =>
                    {
                        using (var table = session.OpenTable(TableName, OpenTableGrbit.Updatable))
                        {
                            var colid = table.GetColumnid(ColumnNames.Flags);
                            foreach (var g in byId)
                            {
                                Api.MakeKey(table.Session, table, g.Key.Id, MakeKeyGrbit.NewKey);
                                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                                {
                                    using (var update = table.Update(JET_prep.Replace))
                                    {
                                        // ReSharper disable once PossibleInvalidOperationException
                                        var oldFlags = EnumMultivalueColumn<GuidColumnValue>(table, colid).Where(c => c?.Value != null).ToHashSet(c => c.Value.Value);
                                        foreach (var a in g)
                                        {
                                            switch (a.Action)
                                            {
                                                case FlagUpdateOperation.Add:
                                                    oldFlags.Add(a.Flag);
                                                    break;
                                                case FlagUpdateOperation.Remove:
                                                    oldFlags.Remove(a.Flag);
                                                    break;
                                                case FlagUpdateOperation.Clear:
                                                    oldFlags.Clear();
                                                    break;
                                            }
                                        }
                                        ClearMultiValue(table, colid);
                                        if (oldFlags.Count > 0)
                                        {
                                            var toSet = oldFlags.Select(f => new GuidColumnValue()
                                            {
                                                Value = f,
                                                Columnid = colid,
                                                SetGrbit = SetColumnGrbit.UniqueMultiValues,
                                                ItagSequence = 0
                                            }).OfType<ColumnValue>().ToArray();
                                            Api.SetColumns(table.Session, table, toSet);
                                        }
                                        update.Save();
                                    }
                                }
                            }
                        }
                        return true;
                    }, 1, CommitTransactionGrbit.LazyFlush);
                    return Nothing.Value;
                });
            }

            return Do().AsAsyncAction();
        }

        /// <summary>
        /// Загрузить флаги сущности.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        public IAsyncOperation<IList<Guid>> LoadFlags(PostStoreEntityId id)
        {
            async Task<IList<Guid>> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            return EnumMultivalueColumn<GuidColumnValue>(table, table.GetColumnid(ColumnNames.Flags)).Where(f => f?.Value != null).Select(f => f.Value.Value).ToList();
                        }
                        return null;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        public IAsyncOperation<IList<PostStoreEntityId>> GetPostQuotes(PostStoreEntityId id)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<PostStoreEntityType> GetCollectionType(PostStoreEntityId collectionId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получить количество медиа-файлов сущности (рекурсивно).
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Количество.</returns>
        public IAsyncOperation<int> GetMediaCount(PostStoreEntityId id)
        {
            async Task<int> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = session.OpenTable(MediaFilesTableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(table.Session, table, GetIndexName(MediaFilesTableName, nameof(MediaFilesIndexes.EntityReferences)));
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            int cnt;
                            Api.JetIndexRecordCount(table.Session, table.Table, out cnt, int.MaxValue);
                            return cnt;
                        }
                        return 0;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить медиафайлы сущности (рекурсивно).
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="skip">Сколько пропустить.</param>
        /// <param name="count">Сколько взять (максимально).</param>
        /// <returns>Медиафайлы.</returns>
        public IAsyncOperation<IList<IPostMedia>> GetPostMedia(PostStoreEntityId id, int skip, int? count)
        {
            async Task<IList<IPostMedia>> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = session.OpenTable(MediaFilesTableName, OpenTableGrbit.ReadOnly))
                    {
                        var result = new List<IPostMedia>();
                        var colid = table.GetColumnid(MediaFilesColumnNames.MediaData);
                        Api.JetSetCurrentIndex(table.Session, table, GetIndexName(MediaFilesTableName, nameof(MediaFilesIndexes.Sequences)));
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnStartLimit);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekGE))
                        {
                            Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnEndLimit);
                            Api.TrySetIndexRange(table.Session, table, SetIndexRangeGrbit.RangeUpperLimit);
                            int counted = 0;
                            bool skipped = true;
                            if (skip > 0)
                            {
                                skipped = Api.TryMove(table.Session, table, (JET_Move)skip, MoveGrbit.None);
                            }
                            if (skipped)
                            {
                                do
                                {
                                    counted++;
                                    if (counted > count)
                                    {
                                        break;
                                    }
                                    var bt = Api.RetrieveColumn(table.Session, table, colid);
                                    var pm = ObjectSerializationService.Deserialize(bt) as IPostMedia;
                                    if (pm != null)
                                    {
                                        result.Add(pm);
                                    }
                                } while (Api.TryMoveNext(table.Session, table.Table));
                            }
                        }
                        return result;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Загрузить документ.
        /// </summary>
        /// <param name="id">Идентификатор сущности.</param>
        /// <returns>Документ.</returns>
        public IAsyncOperation<IPostDocument> GetDocument(PostStoreEntityId id)
        {
            async Task<IPostDocument> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table.Table, SeekGrbit.SeekEQ))
                        {
                            var bt = Api.RetrieveColumn(table.Session, table, table.GetColumnid(ColumnNames.Document));
                            return ObjectSerializationService.Deserialize(bt) as IPostDocument;
                        }
                        return null;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Удалить. Удаление всегда производится рекурсивно.
        /// </summary>
        /// <param name="ids">Список сущностей.</param>
        /// <returns>Список идентификаторов удалённых сущностей.</returns>
        public IAsyncOperation<IList<PostStoreEntityId>> Delete(IList<PostStoreEntityId> ids)
        {
            async Task<IList<PostStoreEntityId>> Do()
            {
                if (ids == null) throw new ArgumentNullException(nameof(ids));
                CheckModuleReady();
                await WaitForTablesInitialize();

                var allEntities = await OpenSession(session =>
                {
                    using (var table = session.OpenTable(TableName, OpenTableGrbit.ReadOnly))
                    {
                        return FindAllChildren(table, ids).ToArray();
                    }
                });
                return await OpenSessionAsync(async session =>
                {
                    return await DoDeleteEntitiesList(session, allEntities.Select(e => e.id));
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Очистить все данные.
        /// </summary>
        public IAsyncAction ClearAllData()
        {
            async Task Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                bool foundAny = true;
                do
                {
                    await OpenSessionAsync(async session =>
                    {
                        await session.RunInTransaction(() =>
                        {
                            int counter = 200;
                            using (var table = session.OpenTable(TableName, OpenTableGrbit.Updatable))
                            {
                                if (Api.TryMoveFirst(table.Session, table))
                                {
                                    do
                                    {
                                        counter--;
                                        Api.JetDelete(table.Session, table);
                                    } while (counter > 0 && Api.TryMoveNext(table.Session, table));
                                }
                                else foundAny = false;
                            }
                            return true;
                        }, 1, CommitTransactionGrbit.None);
                        return Nothing.Value;
                    });
                } while (foundAny);
            }

            return Do().AsAsyncAction();
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

        private async Task DoClearUnfinishedData()
        {
            var toDelete = new Dictionary<int, List<int>>();
            var orphanParents = new HashSet<int>();
            await OpenSession(session =>
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
                await OpenSession(session =>
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
                        await OpenSessionAsync(async session =>
                        {
                            await DoDeleteEntitiesList(session, children.Select(c => new PostStoreEntityId() { Id = c}));
                            return Nothing.Value;
                        });
                        await SetEntityChildrenLoadStatus(new PostStoreEntityId() {Id = parentKey}, ChildrenLoadStageId.NotStarted);
                    }
                }
            }
        }


        /// <summary>
        /// Проверить запрос.
        /// </summary>
        /// <typeparam name="T">Тип запроса.</typeparam>
        /// <param name="query">Запрос.</param>
        /// <returns>Результат.</returns>
        bool IStaticModuleQueryFilter.CheckQuery<T>(T query)
        {
            if (typeof(T) == typeof(string))
            {
                var s = (string)(object)query;
                if (EngineId.Equals(s, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}