using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Атрибут ссылки.
    /// </summary>
    public interface IPostLinkAttribute : IPostAttribute
    {
        /// <summary>
        /// Ссылка.
        /// </summary>
        ILink Link { get; }
    }
}