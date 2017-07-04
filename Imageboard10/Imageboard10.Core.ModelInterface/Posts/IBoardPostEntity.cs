using System;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts.Store;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Сущность коллекции постов.
    /// </summary>
    public interface IBoardPostEntity
    {
        /// <summary>
        /// Тип сущности.
        /// </summary>
        PostStoreEntityType EntityType { get; }

        /// <summary>
        /// Идентификатор в хранилище.
        /// </summary>
        Guid? StoreId { get; }

        /// <summary>
        /// Родительская сущность.
        /// </summary>
        Guid? StoreParentId { get; }

        /// <summary>
        /// Ссылка на пост.
        /// </summary>
        ILink Link { get; }

        /// <summary>
        /// Родительская ссылка.
        /// </summary>
        ILink ParentLink { get; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        string Subject { get; }

        /// <summary>
        /// Превью картинки поста.
        /// </summary>
        IPostMediaWithSize Thumbnail { get; }
    }
}