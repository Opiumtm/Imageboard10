using System;
using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Пост борды.
    /// </summary>
    public class BoardPost : IBoardPost
    {
        /// <summary>
        /// Ссылка на пост.
        /// </summary>
        public ILink Link { get; set; }

        /// <summary>
        /// Родительская ссылка.
        /// </summary>
        public ILink ParentLink { get; set; }

        /// <summary>
        /// Комментарий.
        /// </summary>
        public IPostDocument Comment { get; set; }

        /// <summary>
        /// Цитаты.
        /// </summary>
        public IList<ILink> Quotes { get; set; }

        /// <summary>
        /// Медиафайлы.
        /// </summary>
        public IList<IPostMedia> MediaFiles { get; set; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Дата, как она отображается на доске.
        /// </summary>
        public string BoardSpecificDate { get; set; }

        /// <summary>
        /// Дата поста.
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// Порядковый номер поста.
        /// </summary>
        public int Counter { get; set; }

        /// <summary>
        /// Флаги поста.
        /// </summary>
        public IList<Guid> Flags { get; set; }

        /// <summary>
        /// MD5-хэш, если есть.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Адрес почты, если есть.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Постер.
        /// </summary>
        public IPosterInfo Poster { get; set; }

        /// <summary>
        /// Иконка.
        /// </summary>
        public IBoardPostIcon Icon { get; set; }

        /// <summary>
        /// Страна.
        /// </summary>
        public IBoardPostCountryFlag Country { get; set; }

        /// <summary>
        /// Тэги.
        /// </summary>
        public IBoardPostTags Tags { get; set; }
    }
}