using System;
using System.Linq;
using Imageboard10.Core.ModelInterface.Posting;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Boards
{
    /// <summary>
    /// Возможность постинга.
    /// </summary>
    public class PostingCapability : IPostingCapability
    {
        /// <summary>
        /// Роль поля.
        /// </summary>
        public Guid Role { get; set; }
    }
}