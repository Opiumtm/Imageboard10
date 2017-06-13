using System;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Средство получения URI.
    /// </summary>
    public interface INetworkUriGetter
    {
        /// <summary>
        /// Получить URI из ссылки.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Uri или null, если ссылка не распознана.</returns>
        Uri GetUri(ILink link);
    }
}