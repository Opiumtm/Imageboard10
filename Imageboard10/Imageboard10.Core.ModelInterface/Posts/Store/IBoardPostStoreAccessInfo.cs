using System;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Информация о доступе.
    /// </summary>
    public interface IBoardPostStoreAccessInfo : IBoardPostStoreAccessLogItem
    {
        /// <summary>
        /// Последнее обновление.
        /// </summary>
        DateTimeOffset LastUpdate { get; }

        /// <summary>
        /// Последняя загрузка.
        /// </summary>
        DateTimeOffset LastDownload { get; }

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

        /// <summary>
        /// Находится в избранном.
        /// </summary>
        bool IsFavorite { get; }
    }
}