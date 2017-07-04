using System;
using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Пост на доске.
    /// </summary>
    public interface IBoardPost : IBoardPostLight
    {
        /// <summary>
        /// Комментарий.
        /// </summary>
        IPostDocument Comment { get; }

        /// <summary>
        /// Цитаты.
        /// </summary>
        IList<ILink> Quotes { get; }

        /// <summary>
        /// Медиафайлы.
        /// </summary>
        IList<IPostMedia> MediaFiles { get; }

        /// <summary>
        /// MD5-хэш, если есть.
        /// </summary>
        string Hash { get; }

        /// <summary>
        /// Адрес почты, если есть.
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Постер.
        /// </summary>
        IPosterInfo Poster { get; }

        /// <summary>
        /// Время, когда был загружен пост.
        /// </summary>
        DateTimeOffset LoadedTime { get; }

        /// <summary>
        /// Иконка.
        /// </summary>
        IBoardPostIcon Icon { get; }

        /// <summary>
        /// Страна.
        /// </summary>
        IBoardPostCountryFlag Country { get; }

        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        string UniqueId { get; }
    }
}