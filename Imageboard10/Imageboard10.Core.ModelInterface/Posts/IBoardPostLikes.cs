namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Лайки к посту.
    /// </summary>
    public interface IBoardPostLikes
    {
        /// <summary>
        /// Лайки.
        /// </summary>
        int Likes { get; }

        /// <summary>
        /// Дизлайки.
        /// </summary>
        int Dislikes { get; }
    }
}