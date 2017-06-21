using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Makaba.Models.Posts
{
    /// <summary>
    /// Элемент новостей.
    /// </summary>
    public class MakabaBoardPostCollectionInfoNewsItem : IBoardPostCollectionInfoNewsItem
    {
        /// <summary>
        /// Дата.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Ссылка на новость.
        /// </summary>
        public ILink NewsLink { get; set; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        public string Title { get; set; }
    }
}