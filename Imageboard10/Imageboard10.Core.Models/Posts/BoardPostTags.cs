using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Тэги.
    /// </summary>
    public class BoardPostTags : IBoardPostTags
    {
        /// <summary>
        /// Строка с тэгами.
        /// </summary>
        public string TagStr { get; set; }

        /// <summary>
        /// Тэги.
        /// </summary>
        public IList<string> Tags { get; set; }
    }
}