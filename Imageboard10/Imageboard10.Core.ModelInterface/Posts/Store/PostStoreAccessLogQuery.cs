using System;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Запрос к логу доступа.
    /// </summary>
    public sealed class PostStoreAccessLogQuery
    {
        /// <summary>
        /// Тип сущности.
        /// </summary>
        public PostStoreEntityType EntityType { get; set; }

        /// <summary>
        /// Только последние данные по каждой сущности.
        /// </summary>
        public bool OnlyLatest { get; set; }

        /// <summary>
        /// Максимальный размер лога.
        /// </summary>
        public int? MaxLogSize { get; set; }

        /// <summary>
        /// Начиная со времени.
        /// </summary>
        public DateTimeOffset? From { get; set; }

        /// <summary>
        /// По время.
        /// </summary>
        public DateTimeOffset? To { get; set; }

        /// <summary>
        /// Фильтровать избранное. null - не рассматривать этот флаг.
        /// </summary>
        public bool? FilterFavorites { get; set; }
    }
}