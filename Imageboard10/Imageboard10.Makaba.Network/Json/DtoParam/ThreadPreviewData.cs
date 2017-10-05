using System;
using Imageboard10.Core.Models.Links.LinkTypes;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Данные частично полученного треда с борды.
    /// </summary>
    public struct ThreadPreviewData
    {
        /// <summary>
        /// Посты.
        /// </summary>
        public BoardThread2 Thread;

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

        /// <summary>
        /// Порядок на странице.
        /// </summary>
        public int OnPageSequence;
    }
}