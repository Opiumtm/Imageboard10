using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Иконка поста.
    /// </summary>
    public interface IBoardPostIcon
    {
        /// <summary>
        /// Ссылка на иконку.
        /// </summary>
        ILink ImageLink { get; }

        /// <summary>
        /// Описание.
        /// </summary>
        string Description { get; }
    }
}