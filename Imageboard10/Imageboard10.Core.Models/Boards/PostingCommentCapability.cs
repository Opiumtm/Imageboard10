using System;
using Imageboard10.Core.ModelInterface.Posting;

namespace Imageboard10.Core.Models.Boards
{
    /// <summary>
    /// Постинг комментариев.
    /// </summary>
    public class PostingCommentCapability : PostingCapability, IPostingCommentCapability
    {
        /// <summary>
        /// Тип разметки.
        /// </summary>
        public Guid MarkupType { get; set; }

        /// <summary>
        /// Максимальный размер.
        /// </summary>
        public int? MaxLength { get; set; }
    }
}