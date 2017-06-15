using System;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Modules;
using Imageboard10.Core.NetworkInterface;

namespace Imageboard10.Core.Network
{
    /// <summary>
    /// Получение общих ссылок.
    /// </summary>
    public sealed class CommonUriGetter : ModuleBase<INetworkUriGetter>, INetworkUriGetter, IStaticModuleQueryFilter
    {
        /// <summary>
        /// Идентификатор движка.
        /// </summary>
        public string EngineId => "";

        /// <summary>
        /// Получить URI из ссылки. Контекст ссылки по умолчанию.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Uri или null, если ссылка не распознана.</returns>
        public Uri GetUri(ILink link)
        {
            return GetUri(link, UriGetterContext.HtmlLink);
        }

        /// <summary>
        /// Получить URI из ссылки.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <param name="context">Контекст получения ссылки.</param>
        /// <returns>Uri или null, если ссылка не распознана.</returns>
        public Uri GetUri(ILink link, Guid context)
        {
            if (link == null)
            {
                return null;
            }
            switch (link)
            {
                case MediaLink l:
                    if (context == UriGetterContext.HtmlLink || context == UriGetterContext.ApiGet)
                    {
                        return new Uri(l.Uri);
                    }
                    break;
                case UriLink l:
                    if (context == UriGetterContext.HtmlLink || context == UriGetterContext.ApiGet)
                    {
                        return new Uri(l.Uri);
                    }
                    break;
                case YoutubeLink l:
                    if (context == UriGetterContext.HtmlLink)
                    {
                        return new Uri(l.GetAbsoluteUrl());
                    }
                    if (context == UriGetterContext.ThumbnailLink)
                    {
                        return new Uri(l.GetThumbnailUri());
                    }
                    if (context == UriGetterContext.YoutubeAppLink)
                    {
                        return new Uri(l.GetAppLaunchUrl());
                    }
                    break;
                case IUriLink l:
                    if (l.IsAbsolute)
                    {
                        return new Uri(l.GetAbsoluteUrl() ?? throw new InvalidOperationException("URI = null"));
                    }
                    break;
            }
            return null;
        }

        /// <summary>
        /// Проверить запрос.
        /// </summary>
        /// <typeparam name="T">Тип запроса.</typeparam>
        /// <param name="query">Запрос.</param>
        /// <returns>Результат.</returns>
        bool IStaticModuleQueryFilter.CheckQuery<T>(T query)
        {
            if (typeof(T) == typeof(EngineCapabilityQuery))
            {
                var c = (EngineCapabilityQuery)(object)query;
                if ("".Equals(c.EngineId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

    }
}