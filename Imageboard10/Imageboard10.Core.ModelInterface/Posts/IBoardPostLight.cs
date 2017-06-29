using System;
using System.Collections.Generic;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Облегчённая информация о посте.
    /// </summary>
    public interface IBoardPostLight : IBoardPostEntity
    {
        /// <summary>
        /// Дата поста.
        /// </summary>
        DateTimeOffset Date { get; }

        /// <summary>
        /// Порядковый номер поста (0 - без номера).
        /// </summary>
        int Counter { get; }

        /// <summary>
        /// Дата, как она отображается на доске.
        /// </summary>
        string BoardSpecificDate { get; }

        /// <summary>
        /// Флаги поста.
        /// </summary>
        IList<Guid> Flags { get; }

        /// <summary>
        /// Тэги.
        /// </summary>
        IBoardPostTags Tags { get; }

        /// <summary>
        /// Лайки.
        /// </summary>
        IBoardPostLikes Likes { get; }
    }
}