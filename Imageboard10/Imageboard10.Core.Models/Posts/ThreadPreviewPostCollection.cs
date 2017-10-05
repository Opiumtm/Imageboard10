using System;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Превью постов.
    /// </summary>
    public class ThreadPreviewPostCollection : BoardPostCollection, IThreadPreviewPostCollection
    {
        /// <summary>
        /// Количество изображений.
        /// </summary>
        public int? ImageCount { get; set; }

        /// <summary>
        /// Пропущено изображений.
        /// </summary>
        public int? OmitImages { get; set; }

        /// <summary>
        /// Пропущено постов.
        /// </summary>
        public int? Omit { get; set; }

        /// <summary>
        /// Количество ответов.
        /// </summary>
        public int? ReplyCount { get; set; }

        /// <summary>
        /// Порядок на странице.
        /// </summary>
        public int OnPageSequence { get; set; }
    }
}