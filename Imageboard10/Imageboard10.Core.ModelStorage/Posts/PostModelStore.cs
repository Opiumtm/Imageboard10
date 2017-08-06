﻿using System;
using System.Collections.Generic;
using System.Linq;
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var index = table.Indexes.PrimaryIndex;
                        if (index.Find(index.CreateKey(id.Id)))
                        {
                            return GetLinkAtCurrentPosition(table);
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var index = table.Indexes.PrimaryIndex;
                        foreach (var id in ids2)
                        {
                            if (index.Find(index.CreateKey(id)))
                            {
                                result.Add(new LinkWithStoreId()
                                {
                                    Link = GetLinkAtCurrentPosition(table),
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

        /// <summary>
        /// Загрузить сущность.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="mode">Режим загрузки.</param>
        /// <returns>Сущность.</returns>
        public IAsyncOperation<IBoardPostEntity> Load(PostStoreEntityId id, PostStoreLoadMode mode)
        {
            async Task<IBoardPostEntity> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                var r = await LoadEntities(new[] {id}, mode);
                return r.FirstOrDefault();
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Загрузить посты.
        /// </summary>
        /// <param name="ids">Идентификаторы.</param>
        /// <param name="mode">Режим загрузки.</param>
        /// <returns>Посты.</returns>
        public IAsyncOperation<IList<IBoardPostEntity>> Load(IList<PostStoreEntityId> ids, PostStoreLoadMode mode)
        {
            async Task<IList<IBoardPostEntity>> Do()
            {
                CheckModuleReady();
                if (ids == null) throw new ArgumentNullException(nameof(ids));
                await WaitForTablesInitialize();

                return await LoadEntities(ids, mode);
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Загрузить сущности.
        /// </summary>
        /// <param name="parentId">Идентификатор родительской сущности.</param>
        /// <param name="skip">Пропустить сущностей.</param>
        /// <param name="count">Сколько взять сущностей (максимально).</param>
        /// <param name="mode">Режим загрузки.</param>
        /// <returns>Посты.</returns>
        public IAsyncOperation<IList<IBoardPostEntity>> Load(PostStoreEntityId parentId, int skip, int? count, PostStoreLoadMode mode)
        {
            async Task<IList<IBoardPostEntity>> Do()
            {
                if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
                var children = await GetChildren(parentId, skip, count);
                var toLoad = new List<(PostStoreEntityId id, int counter)>();
                var counter = skip;
                foreach (var c in children)
                {
                    counter++;
                    toLoad.Add((c, counter));
                }
                return await LoadEntities(toLoad, mode);
            }

            return Do().AsAsyncOperation();
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var result = new List<PostStoreEntityId>();
                        var index = table.Indexes.InThreadPostLinkIndex;
                        index.SetAsCurrentIndex();
                        if (index.SeekPartial(index.CreateKey(collectionId.Id)))
                        {
                            foreach (var _ in table.EnumerateToEnd(skip, count))
                            {
                                var id = index.Views.RetrieveIdFromIndexView.Fetch();
                                result.Add(new PostStoreEntityId() { Id = id.Id });
                            }
                        }
                        return result;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить порядковый номер поста в треде.
        /// </summary>
        /// <param name="postLink">Ссылка на пост.</param>
        /// <param name="threadId">Идентификатор треда.</param>
        /// <returns>Результат.</returns>
        public IAsyncOperation<int?> GetPostCounterNumber(ILink postLink, PostStoreEntityId threadId)
        {
            async Task<int?> Do()
            {
                CheckModuleReady();
                if (postLink == null) throw new ArgumentNullException(nameof(postLink));
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    var linkData = ExtractPostLinkData(postLink);
                    return GetPostCounterNumber(session, threadId, linkData.postId);
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(collectionId.Id)))
                        {
                            var ls = table.Columns.ChildrenLoadStage;
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var index = table.Indexes.ParentIdIndex;
                        index.SetAsCurrentIndex();
                        return index.GetIndexRecordCount(index.CreateKey(collectionId.Id));
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        if (type != null)
                        {
                            var index = table.Indexes.TypeIndex;
                            index.SetAsCurrentIndex();
                            return index.GetIndexRecordCount(index.CreateKey((byte) type.Value));
                        }
                        return table.Indexes.PrimaryIndex.GetIndexRecordCount();
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var index = table.Indexes.TypeAndPostIdIndex;
                        index.SetAsCurrentIndex();
                        if (index.Find(index.CreateKey((byte) type, key.boardId, key.sequenceId)))
                        {
                            var id = index.Views.RetrieveIdFromIndexView.Fetch();
                            return new PostStoreEntityId()
                            {
                                Id = id.Id
                            };
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var index = table.Indexes.TypeAndPostIdIndex;
                        index.SetAsCurrentIndex();
                        foreach (var key in toFind)
                        {
                            var key1 = index.CreateKey((byte) key.entityType, key.boardId, key.sequenceId);
                            foreach (var v in index.EnumerateAsParentsAndIdViewForIndex(key1))
                            {
                                bool skip = false;
                                if (parentId != null)
                                {
                                    // ReSharper disable once SimplifyLinqExpression
                                    if (!v.ParentId.Any(c => c.Value == parentId.Value.Id))
                                    {
                                        skip = true;
                                    }
                                    if (!skip)
                                    {
                                        result.Add(new PostStoreEntityIdSearchResult()
                                        {
                                            EntityType = key.entityType,
                                            Link = key.link,
                                            Id = new PostStoreEntityId() { Id = v.Id }
                                        });
                                    }
                                }
                            }
                        }
                    }
                    return result;
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить информацию о доступе.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Результат.</returns>
        public IAsyncOperation<IBoardPostStoreAccessInfo> GetAccessInfo(PostStoreEntityId id)
        {
            async Task<IBoardPostStoreAccessInfo> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        if (GotoEntityId(table, id))
                        {
                            return LoadAccessInfo(session, table);
                        }
                        return null;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить информацию о доступе.
        /// </summary>
        /// <param name="ids">Идентификаторы.</param>
        /// <returns>Результат.</returns>
        public IAsyncOperation<IList<IBoardPostStoreAccessInfo>> GetAccessInfos(IList<PostStoreEntityId> ids)
        {
            async Task<IList<IBoardPostStoreAccessInfo>> Do()
            {
                CheckModuleReady();
                if (ids == null) throw new ArgumentNullException(nameof(ids));
                await WaitForTablesInitialize();

                var result = new List<IBoardPostStoreAccessInfo>();

                return await OpenSession(session =>
                {
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        foreach (var id in ids.Distinct(PostStoreEntityIdEqualityComparer.Instance))
                        {
                            if (GotoEntityId(table, id))
                            {
                                result.Add(LoadAccessInfo(session, table));
                            }
                        }
                        return result;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить информацию о доступе для всех коллекций.
        /// </summary>
        /// <param name="entityType">Тип сущности.</param>
        /// <returns>Результат.</returns>
        public IAsyncOperation<IList<IBoardPostStoreAccessInfo>> GetAllAccessInfos(PostStoreEntityType entityType)
        {
            async Task<IList<IBoardPostStoreAccessInfo>> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                var result = new List<IBoardPostStoreAccessInfo>();

                return await OpenSession(session =>
                {
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var index = table.Indexes.TypeIndex;
                        index.SetAsCurrentIndex();
                        foreach (var _ in index.Enumerate(index.CreateKey((byte) entityType)))
                        {
                            result.Add(LoadAccessInfo(session, table));
                        }
                        return result;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Обновить информация об использовании. Вызов этого метода производит запись в лог доступа.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="accessTime">Время использования (null - текущее).</param>
        /// <returns>Идентификатор записи лога доступа.</returns>
        public IAsyncOperation<Guid?> Touch(PostStoreEntityId id, DateTimeOffset? accessTime)
        {
            async Task<Guid?> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSessionAsync(async session =>
                {
                    Guid? result = null;
                    await session.RunInTransaction(() =>
                    {
                        using (var table = OpenPostsTable(session, OpenTableGrbit.None))
                        {
                            if (GotoEntityId(table, id))
                            {
                                var pt = (PostStoreEntityType) (table.Columns.EntityType);
                                var pt2 = ToGenericEntityType(pt);
                                if (pt2 == GenericPostStoreEntityType.Post)
                                {
                                    throw new InvalidOperationException($"Нельзя выполнять операцию для типа сущности {pt}");
                                }
                                int? rp = null;
                                if (pt == PostStoreEntityType.Thread)
                                {
                                    using (var postTable = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                                    {
                                        rp = CountDirectParent(postTable, id) - CountDirectParentWithFlag(postTable, id, BoardPostFlags.IsDeletedOnServer);
                                    }
                                }
                                var accTime = accessTime?.UtcDateTime ?? DateTime.Now.ToUniversalTime();
                                if (rp != null)
                                {
                                    table.Update.UpdateAsNumberOfReadPostsUpdateView(new PostsTable.ViewValues.NumberOfReadPostsUpdateView() { NumberOfReadPosts = rp });
                                }
                                using (var accessTable = OpenAccessLogTable(session, OpenTableGrbit.None))
                                {
                                    var newId = Guid.NewGuid();
                                    result = newId;
                                    accessTable.Insert.InsertAsInsertAllColumnsView(new AccessLogTable.ViewValues.InsertAllColumnsView()
                                    {
                                        Id = newId,
                                        EntityId = id.Id,
                                        AccessTime = accTime
                                    });
                                }
                            }
                        }
                        return true;
                    }, 2);
                    return result;
                });
            }

            return Do().AsAsyncOperation();
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        if (GotoEntityId(table, id))
                        {
                            return table.Columns.Etag;
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
                        using (var table = OpenPostsTable(session, OpenTableGrbit.Updatable))
                        {
                            if (GotoEntityId(table, id))
                            {
                                using (var update = table.Update.CreateUpdate())
                                {
                                    var c = table.Columns;
                                    c.Etag = etag;
                                    update.Save();
                                }
                            }
                        }
                        return true;
                    }, 1);
                    return Nothing.Value;
                });
            }

            return Do().AsAsyncAction();
        }

        /// <summary>
        /// Обновить информацию о коллекции.
        /// </summary>
        /// <param name="updateInfo">Информация об обновлении.</param>
        public IAsyncAction SetCollectionUpdateInfo(IBoardPostCollectionUpdateInfo updateInfo)
        {
            async Task Do()
            {
                CheckModuleReady();
                if (updateInfo == null) throw new ArgumentNullException(nameof(updateInfo));
                await WaitForTablesInitialize();

                await OpenSessionAsync(async session =>
                {
                    await session.RunInTransaction(() =>
                    {
                        using (var table = OpenPostsTable(session, OpenTableGrbit.Updatable))
                        {
                            if (!GotoEntityId(table, updateInfo.Id))
                            {
                                throw new InvalidOperationException($"Не найдена сущность с идентификатором {updateInfo.Id.Id}");
                            }
                            var entityType = (PostStoreEntityType) table.Columns.EntityType;
                            if (entityType != PostStoreEntityType.Thread)
                            {
                                throw new InvalidOperationException($"Нельзя устанавливать информацию об обновлениях на сервера для сущности типа {entityType}");
                            }

                            int? lastPost = null;
                            if (updateInfo.LastPost is PostLink pl)
                            {
                                var li = table.Views.LinkInfoView.Fetch();
                                if (pl.Engine == EngineId && pl.Board == li.BoardId && pl.OpPostNum == li.SequenceNumber)
                                {
                                    lastPost = pl.PostNum;
                                }
                            }
                            table.Update.UpdateAsPostCollectionUpdateInfoView(new PostsTable.ViewValues.PostCollectionUpdateInfoView()
                            {
                                LastPostLinkOnServer = lastPost,
                                LastServerUpdate = updateInfo.LastUpdate.UtcDateTime,
                                NumberOfPostsOnServer = updateInfo.NumberOfPosts
                            });
                        }

                        return true;
                    }, 1);
                    return Nothing.Value;
                });
            }

            return Do().AsAsyncAction();
        }

        /// <summary>
        /// Обновить количество прочитанных постов.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="readPosts">Прочитано постов.</param>
        /// <param name="keepMax">Держать максимальное значение.</param>
        public IAsyncAction SetReadPostsCount(PostStoreEntityId id, int readPosts, bool keepMax)
        {
            async Task Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                await OpenSessionAsync(async session =>
                {
                    await session.RunInTransaction(() =>
                    {
                        using (var table = OpenPostsTable(session, OpenTableGrbit.Updatable))
                        {
                            if (!GotoEntityId(table, id))
                            {
                                throw new InvalidOperationException($"Не найдена сущность с идентификатором {id.Id}");
                            }

                            var entityType = (PostStoreEntityType)table.Columns.EntityType;
                            var genType = ToGenericEntityType(entityType);
                            if (genType != GenericPostStoreEntityType.Thread && genType != GenericPostStoreEntityType.BoardPage && genType != GenericPostStoreEntityType.Catalog)
                            {
                                throw new InvalidOperationException($"Нельзя устанавливать информацию о прочитанных постах для сущности типа {entityType}");
                            }
                            if (!keepMax)
                            {
                                table.Update.UpdateAsNumberOfReadPostsUpdateView(new PostsTable.ViewValues.NumberOfReadPostsUpdateView()
                                {
                                    NumberOfReadPosts = readPosts
                                });
                            }
                            else
                            {
                                var v = table.Views.NumberOfReadPostsUpdateView.Fetch();
                                if (v.NumberOfReadPosts == null || readPosts > v.NumberOfReadPosts)
                                {
                                    v.NumberOfReadPosts = readPosts;
                                    table.Update.UpdateAsNumberOfReadPostsUpdateView(v);
                                }
                            }
                        }

                        return true;
                    }, 1);

                    return Nothing.Value;
                });
            }

            return Do().AsAsyncAction();
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        if (GotoEntityId(table, collectionId))
                        {
                            var entityType = (PostStoreEntityType)table.Columns.EntityType;
                            var genEntityType = ToGenericEntityType(entityType);
                            if (genEntityType == GenericPostStoreEntityType.Thread ||
                                genEntityType == GenericPostStoreEntityType.Catalog ||
                                genEntityType == GenericPostStoreEntityType.BoardPage)
                            {
                                var bt = table.Columns.OtherDataBinary;
                                return ObjectSerializationService.Deserialize(bt) as IBoardPostCollectionInfoSet;
                            }
                        }
                        return null;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Обновить лайки.
        /// </summary>
        /// <param name="likes">Лайки.</param>
        public IAsyncAction UpdateLikes(IList<IBoardPostLikesStoreInfo> likes)
        {
            async ValueTask<Nothing> DoProcess(IEsentSession session, IList<IBoardPostLikesStoreInfo[]> data)
            {
                foreach (var likes2 in data)
                {
                    await session.RunInTransaction(() =>
                    {
                        using (var table = OpenPostsTable(session, OpenTableGrbit.Updatable))
                        {
                            foreach (var l in likes2)
                            {
                                if (l != null)
                                {
                                    if (GotoEntityId(table, l.Id))
                                    {
                                        table.Update.LikesUpdateView.Set(new PostsTable.ViewValues.LikesUpdateView()
                                        {
                                            Likes = l.Likes?.Likes,
                                            Dislikes = l.Likes?.Dislikes
                                        });
                                    }
                                }
                            }
                        }
                        return true;
                    }, 1.5);
                }
                return Nothing.Value;
            }

            async Task Do()
            {
                CheckModuleReady();
                if (likes == null) throw new ArgumentNullException(nameof(likes));
                await WaitForTablesInitialize();

                var toProcess = likes.SplitSet(25).Select(s => s.ToArray()).DistributeToProcess(3);
                await ParallelizeOnSessions(toProcess, DoProcess);
            }

            return Do().AsAsyncAction();
        }

        /// <summary>
        /// Загрузить информацию о лайках.
        /// </summary>
        /// <param name="ids">Идентификаторы.</param>
        /// <returns>Лайки.</returns>
        public IAsyncOperation<IList<IBoardPostLikesInfo>> LoadLikes(IList<PostStoreEntityId> ids)
        {
            async Task<IList<IBoardPostLikesInfo>> Do()
            {
                CheckModuleReady();
                if (ids == null) throw new ArgumentNullException(nameof(ids));
                await WaitForTablesInitialize();
                return await OpenSession(session =>
                {
                    var result = new List<IBoardPostLikesInfo>();
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        foreach (var id in ids)
                        {
                            if (GotoEntityId(table, id))
                            {
                                var likes = table.Views.LikesUpdateView.Fetch();
                                if (likes.Likes != null || likes.Dislikes != null)
                                {
                                    result.Add(new LikesInfo()
                                    {
                                        StoreId = id,
                                        Likes = new BoardPostLikes() { Likes = likes.Likes ?? 0, Dislikes = likes.Dislikes ?? 0 }
                                    });
                                }
                            }
                        }
                    }
                    return result;
                });
            }

            return Do().AsAsyncOperation();
        }

        private class LikesInfo : IBoardPostLikesInfo
        {
            public PostStoreEntityId StoreId { get; set; }
            public IBoardPostLikes Likes { get; set; }
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
                        using (var table = OpenPostsTable(session, OpenTableGrbit.Updatable))
                        {
                            foreach (var g in byId)
                            {
                                if (GotoEntityId(table, g.Key))
                                {
                                    using (var update = table.Update.CreateUpdate())
                                    {
                                        // ReSharper disable once PossibleInvalidOperationException
                                        var columns = table.Columns;
                                        var oldFlags = columns.Flags.Values.Where(c => c?.Value != null).ToHashSet(c => c.Value.Value);
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
                                        columns.SetFlagsValueArr(oldFlags.Select(id => new GuidColumnValue()
                                        {
                                            Value = id,
                                            SetGrbit = SetColumnGrbit.UniqueMultiValues

                                        }).ToArray(), false);
                                        update.Save();
                                    }
                                }
                            }
                        }
                        return true;
                    }, 1);
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        if (GotoEntityId(table, id))
                        {
                            return table.Columns.Flags.Values.Where(f => f?.Value != null).Select(f => f.Value.Value).ToList();
                        }
                        return null;
                    }
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить ответы на этот пост.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Ответы.</returns>
        public IAsyncOperation<IList<PostStoreEntityId>> GetPostQuotes(PostStoreEntityId id)
        {
            async Task<IList<PostStoreEntityId>> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    var result = new HashSet<PostStoreEntityId>(PostStoreEntityIdEqualityComparer.Instance);
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        if (GotoEntityId(table, id))
                        {
                            var entityType = (PostStoreEntityType)table.Columns.EntityType;
                            var genEntityType = ToGenericEntityType(entityType);
                            if (genEntityType == GenericPostStoreEntityType.Post)
                            {
                                var v = table.Views.DirectParentAndSequenceNumberView.Fetch();
                                if (v.DirectParentId != null)
                                {
                                    var index = table.Indexes.QuotedPostsIndex;
                                    index.SetAsCurrentIndex();
                                    foreach (var id2 in index.EnumerateAsRetrieveIdFromIndexView(index.CreateKey(v.DirectParentId.Value, v.SequenceNumber)))
                                    {
                                        result.Add(new PostStoreEntityId() { Id = id2.Id });
                                    }
                                }
                            }
                        }
                    }
                    return result.ToList();
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Получить тип коллекции.
        /// </summary>
        /// <param name="collectionId">Идентификатор коллекции.</param>
        /// <returns>Тип коллекции.</returns>
        public IAsyncOperation<PostStoreEntityType> GetCollectionType(PostStoreEntityId collectionId)
        {
            async Task<PostStoreEntityType> Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                return await OpenSession(session =>
                {
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        if (GotoEntityId(table, collectionId))
                        {
                            return (PostStoreEntityType) table.Columns.EntityType;
                        }
                        return PostStoreEntityType.Post;
                    }
                });
            }

            return Do().AsAsyncOperation();
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
                    using (var table = OpenMediaFilesTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var index = table.Indexes.EntityReferencesIndex;
                        index.SetAsCurrentIndex();
                        return index.GetIndexRecordCount(index.CreateKey(id.Id));
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
                    using (var table = OpenMediaFilesTable(session, OpenTableGrbit.ReadOnly))
                    {
                        var result = new List<IPostMedia>();
                        var index = table.Indexes.SequencesIndex;
                        index.SetAsCurrentIndex();
                        if (index.SeekPartial(index.CreateKey(id.Id)))
                        {
                            foreach (var _ in table.EnumerateToEnd(skip, count))
                            {
                                var pm = ObjectSerializationService.Deserialize(table.Columns.MediaData) as IPostMedia;
                                if (pm != null)
                                {
                                    result.Add(pm);
                                }
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        if (GotoEntityId(table, id))
                        {
                            return ObjectSerializationService.Deserialize(table.Columns.Document) as IPostDocument;
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
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
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
                            using (var table = OpenPostsTable(session, OpenTableGrbit.Updatable))
                            {
                                if (table.TryMoveFirst())
                                {
                                    do
                                    {
                                        counter--;
                                        table.DeleteCurrentRow();
                                    } while (counter > 0 && table.TryMoveNext());
                                }
                                else foundAny = false;
                            }
                            return true;
                        }, 1);
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

        /// <summary>
        /// Запросить по флагам.
        /// </summary>
        /// <param name="type">Тип сущности.</param>
        /// <param name="parentId">Идентификатор родительской сущности.</param>
        /// <param name="havingFlags">Должен иметь флаги.</param>
        /// <param name="notHavingFlags">Не должен иметь флаги.</param>
        /// <returns>Результат.</returns>
        public IAsyncOperation<IList<PostStoreEntityId>> QueryByFlags(PostStoreEntityType type, PostStoreEntityId? parentId, IList<Guid> havingFlags, IList<Guid> notHavingFlags)
        {
            async Task<IList<PostStoreEntityId>> Do()
            {
                CheckModuleReady();
                if (havingFlags == null) throw new ArgumentNullException(nameof(havingFlags));
                if (havingFlags.Count < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(havingFlags), "Нужно указать как минимум 1 флаг для проверки");
                }
                await WaitForTablesInitialize();
                return await OpenSessionAsync(async session =>
                {
                    var bookmarks = await session.Run(() =>
                    {
                        using (var toDispose = new CompositeDisposable(null))
                        {
                            List<PostsTable> tables = new List<PostsTable>();

                            var mainTable = OpenPostsTable(session, OpenTableGrbit.ReadOnly);
                            toDispose.AddDisposable(mainTable);
                            tables.Add(mainTable);

                            if (parentId != null)
                            {
                                var index = mainTable.Indexes.ParentIdIndex;
                                index.SetAsCurrentIndex();
                                if (!index.Find(index.CreateKey(parentId.Value.Id)))
                                {
                                    goto CancelLabel;
                                }
                            }
                            else
                            {
                                var index = mainTable.Indexes.TypeIndex;
                                index.SetAsCurrentIndex();
                                if (!index.Find(index.CreateKey((byte) type)))
                                {
                                    goto CancelLabel;
                                }
                            }

                            foreach (var f in havingFlags)
                            {
                                var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly);
                                toDispose.AddDisposable(table);
                                tables.Add(table);
                                var index = table.Indexes.FlagsIndex;
                                index.SetAsCurrentIndex();
                                if (!index.Find(index.CreateKey(f)))
                                {
                                    goto CancelLabel;
                                }
                            }

                            return Api.IntersectIndexes(session.Session, tables.Select(t => t.Table).ToArray()).ToArray();

                            CancelLabel:
                            return null;
                        }
                    });

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
                                if (table.TryGotoBookmark(bm))
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
                });
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Очистить лог доступа.
        /// </summary>
        /// <param name="maxAgeSec">Максимальное время нахождения записи в логе в секундах.</param>
        public IAsyncAction ClearAccessLog(double maxAgeSec)
        {
            async Task Do()
            {
                CheckModuleReady();
                await WaitForTablesInitialize();

                await OpenSessionAsync(async session =>
                {
                    var toDelete = await session.Run(() =>
                    {
                        var td = new List<Guid>();
                        using (var table = OpenAccessLogTable(session, OpenTableGrbit.ReadOnly))
                        {
                            var index = table.Indexes.AccessTimeDescIndex;
                            index.SetAsCurrentIndex();
                            var maxAge = DateTime.Now.AddSeconds(-1 * maxAgeSec).ToUniversalTime();
                            index.SetKey(index.CreateKey(maxAge));
                            if (table.TrySeek(SeekGrbit.SeekLT))
                            {
                                do
                                {
                                    td.Add(index.Views.IdForIndex.Fetch().Id);
                                } while (table.TryMoveNext());
                            }
                            return td;
                        }
                    });
                    foreach (var toDelete2 in toDelete.SplitSet(100))
                    {
                        var td = toDelete2.ToArray();
                        await session.RunInTransaction(() =>
                        {
                            using (var table = OpenAccessLogTable(session, OpenTableGrbit.None))
                            {
                                foreach (var id in td)
                                {
                                    if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(id)))
                                    {
                                        table.DeleteCurrentRow();
                                    }
                                }
                            }
                            return true;
                        }, 1.5);
                    }
                    return Nothing.Value;
                });
            }

            return Do().AsAsyncAction();
        }

        public IAsyncAction SyncAccessLog(IList<IBoardPostStoreAccessLogItem> accessLog)
        {
            throw new NotImplementedException();
        }

        private async Task DoClearUnfinishedData()
        {
            var toDelete = new Dictionary<int, HashSet<int>>();
            var orphanParents = new HashSet<int>();
            await OpenSession(session =>
            {
                using (var parTable = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                {
                    using (var idTable = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        foreach (var id in FindAllParents(parTable))
                        {
                            if (GotoEntityId(idTable, id))
                            {
                                orphanParents.Add(id.Id);
                            }
                        }
                    }
                }
                using (var incTable = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                {
                    var index = incTable.Indexes.ChildrenLoadStageIndex;
                    foreach (var id in index.EnumerateAsRetrieveIdFromIndexView(index.CreateKey(ChildrenLoadStageId.Started)))
                    {
                        orphanParents.Add(id.Id);
                    }
                }
                return Nothing.Value;
            });
            if (orphanParents.Count > 0)
            {
                await OpenSession(session =>
                {
                    using (var table = OpenPostsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        foreach (var child in FindAllChildren(table, orphanParents.Select(p => new PostStoreEntityId() { Id = p })))
                        {
                            if (!toDelete.ContainsKey(child.parentId.Id))
                            {
                                toDelete[child.parentId.Id] = new HashSet<int>();
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