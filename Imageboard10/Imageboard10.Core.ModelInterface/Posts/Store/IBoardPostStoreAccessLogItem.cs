using System;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Элемент лога доступа.
    /// </summary>
    public interface IBoardPostStoreAccessLogItem
    {
        /// <summary>
        /// Идентификатор записи в логе.
        /// </summary>
        Guid? LogEntryId { get; }

        /// <summary>
        /// Ссылка на коллекцию.
        /// </summary>
        ILink Link { get; }

        /// <summary>
        /// Идентификатор.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Тип сущности.
        /// </summary>
        PostStoreEntityType EntityType { get; }

        /// <summary>
        /// Время доступа.
        /// </summary>
        DateTimeOffset AccessTime { get; }
    }
}