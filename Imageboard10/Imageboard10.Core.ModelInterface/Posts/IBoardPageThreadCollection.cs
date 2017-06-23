using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Коллекция тредов.
    /// </summary>
    public interface IBoardPageThreadCollection
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
        IList<IThreadPreviewPostCollection> Threads { get; }

        /// <summary>
        /// Дополнительная информация.
        /// </summary>
        IBoardPostCollectionInfoSet Info { get; }

        /// <summary>
        /// Штамп изменений.
        /// </summary>
        string Etag { get; }
    }
}