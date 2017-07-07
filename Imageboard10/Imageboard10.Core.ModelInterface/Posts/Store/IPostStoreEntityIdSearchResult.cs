using System;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Результат поиска сущностей.
    /// </summary>
    public interface IPostStoreEntityIdSearchResult
    {
        /// <summary>
        /// Ссылка.
        /// </summary>
        ILink Link { get; }

        /// <summary>
        /// Идентификатор.
        /// </summary>
        PostStoreEntityId Id { get; }

        /// <summary>
        /// Тип сущности.
        /// </summary>
        PostStoreEntityType EntityType { get; }
    }
}