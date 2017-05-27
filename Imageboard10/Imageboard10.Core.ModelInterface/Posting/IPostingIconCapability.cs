using System.Collections.Generic;

namespace Imageboard10.Core.ModelInterface.Posting
{
    /// <summary>
    /// Поле с иконкой.
    /// </summary>
    public interface IPostingIconCapability : IPostingCapability
    {
        /// <summary>
        /// Доступные для выбора иконки.
        /// </summary>
        IList<IPostingCapabilityIcon> Icons { get; }
    }
}