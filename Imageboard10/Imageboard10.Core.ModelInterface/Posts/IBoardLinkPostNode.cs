using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Ссылка на объект доски.
    /// </summary>
    public interface IBoardLinkPostNode : IPostNode
    {
        /// <summary>
        /// Ссылка.
        /// </summary>
        ILink BoardLink { get; }
    }
}