using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Коллекция постов.
    /// </summary>
    public interface IBoardPostCollection : IBoardPostEntity
    {
        /// <summary>
        /// Посты.
        /// </summary>
        IList<IBoardPost> Posts { get; }

        /// <summary>
        /// Дополнительная информация.
        /// </summary>
        IBoardPostCollectionInfoSet Info { get; }
    }
}