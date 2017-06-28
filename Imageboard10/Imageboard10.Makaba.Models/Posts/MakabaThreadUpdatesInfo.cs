using System;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.NetworkInterface;

namespace Imageboard10.Makaba.Models.Posts
{
    /// <summary>
    /// Информация об обновлениях треда Makaba.
    /// </summary>
    public sealed class MakabaThreadUpdatesInfo : IThreadUpdatesInfo
    {
        /// <summary>
        /// Ссылка на тред.
        /// </summary>
        public ILink Link { get; set; }

        /// <summary>
        /// Последний пост.
        /// </summary>
        public ILink LastPost { get; set; }

        /// <summary>
        /// Количество постов.
        /// </summary>
        public int NumberOfPosts { get; set; }

        /// <summary>
        /// Последний апдейт.
        /// </summary>
        public DateTimeOffset LastUpdate { get; set; }
    }
}