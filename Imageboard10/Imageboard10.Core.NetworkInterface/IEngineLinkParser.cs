using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Парсер ссылок.
    /// </summary>
    public interface IEngineLinkParser : INetworkEngineCapability
    {
        /// <summary>
        /// Попробовать распарсить строку.
        /// </summary>
        /// <param name="uri">URI.</param>
        /// <param name="parseRelative">Парсить также относительные ссылки.</param>
        /// <returns>Результат или null, если не определён.</returns>
        ILink TryParseLink(string uri, bool parseRelative);

        /// <summary>
        /// true, если ссылка подходит к данному движку.
        /// </summary>
        /// <param name="uri">URI.</param>
        /// <param name="parseRelative">Парсить также относительные ссылки.</param>
        /// <returns>Результат.</returns>
        bool IsLinkForEngine(string uri, bool parseRelative);
    }
}