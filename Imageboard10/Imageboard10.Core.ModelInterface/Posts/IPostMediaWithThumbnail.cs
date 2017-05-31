namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Медиафайл с изображением предварительного просмотра.
    /// </summary>
    public interface IPostMediaWithThumbnail : IPostMedia
    {
        /// <summary>
        /// Изображение предварительного просмотра.
        /// </summary>
        IPostMediaWithSize Thumbnail { get; }
    }
}