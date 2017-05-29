using Imageboard10.Core.ModelInterface.Posting;

namespace Imageboard10.Core.Models.Boards
{
    /// <summary>
    /// Иконка.
    /// </summary>
    public class PostingCapabilityIcon : IPostingCapabilityIcon
    {
        /// <summary>
        /// Идентификатор иконки.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Имя.
        /// </summary>
        public string Name { get; set; }
    }
}