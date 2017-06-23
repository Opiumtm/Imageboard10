using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Коллекция тредов.
    /// </summary>
    public class BoardPageThreadCollection : IBoardPageThreadCollection
    {
        /// <summary>
        /// Ссылка.
        /// </summary>
        public ILink Link { get; set; }

        /// <summary>
        /// Родительская ссылка.
        /// </summary>
        public ILink ParentLink { get; set; }

        /// <summary>
        /// Посты.
        /// </summary>
        public IList<IThreadPreviewPostCollection> Threads { get; set; }

        /// <summary>
        /// Дополнительная информация.
        /// </summary>
        public IBoardPostCollectionInfoSet Info { get; set; }

        /// <summary>
        /// Штамп изменений.
        /// </summary>
        public string Etag { get; set; }
    }
}