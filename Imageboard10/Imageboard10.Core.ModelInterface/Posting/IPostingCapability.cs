using System;

namespace Imageboard10.Core.ModelInterface.Posting
{
    /// <summary>
    /// Поле для постинга.
    /// </summary>
    public interface IPostingCapability
    {
        /// <summary>
        /// Роль поля.
        /// </summary>
        Guid Role { get; }
    }
}