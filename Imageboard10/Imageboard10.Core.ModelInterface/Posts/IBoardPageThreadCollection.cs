using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Коллекция тредов.
    /// </summary>
    public interface IBoardPageThreadCollection : IBoardPostEntity, IBoardPostCollectionInfoEnabled, IBoardPostCollectionEtagEnabled
    {
        /// <summary>
        /// Посты.
        /// </summary>
        IList<IThreadPreviewPostCollection> Threads { get; }

        /// <summary>
        /// Штамп изменений.
        /// </summary>
        string Etag { get; }
    }
}