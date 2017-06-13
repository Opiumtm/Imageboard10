using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Network
{
    /// <summary>
    /// Парсер HTML.
    /// </summary>
    public interface IHtmlParser
    {
        /// <summary>
        /// Получить документ.
        /// </summary>
        /// <param name="html">HTML.</param>
        /// <returns>Документ.</returns>
        IPostDocument ParseHtml(string html);
    }
}