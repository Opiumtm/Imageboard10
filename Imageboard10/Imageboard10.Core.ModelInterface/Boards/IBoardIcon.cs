using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Boards
{
    /// <summary>
    /// Иконка.
    /// </summary>
    public interface IBoardIcon
    {
        /// <summary>
        /// Имя.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Ссылка на медиа иконки.
        /// </summary>
        ILink MediaLink { get; }
    }
}