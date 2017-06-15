using System;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Modules;
using Imageboard10.Core.NetworkInterface;

namespace Imageboard10.Core.Network
{
    /// <summary>
    /// Класс-помощник с URI.
    /// </summary>
    public static class UriHelper
    {
        /// <summary>
        /// Получить URI из ссылки.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <param name="modules">Модули.</param>
        /// <returns>Результат.</returns>
        public static Uri GetUri(this ILink link, IModuleProvider modules)
        {
            if (link == null || modules == null)
            {
                return null;
            }
            if (link is IEngineLink e)
            {
                var uriGetter = modules.QueryEngineCapability<INetworkUriGetter>(e.Engine);
                if (uriGetter != null)
                {
                    return uriGetter.GetUri(link);
                }
            }
            else
            {
                var uriGetter = modules.QueryEngineCapability<INetworkUriGetter>("");
                if (uriGetter != null)
                {
                    return uriGetter.GetUri(link);
                }
            }
            return null;
        }
    }
}