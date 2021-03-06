using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Makaba.Models.Posts
{
    /// <summary>
    /// ������� ��������.
    /// </summary>
    public class MakabaBoardPostCollectionInfoNewsItem : IBoardPostCollectionInfoNewsItem
    {
        /// <summary>
        /// ����.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// ������ �� �������.
        /// </summary>
        public ILink NewsLink { get; set; }

        /// <summary>
        /// ���������.
        /// </summary>
        public string Title { get; set; }
    }
}