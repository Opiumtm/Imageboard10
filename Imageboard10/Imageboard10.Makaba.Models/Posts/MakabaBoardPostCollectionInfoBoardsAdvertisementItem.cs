using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Makaba.Models.Posts
{
    /// <summary>
    /// Реклама доски.
    /// </summary>
    public class MakabaBoardPostCollectionInfoBoardsAdvertisementItem : IBoardPostCollectionInfoBoardsAdvertisementItem
    {
        /// <summary>
        /// Ссылка на доску.
        /// </summary>
        public ILink BoardLink { get; set; }

        /// <summary>
        /// Имя.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Информация.
        /// </summary>
        public string Info { get; set; }
    }
}