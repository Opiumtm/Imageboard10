using System;
using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Коллекция тредов.
    /// </summary>
    public class BoardPageThreadCollection : IBoardPageThreadCollection
    {
        /// <summary>
        /// Тип сущности.
        /// </summary>
        public PostStoreEntityType EntityType { get; set; }

        /// <summary>
        /// Идентификатор в хранилище.
        /// </summary>
        public Guid? StoreId { get; set; }

        /// <summary>
        /// Родительская сущность.
        /// </summary>
        public Guid? StoreParentId { get; set; }

        /// <summary>
        /// Ссылка.
        /// </summary>
        public ILink Link { get; set; }

        /// <summary>
        /// Родительская ссылка.
        /// </summary>
        public ILink ParentLink { get; set; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        public string Subject => null;

        /// <summary>
        /// Превью поста.
        /// </summary>
        public IPostMediaWithSize Thumbnail => null;

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