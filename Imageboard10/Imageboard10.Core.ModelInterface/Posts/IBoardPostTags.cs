using System.Collections.Generic;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Тэги.
    /// </summary>
    public interface IBoardPostTags
    {
        /// <summary>
        /// Строка с тэгами.
        /// </summary>
        string TagStr { get; }

        /// <summary>
        /// Тэги.
        /// </summary>
        IList<string> Tags { get; }
    }
}