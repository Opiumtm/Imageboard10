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
        SizeOfInt32 Size { get; }
    }
}