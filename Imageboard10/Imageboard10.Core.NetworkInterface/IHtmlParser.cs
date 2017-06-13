using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Парсер HTML.
    /// </summary>
    public interface IHtmlParser : INetworkEngineCapability
    {
        /// <summary>
        /// Получить документ.
        /// </summary>
        /// <param name="html">HTML.</param>
        /// <param name="baseLink">Базовая ссылка.</param>
        /// <returns>Документ.</returns>
        IPostDocument ParseHtml(string html, ILink baseLink);
    }
}