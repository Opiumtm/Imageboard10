using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Пост в каталоге.
    /// </summary>
    public class CatalogBoardPost : BoardPost, IBoardPostEntityWithSequence
    {
        /// <summary>
        /// Порядок на странице.
        /// </summary>
        public int OnPageSequence { get; set; }
    }
}