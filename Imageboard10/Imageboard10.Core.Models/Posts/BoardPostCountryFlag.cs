using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Флаг страны.
    /// </summary>
    public class BoardPostCountryFlag : IBoardPostCountryFlag
    {
        /// <summary>
        /// Ссылка на изображение.
        /// </summary>
        public ILink ImageLink { get; set; }
    }
}