using System;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Режим загрузки сущностей.
    /// </summary>
    public sealed class PostStoreLoadMode
    {
        /// <summary>
        /// Выяснить порядковый номер поста в треде.
        /// </summary>
        public bool RetrieveCounterNumber { get; set; }

        /// <summary>
        /// Режим загрузки постов.
        /// </summary>
        public PostStoreEntityLoadMode EntityLoadMode { get; set; }

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public PostStoreLoadMode Clone()
        {
            return new PostStoreLoadMode()
            {
                EntityLoadMode = EntityLoadMode,
                RetrieveCounterNumber = RetrieveCounterNumber
            };
        }
    }

    /// <summary>
    /// Реэтм загрузки постов.
    /// </summary>
    public enum PostStoreEntityLoadMode
    {
        /// <summary>
        /// Загрузка полных данных.
        /// </summary>
        Full,

        /// <summary>
        /// Облегчённые данные.
        /// </summary>
        Light,

        /// <summary>
        /// Только общую для всех типов информацию сущности.
        /// </summary>
        EntityOnly,

        /// <summary>
        /// Только ссылку.
        /// </summary>
        LinkOnly,
    }
}