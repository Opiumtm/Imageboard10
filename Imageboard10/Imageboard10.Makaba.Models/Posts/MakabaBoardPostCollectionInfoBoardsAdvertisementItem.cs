using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Makaba.Models.Posts
{
    /// <summary>
    /// ������� �����.
    /// </summary>
    public class MakabaBoardPostCollectionInfoBoardsAdvertisementItem : IBoardPostCollectionInfoBoardsAdvertisementItem
    {
        /// <summary>
        /// ������ �� �����.
        /// </summary>
        public ILink BoardLink { get; set; }

        /// <summary>
        /// ���.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ����������.
        /// </summary>
        public string Info { get; set; }
    }
}