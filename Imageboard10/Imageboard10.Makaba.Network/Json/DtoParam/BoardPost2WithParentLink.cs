using System;
using Imageboard10.Core.ModelInterface.Posts.Store;
using Imageboard10.Core.Models.Links.LinkTypes;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Пост со ссылкой на родительский тред.
    /// </summary>
    public struct BoardPost2WithParentLink
    {
        /// <summary>
        /// Пост.
        /// </summary>
        public BoardPost2 Post;

        /// <summary>
        /// Ссылка на тред.
        /// </summary>
        public ThreadLink ParentLink;
       
        /// <summary>
        /// Порядковый номер поста в треде.
        /// </summary>
        public int Counter;

        /// <summary>
        /// Время загрузки.
        /// </summary>
        public DateTimeOffset LoadedTime;

        /// <summary>
        /// Тип сущности.
        /// </summary>
        public PostStoreEntityType EntityType;
    }
}