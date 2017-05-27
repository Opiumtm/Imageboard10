using Windows.Graphics;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Медиа с размером изображения.
    /// </summary>
    public interface IPostMediaWithSize : IPostMedia
    {
        /// <summary>
        /// Размер.
        /// </summary>
        SizeInt32 Size { get; }
    }
}