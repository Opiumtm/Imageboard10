using System;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Действие по обновлению флага.
    /// </summary>
    public sealed class FlagUpdateAction
    {
        /// <summary>
        /// Идентификатор сущности.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Действие.
        /// </summary>
        public FlagUpdateOperation Action { get; set; }

        /// <summary>
        /// Флаг.
        /// </summary>
        public Guid Flag { get; set; }
    }
}