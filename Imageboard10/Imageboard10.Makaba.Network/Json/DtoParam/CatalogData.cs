using System;
using Imageboard10.Core.Models.Links.LinkTypes;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Данные каталога.
    /// </summary>
    public struct CatalogData
    {
        /// <summary>
        /// Посты.
        /// </summary>
        public CatalogEntity Thread;

        /// <summary>
        /// Ссылка.
        /// </summary>
        public CatalogLink Link;

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