using System;
using Imageboard10.Core.Models.Links.LinkTypes;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Данные частично полученного треда.
    /// </summary>
    public struct PartialThreadData
    {
        /// <summary>
        /// Посты.
        /// </summary>
        public BoardPost2[] Posts;

        /// <summary>
        /// Ссылка.
        /// </summary>
        public ThreadLink Link;

        /// <summary>
        /// ETag.
        /// </summary>
        public string Etag;

        /// <summary>
        /// Время загрузки.
        /// </summary>
        public DateTimeOffset LoadedTime;
    }
}