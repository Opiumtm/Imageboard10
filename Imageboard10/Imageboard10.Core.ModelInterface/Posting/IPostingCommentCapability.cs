using System;

namespace Imageboard10.Core.ModelInterface.Posting
{
    /// <summary>
    /// Поле комментария.
    /// </summary>
    public interface IPostingCommentCapability : IPostingCapability
    {
        /// <summary>
        /// Тип разметки.
        /// </summary>
        Guid MarkupType { get; }

        /// <summary>
        /// Максимальный размер.
        /// </summary>
        int? MaxLength { get; }
    }
}