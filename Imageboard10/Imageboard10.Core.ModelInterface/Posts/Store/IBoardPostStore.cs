using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.ModuleInterface;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Хранилище постов.
    /// </summary>
    public interface IBoardPostStore
    {
        /// <summary>
        /// Загрузить сущность.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="mode">Режим загрузки.</param>
        /// <returns>Сущность.</returns>
        IAsyncOperation<IBoardPostEntity> Load(Guid id, PostStoreLoadMode mode);

        /// <summary>
        /// Загрузить посты.
        /// </summary>
        /// <param name="ids">Идентификаторы.</param>
        /// <param name="mode">Режим загрузки.</param>
        /// <returns>Посты.</returns>
        [DefaultOverload]
        IAsyncOperation<IList<IBoardPostEntity>> Load(IList<Guid> ids, PostStoreLoadMode mode);

        /// <summary>
        /// Загрузить сущности.
        /// </summary>
        /// <param name="parentId">Идентификатор родительской сущности.</param>
        /// <param name="skip">Пропустить сущностей.</param>
        /// <param name="count">Сколько взять сущностей (максимально).</param>
        /// <param name="mode">Режим загрузки.</param>
        /// <returns>Посты.</returns>
        IAsyncOperation<IList<IBoardPostEntity>> Load(Guid? parentId, int skip, int? count, PostStoreLoadMode mode);

        /// <summary>
        /// Получить дочерние сущности.
        /// </summary>
        /// <param name="collectionId">Идентификатор коллекции.</param>
        /// <param name="skip">Пропустить постов.</param>
        /// <param name="count">Сколько взять постов (максимально).</param>
        /// <returns>Идентификаторы сущностей.</returns>
        IAsyncOperation<IList<Guid>> GetChildren(Guid collectionId, int skip, int? count);

        /// <summary>
        /// Получить количество постов в коллекции.
        /// </summary>
        /// <param name="collectionId">Идентификатор коллекции.</param>
        /// <returns>Количество постов.</returns>
        IAsyncOperation<int> GetCollectionSize(Guid collectionId);

        /// <summary>
        /// Получить общее количество сущностей в базе.
        /// </summary>
        /// <param name="type">Тип сущности.</param>
        /// <returns>Количество сущностей.</returns>
        IAsyncOperation<int> GetTotalSize(PostStoreEntityType type);

        /// <summary>
        /// Найти коллекцию.
        /// </summary>
        /// <param name="type">Тип сущности.</param>
        /// <param name="link">Ссылка на коллекцию.</param>
        /// <returns>Коллекция.</returns>
        IAsyncOperation<Guid> FindEntity(PostStoreEntityType type, ILink link);

        /// <summary>
        /// Найти коллекции.
        /// </summary>
        /// <param name="parentId">Идентификатор родительской коллекции.</param>
        /// <param name="links">Ссылки.</param>
        /// <returns>Коллекции.</returns>
        IAsyncOperation<IList<IPostStoreEntityIdSearchResult>> FindEntities(Guid? parentId, IList<ILink> links);

        /// <summary>
        /// Получить информацию о доступе.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Результат.</returns>
        IAsyncOperation<IBoardPostStoreAccessInfo> GetAccessInfo(Guid id);

        /// <summary>
        /// Получить информацию о доступе.
        /// </summary>
        /// <param name="ids">Идентификаторы.</param>
        /// <returns>Результат.</returns>
        IAsyncOperation<IList<IBoardPostStoreAccessInfo>> GetAccessInfos(IList<Guid> ids);

        /// <summary>
        /// Получить информацию о доступе для всех коллекций.
        /// </summary>
        /// <returns>Результат.</returns>
        IAsyncOperation<IList<IBoardPostStoreAccessInfo>> GetAllAccessInfos();

        /// <summary>
        /// Обновить информация об использовании. Вызов этого метода производит запись в лог доступа.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="accessTime">Время использования (null - текущее).</param>
        IAsyncAction Touch(Guid id, DateTimeOffset? accessTime);

        /// <summary>
        /// Получить ETAG коллекции.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>ETAG.</returns>
        IAsyncOperation<string> GetEtag(Guid id);

        /// <summary>
        /// Обновить ETAG.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="etag">ETAG.</param>
        IAsyncAction UpdateEtag(Guid id, string etag);

        /// <summary>
        /// Обновить информацию о коллекции.
        /// </summary>
        /// <param name="updateInfo">Информация об обновлении.</param>
        IAsyncAction SetCollectionUpdateInfo(IBoardPostCollectionUpdateInfo updateInfo);

        /// <summary>
        /// Обновить количество прочитанных постов.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="readPosts">Прочитано постов.</param>
        IAsyncAction SetReadPostsCount(Guid id, int readPosts);

        /// <summary>
        /// Сохранить коллекцию.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        /// <param name="replace">Режим замены.</param>
        /// <param name="cleanupPolicy">Политика зачистки старых данных. Если null - не производить зачистку.</param>
        /// <returns>Идентификатор коллекции.</returns>
        IAsyncOperationWithProgress<Guid, OperationProgress> SaveCollection(IBoardPostEntity collection, BoardPostCollectionUpdateMode replace, PostStoreStaleDataClearPolicy cleanupPolicy);

        /// <summary>
        /// Загрузить информацию о коллекции.
        /// </summary>
        /// <param name="collectionId">Идентификатор коллекции.</param>
        /// <returns>Результат.</returns>
        IAsyncOperation<IBoardPostCollectionInfoSet> LoadCollectionInfoSet(Guid collectionId);

        /// <summary>
        /// Обновить лайки.
        /// </summary>
        /// <param name="likes">Лайки.</param>
        IAsyncAction UpdateLikes(IList<IBoardPostLikesStoreInfo> likes);

        /// <summary>
        /// Загрузить информацию о лайках.
        /// </summary>
        /// <param name="ids">Идентификаторы.</param>
        /// <returns>Лайки.</returns>
        IAsyncOperation<IList<IBoardPostLikes>> LoadLikes(IList<Guid> ids);

        /// <summary>
        /// Обновить флаги сущности.
        /// </summary>
        /// <param name="flags">Флаги.</param>
        IAsyncAction UpdateFlags(IList<FlagUpdateAction> flags);

        /// <summary>
        /// Загрузить флаги сущности.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        IAsyncOperation<IList<Guid>> LoadFlags(Guid id);

        /// <summary>
        /// Получить ответы на этот пост.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Ответы.</returns>
        IAsyncOperation<IList<Guid>> GetPostQuotes(Guid id);

        /// <summary>
        /// Получить тип коллекции.
        /// </summary>
        /// <param name="collectionId">Идентификатор коллекции.</param>
        /// <returns>Тип коллекции.</returns>
        IAsyncOperation<PostStoreEntityType> GetCollectionType(Guid collectionId);

        /// <summary>
        /// Получить количество медиа-файлов сущности (рекурсивно).
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Количество.</returns>
        IAsyncOperation<int> GetMediaCount(Guid id);
        
        /// <summary>
        /// Получить медиафайлы сущности (рекурсивно).
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="skip">Сколько пропустить.</param>
        /// <param name="count">Сколько взять (максимально).</param>
        /// <returns>Медиафайлы.</returns>
        IAsyncOperation<IList<IPostMedia>> GetPostMedia(Guid id, int skip, int? count);

        /// <summary>
        /// Загрузить документ.
        /// </summary>
        /// <param name="id">Идентификатор сущности.</param>
        /// <returns>Документ.</returns>
        IAsyncOperation<IPostDocument> GetDocument(Guid id);

        /// <summary>
        /// Удалить. Удаление всегда производится рекурсивно.
        /// </summary>
        /// <param name="ids">Список сущностей.</param>
        /// <returns>Список идентификаторов удалённых сущностей.</returns>
        IAsyncOperation<IList<Guid>> Delete(IList<Guid> ids);

        /// <summary>
        /// Очистить все данные.
        /// </summary>
        IAsyncAction ClearAllData();

        /// <summary>
        /// Очистить старые данные.
        /// </summary>
        /// <param name="policy">Политика удаления старых данных.</param>
        IAsyncAction ClearStaleData(PostStoreStaleDataClearPolicy policy);

        /// <summary>
        /// Очистить незавершённые загрузки.
        /// </summary>
        IAsyncAction ClearUnfinishedData();

        /// <summary>
        /// Загрузить лог последнего доступа.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Лог доступа.</returns>
        IAsyncOperation<IList<IBoardPostStoreAccessLogItem>> GetAccessLog(PostStoreAccessLogQuery query);

        /// <summary>
        /// Запросить по флагам.
        /// </summary>
        /// <param name="type">Тип сущности.</param>
        /// <param name="parentId">Идентификатор родительской сущности.</param>
        /// <param name="havingFlags">Должен иметь флаги.</param>
        /// <param name="notHavingFlags">Не должен иметь флаги.</param>
        /// <returns>Результат.</returns>
        IAsyncOperation<IList<Guid>> QueryByFlags(PostStoreEntityType type, Guid? parentId, IList<Guid> havingFlags, IList<Guid> notHavingFlags);
        
        /// <summary>
        /// Очистить лог доступа.
        /// </summary>
        /// <param name="maxAgeSec">Максимальное время нахождения записи в логе в секундах.</param>
        IAsyncAction ClearAccessLog(double maxAgeSec);

        /// <summary>
        /// Синхронизировать лог доступа между устройствами.
        /// </summary>
        /// <param name="accessLog">Лог доступа из внешнего источника.</param>
        IAsyncAction SyncAccessLog(IList<IBoardPostStoreAccessLogItem> accessLog);
    }
}