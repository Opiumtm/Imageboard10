using System;
using Imageboard10.Core.Models.Links.LinkTypes;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Данные по треду.
    /// </summary>
    public struct ThreadData
    {
        /// <summary>
        /// Сущность Makaba.
        /// </summary>
        public BoardEntity2 Entity;

        /// <summary>
        /// Ссылка.
        /// </summary>
        public ThreadLink Link;

        /// <summary>
        /// Время загрузки.
        /// </summary>
        public DateTimeOffset LoadedTime;

        /// <summary>
        /// Etag.
        /// </summary>
        public string Etag;
    }
}