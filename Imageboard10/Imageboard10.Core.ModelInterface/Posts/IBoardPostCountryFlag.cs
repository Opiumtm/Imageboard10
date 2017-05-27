using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Флаг страны.
    /// </summary>
    public interface IBoardPostCountryFlag
    {
        /// <summary>
        /// Ссылка на изображение.
        /// </summary>
        ILink ImageLink { get; }
    }
}