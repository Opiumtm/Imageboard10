namespace Imageboard10.Core.ModelInterface.Posting
{
    /// <summary>
    /// Медиа-файл.
    /// </summary>
    public interface IPostingMediaFileCapability : IPostingCapability
    {
        /// <summary>
        /// Максимальное количество файлов.
        /// </summary>
        int? MaxFileCount { get; }
    }
}