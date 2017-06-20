namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Медиа с дополнительной информацией.
    /// </summary>
    public interface IPostMediaWithInfo : IPostMedia
    {
        /// <summary>
        /// Имя.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Отображаемое имя.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Полное имя.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Not safe for work.
        /// </summary>
        bool Nsfw { get; }

        /// <summary>
        /// Хэш.
        /// </summary>
        string Hash { get; }
    }
}