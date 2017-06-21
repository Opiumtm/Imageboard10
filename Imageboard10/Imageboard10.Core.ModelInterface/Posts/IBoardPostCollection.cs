using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Коллекция постов.
    /// </summary>
    public interface IBoardPostCollection
    {
        /// <summary>
        /// Ссылка.
        /// </summary>
        ILink Link { get; }

        /// <summary>
        /// Родительская ссылка.
        /// </summary>
        ILink ParentLink { get; }

        /// <summary>
        /// Посты.
        /// </summary>
        IList<IBoardPost> Posts { get; }

        /// <summary>
        /// Дополнительная информация.
        /// </summary>
        IList<IBoardPostCollectionInfo> Info { get; }
    }
}