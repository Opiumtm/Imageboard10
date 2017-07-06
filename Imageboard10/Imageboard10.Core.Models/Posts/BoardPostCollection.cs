using System;
using System.Collections.Generic;
using System.Linq;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Коллекция постов.
    /// </summary>
    public class BoardPostCollection : IBoardPostCollection, IBoardPostCollectionEtag
    {
        /// <summary>
        /// Тип сущности.
        /// </summary>
        public PostStoreEntityType EntityType { get; set; }

        /// <summary>
        /// Идентификатор в хранилище.
        /// </summary>
        public long? StoreId { get; set; }

        /// <summary>
        /// Родительская сущность.
        /// </summary>
        public long? StoreParentId { get; set; }

        /// <summary>
        /// Ссылка.
        /// </summary>
        public ILink Link { get; set; }

        /// <summary>
        /// Родительская ссылка.
        /// </summary>
        public ILink ParentLink { get; set; }

        /// <summary>
        /// Предварительно загруженный заголовок.
        /// </summary>
        public string SubjectPreload { get; set; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        public string Subject => SubjectPreload ?? Posts?.FirstOrDefault()?.Subject;

        /// <summary>
        /// Предварительно загруженное превью.
        /// </summary>
        public IPostMediaWithSize ThumbnailPreload { get; set; }

        /// <summary>
        /// Превью поста.
        /// </summary>
        public IPostMediaWithSize Thumbnail => ThumbnailPreload ?? Posts?.FirstOrDefault()?.Thumbnail;

        /// <summary>
        /// Посты.
        /// </summary>
        public IList<IBoardPost> Posts { get; set; }

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