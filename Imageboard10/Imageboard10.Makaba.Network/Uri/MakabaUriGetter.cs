using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Imageboard10.Core;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Core.NetworkInterface;
using Imageboard10.Makaba.Network.Config;

namespace Imageboard10.Makaba.Network.Uri
{
    /// <summary>
    /// Средство получения URI.
    /// </summary>
    public sealed class MakabaUriGetter : MakabaEngineModuleBase<INetworkUriGetter>, INetworkUriGetter
    {
        private IMakabaNetworkConfig _networkConfig;

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            _networkConfig = await moduleProvider.QueryModuleAsync<IMakabaNetworkConfig>() ?? throw new ModuleNotFoundException("IMakabaNetworkConfig");
            return Nothing.Value;
        }

        /// <summary>
        /// Получить URI из ссылки. Контекст ссылки по умолчанию.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Uri или null, если ссылка не распознана.</returns>
        public System.Uri GetUri(ILink link)
        {
            return GetUri(link, UriGetterContext.HtmlLink);
        }

        private System.Uri BaseUri => _networkConfig.BaseUri ?? throw new InvalidOperationException("Не настроен базовый URI для движка makaba");

        /// <summary>
        /// Получить URI из ссылки.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <param name="context">Контекст получения ссылки.</param>
        /// <returns>Uri или null, если ссылка не распознана.</returns>
        public System.Uri GetUri(ILink link, Guid context)
        {
            if (link == null)
            {
                return null;
            }
            if (link is EngineLinkBase e)
            {
                if (!MakabaConstants.MakabaEngineId.Equals(e.Engine, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }
            switch (link)
            {
                case PostLink l:
                    if (context == UriGetterContext.HtmlLink)
                    {
                        return new System.Uri(BaseUri, $"{l.Board}/res/{l.OpPostNum}.html#{l.PostNum}");
                    }
                    break;
                case ThreadPartLink l:
                    if (context == UriGetterContext.HtmlLink)
                    {
                        return new System.Uri(BaseUri, $"{l.Board}/res/{l.OpPostNum}.html");
                    }
                    if (context == UriGetterContext.ApiGet)
                    {
                        return new System.Uri(BaseUri, $"makaba/mobile.fcgi?task=get_thread&board={l.Board}&thread={l.OpPostNum}&num={l.FromPost}");
                    }
                    break;
                case ThreadLink l:
                    if (context == UriGetterContext.HtmlLink)
                    {
                        return new System.Uri(BaseUri, $"{l.Board}/res/{l.OpPostNum}.html");
                    }
                    if (context == UriGetterContext.ApiGet)
                    {
                        return new System.Uri(BaseUri, $"{l.Board}/res/{l.OpPostNum}.json");
                    }
                    if (context == UriGetterContext.ApiThreadPostCount)
                    {
                        return new System.Uri(BaseUri, $"makaba/mobile.fcgi?task=get_thread_last_info&board={l.Board}&thread={l.OpPostNum}");
                    }
                    break;
                case BoardPageLink l:
                    if (context == UriGetterContext.HtmlLink)
                    {
                        return new System.Uri(BaseUri, l.Page == 0 ? $"{l.Board}" : $"{l.Board}/{l.Page}.html");
                    }
                    if (context == UriGetterContext.ApiGet)
                    {
                        return new System.Uri(BaseUri, l.Page == 0 ? $"{l.Board}/index.json" : $"{l.Board}/{l.Page}.json");
                    }
                    break;
                case CatalogLink l:
                    if (context == UriGetterContext.HtmlLink)
                    {
                        switch (l.SortMode)
                        {
                            case BoardCatalogSort.CreateDate:
                                return new System.Uri(BaseUri, $"{l.Board}/catalog_num.html");
                            default:
                                return new System.Uri(BaseUri, $"{l.Board}/catalog.html");
                        }
                    }
                    if (context == UriGetterContext.ApiGet)
                    {
                        switch (l.SortMode)
                        {
                            case BoardCatalogSort.CreateDate:
                                return new System.Uri(BaseUri, $"{l.Board}/catalog_num.json");
                            default:
                                return new System.Uri(BaseUri, $"{l.Board}/catalog.json");
                        }
                    }
                    break;
                //case ThreadTagLink l:
                // TODO: Поддержать этот тип ссылки. Пока не поддерживается.
                case BoardMediaLink l:
                    if (context == UriGetterContext.HtmlLink || context == UriGetterContext.ApiGet)
                    {
                        return new System.Uri(BaseUri, $"{l.Board}/{CleanRelative(l.Uri)}");
                    }
                    break;
                case BoardLink l:
                    if (context == UriGetterContext.HtmlLink)
                    {
                        return new System.Uri(BaseUri, $"{l.Board}");
                    }
                    if (context == UriGetterContext.ApiGet)
                    {
                        return new System.Uri(BaseUri, $"{l.Board}/index.json");
                    }
                    break;
                case EngineMediaLink l:
                    if (context == UriGetterContext.HtmlLink || context == UriGetterContext.ApiGet)
                    {
                        return new System.Uri(BaseUri, $"{l.Uri}");
                    }
                    break;
                case EngineUriLink l:
                    if (context == UriGetterContext.HtmlLink || context == UriGetterContext.ApiGet)
                    {
                        return new System.Uri(BaseUri, $"{l.Uri}");
                    }
                    break;
                case RootLink _:
                    if (context == UriGetterContext.HtmlLink)
                    {
                        return BaseUri;
                    }
                    if (context == UriGetterContext.ApiBoardsList)
                    {
                        return new System.Uri(BaseUri, "makaba/mobile.fcgi?task=get_boards");
                    }
                    break;
                case CaptchaLink l:
                    // ReSharper disable once InconsistentNaming
                    const string CaptchaUriV2 = "api/captcha/";
                    if (l.CaptchaType == MakabaConstants.CaptchaTypes.DvachCaptcha)
                    {
                        if (context == UriGetterContext.ThumbnailLink)
                        {
                            return new System.Uri(BaseUri, CaptchaUriV2 + "2chaptcha/image/" + l.CaptchaId);
                        }
                        if (context == UriGetterContext.ApiGet)
                        {
                            if (l.CaptchaContext == CaptchaLinkContext.Thread)
                            {
                                return new System.Uri(BaseUri, CaptchaUriV2 + $"2chaptcha/id/?board={l.Board}&thread={l.ThreadId}");
                            }
                            if (l.CaptchaContext == CaptchaLinkContext.NewThread)
                            {
                                return new System.Uri(BaseUri, CaptchaUriV2 + $"2chaptcha/id/?board={l.Board}");
                            }
                        }
                    }
                    if (l.CaptchaType == MakabaConstants.CaptchaTypes.NoCaptcha)
                    {
                        if (l.CaptchaContext == CaptchaLinkContext.NewThread)
                        {
                            return null;
                        }
                        if (context == UriGetterContext.ApiCheck)
                        {
                            return new System.Uri(BaseUri, CaptchaUriV2 + "app/check/" + l.CaptchaId);
                        }
                        if (context == UriGetterContext.ApiGet)
                        {
                            return new System.Uri(BaseUri, CaptchaUriV2 + "app/id/" + l.CaptchaId);
                        }
                    }
                    break;
            }
            return null;
        }

        private string CleanRelative(string uri)
        {
            if (uri?.StartsWith("/") ?? false)
            {
                return uri.Remove(0, 1);
            }
            return uri;
        }
    }
}