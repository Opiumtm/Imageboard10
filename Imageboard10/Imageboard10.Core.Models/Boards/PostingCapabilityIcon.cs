using Imageboard10.Core.ModelInterface.Posting;

namespace Imageboard10.Core.Models.Boards
{
    /// <summary>
    /// ������.
    /// </summary>
    public class PostingCapabilityIcon : IPostingCapabilityIcon
    {
        /// <summary>
        /// ������������� ������.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ���.
        /// </summary>
        public string Name { get; set; }
    }
}