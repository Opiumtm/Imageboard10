using System;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Информация о доступе.
    /// </summary>
    public interface IBoardPostStoreAccessInfo
    {
        /// <summary>
        /// Ссылка на коллекцию.
        /// </summary>
        ILink Link { get; }

        /// <summary>
        /// Идентификатор.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Тип сущности.
        /// </summary>
        PostStoreEntityType EntityType { get; }

        /// <summary>
        /// Последнее обновление.
        /// </summary>
        DateTimeOffset LastUpdate { get; }

        /// <summary>
        /// Последняя загрузка.
        /// </summary>
        DateTimeOffset LastDownload { get; }

        /// <summary>
        /// Время доступа.
        /// </summary>
        DateTimeOffset AccessTime { get; }

        /// <summary>
        /// Количество постов.
        /// </summary>
        int NumberOfPosts { get; }

        /// <summary>
        /// Количество загруженных постов.
        /// </summary>
        int NumberOfLoadedPosts { get; }

        /// <summary>
        /// Количество прочитанных постов.
        /// </summary>
        int NumberOfReadPosts { get; }

        /// <summary>
        /// Последний пост.
        /// </summary>
        ILink LastPost { get; }

        /// <summary>
        /// Последний загруженный пост.
        /// </summary>
        ILink LastLoadedPost { get; }

        /// <summary>
        /// ETAG.
        /// </summary>
        string Etag { get; }

        /// <summary>
        /// Занесено в архив.
        /// </summary>
        bool IsArchived { get; }
    }
}