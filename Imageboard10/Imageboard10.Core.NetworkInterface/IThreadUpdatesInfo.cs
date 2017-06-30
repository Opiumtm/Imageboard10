using System;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Информация об обновлениях треда.
    /// </summary>
    public interface IThreadUpdatesInfo
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        ILink Link { get; }

        /// <summary>
        /// Последний пост.
        /// </summary>
        ILink LastPost { get; }

        /// <summary>
        /// Количество постов.
        /// </summary>
        int NumberOfPosts { get; }

        /// <summary>
        /// Последний апдейт.
        /// </summary>
        DateTimeOffset LastUpdate { get; }
    }
}