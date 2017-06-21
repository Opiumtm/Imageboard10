namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Превью постов.
    /// </summary>
    public interface IThreadPreviewPosts : IBoardPostCollection
    {
        /// <summary>
        /// Количество изображений.
        /// </summary>
        int? ImageCount { get; }

        /// <summary>
        /// Пропущено изображений.
        /// </summary>
        int? OmitImages { get; }

        /// <summary>
        /// Пропущено постов.
        /// </summary>
        int? Omit { get; }

        /// <summary>
        /// Количество ответов.
        /// </summary>
        int? ReplyCount { get; }
    }
}