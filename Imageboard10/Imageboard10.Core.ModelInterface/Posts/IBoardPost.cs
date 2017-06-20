using System;
using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Пост на доске.
    /// </summary>
    public interface IBoardPost
    {
        /// <summary>
        /// Ссылка на пост.
        /// </summary>
        ILink Link { get; }

        /// <summary>
        /// Родительская ссылка.
        /// </summary>
        ILink ParentLink { get; }

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
        /// Заголовок.
        /// </summary>
        string Subject { get; }

        /// <summary>
        /// Дата, как она отображается на доске.
        /// </summary>
        string BoardSpecificDate { get; }

        /// <summary>
        /// Дата поста.
        /// </summary>
        DateTimeOffset Date { get; }

        /// <summary>
        /// Порядковый номер поста.
        /// </summary>
        int Counter { get; }

        /// <summary>
        /// Флаги поста.
        /// </summary>
        IList<Guid> Flags { get; }

        /// <summary>
        /// MD5-хэш, если есть.
        /// </summary>
        string Hash { get; }

        /// <summary>
        /// Адрес почты, если есть.
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        string UniqueId { get; }

        /// <summary>
        /// Постер.
        /// </summary>
        IPosterInfo Poster { get; }

        /// <summary>
        /// Иконка.
        /// </summary>
        IBoardPostIcon Icon { get; }

        /// <summary>
        /// Страна.
        /// </summary>
        IBoardPostCountryFlag Country { get; }

        /// <summary>
        /// Тэги.
        /// </summary>
        IBoardPostTags Tags { get; }

        /// <summary>
        /// Лайки.
        /// </summary>
        IBoardPostLikes Likes { get; }

        /// <summary>
        /// Время, когда был загружен пост.
        /// </summary>
        DateTimeOffset LoadedTime { get; }
    }
}