using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Лайки.
    /// </summary>
    public class BoardPostLikes : IBoardPostLikes
    {
        /// <summary>
        /// Лайки.
        /// </summary>
        public int Likes { get; set; }

        /// <summary>
        /// Дизлайки.
        /// </summary>
        public int Dislikes { get; set; }
    }
}