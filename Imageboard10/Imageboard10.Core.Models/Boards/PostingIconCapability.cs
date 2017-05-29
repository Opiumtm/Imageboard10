using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Posting;

namespace Imageboard10.Core.Models.Boards
{
    /// <summary>
    /// Постинг иконок.
    /// </summary>
    public class PostingIconCapability : PostingCapability, IPostingIconCapability
    {
        /// <summary>
        /// Доступные для выбора иконки.
        /// </summary>
        public IList<IPostingCapabilityIcon> Icons { get; set; }
    }
}