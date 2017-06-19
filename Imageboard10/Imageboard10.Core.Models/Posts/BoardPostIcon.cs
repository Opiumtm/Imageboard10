using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Иконка поста.
    /// </summary>
    public class BoardPostIcon : IBoardPostIcon
    {
        /// <summary>
        /// Ссылка на иконку.
        /// </summary>
        public ILink ImageLink { get; set; }

        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; set; }
    }
}