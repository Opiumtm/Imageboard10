using Imageboard10.Core.ModelInterface.Posting;

namespace Imageboard10.Core.Models.Boards
{
    /// <summary>
    /// Постинг медиа-файлов.
    /// </summary>
    public class PostingMediaFileCapability : PostingCapability, IPostingMediaFileCapability
    {
        /// <summary>
        /// Максимальное количество файлов.
        /// </summary>
        public int? MaxFileCount { get; set; }
    }
}