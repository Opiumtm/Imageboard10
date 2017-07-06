using System;
using System.Collections.Generic;
using System.Linq;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Пост борды.
    /// </summary>
    public class BoardPost : IBoardPost, IBoardPostOnServerCounter
    {
        /// <summary>
        /// Тип сущности.
        /// </summary>
        public PostStoreEntityType EntityType => PostStoreEntityType.Post;

        /// <summary>
        /// Идентификатор в хранилище.
        /// </summary>
        public PostStoreEntityId? StoreId { get; set; }

        /// <summary>
        /// Родительская сущность.
        /// </summary>
        public PostStoreEntityId? StoreParentId { get; set; }

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

        /// <summary>
        /// Лайки.
        /// </summary>
        public IBoardPostLikes Likes { get; set; }

        /// <summary>
        /// Превью поста.
        /// </summary>
        public IPostMediaWithSize Thumbnail => MediaFiles?.OfType<IPostMediaWithThumbnail>()?.Select(m => m.Thumbnail)?.FirstOrDefault(m => m != null);

        /// <summary>
        /// Время загрузки.
        /// </summary>
        public DateTimeOffset LoadedTime { get; set; }

        /// <summary>
        /// Нумерация на сервере в треде.
        /// </summary>
        public int? OnServerCounter { get; set; }
    }
}