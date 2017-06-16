using System;
using System.Threading.Tasks;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Core.NetworkInterface;
using Imageboard10.Makaba;
using Imageboard10.Makaba.Network.Config;
using Imageboard10.Makaba.Network.Uri;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imageboard10UnitTests
{
    [TestClass]
    [TestCategory("MakabaUri")]
    public class MakabaUriTests
    {
        private ModuleCollection _collection;
        private IModuleProvider _provider;

        [TestInitialize]
        public async Task Initialize()
        {
            _collection = new ModuleCollection();
            _collection.RegisterModule<MakabaNetworkConfig, IMakabaNetworkConfig>();
            _collection.RegisterModule<CommonUriGetter, INetworkUriGetter>();
            _collection.RegisterModule<MakabaUriGetter, INetworkUriGetter>();

            await _collection.Seal();
            _provider = _collection.GetModuleProvider();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            var config = _provider.QueryModule<IMakabaNetworkConfig>();
            config.BaseUri = null;
            await config.Save();
            await _collection.Dispose();
            _collection = null;
            _provider = null;
        }

        [TestMethod]
        public async Task MakabaNetworkConfig()
        {
            var config = _provider.QueryModule<IMakabaNetworkConfig>();
            Assert.AreEqual(new Uri("https://2ch.hk/"), config.BaseUri, "Значение по умолчанию не совпадает");
            config.BaseUri = new Uri("http://2ch.so/");
            await config.Save();
            await _collection.Dispose();

            _collection = new ModuleCollection();
            _collection.RegisterModule<MakabaNetworkConfig, IMakabaNetworkConfig>();
            _collection.RegisterModule<CommonUriGetter, INetworkUriGetter>();
            _collection.RegisterModule<MakabaUriGetter, INetworkUriGetter>();

            await _collection.Seal();
            _provider = _collection.GetModuleProvider();

            Assert.AreEqual(new Uri("http://2ch.so/"), config.BaseUri, "Значение не совпадает");
        }

        [TestMethod]
        public async Task MakabaNetworkConfigSaveEvent()
        {
            var config = _provider.QueryModule<IMakabaNetworkConfig>();
            bool isSaved = false;
            config.Saved.AddHandler(async (sender, e) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(0.5));
                isSaved = true;
            });
            config.BaseUri = new Uri("http://2ch.so/");
            await config.Save();
            Assert.IsTrue(isSaved, "Событие не было вызвано");
        }

        [TestMethod]
        public async Task NullLinkUri()
        {
            await AssertUri(null, UriGetterContext.HtmlLink, null);
        }

        [TestMethod]
        public async Task InvalidEngineLinkUri()
        {
            await AssertUri(new PostLink()
            {
                Engine = "unknown engine",
                Board = "b",
                OpPostNum = 1234,
                PostNum = 4321
            }, UriGetterContext.HtmlLink, null);
        }

        [TestMethod]
        public async Task MakabaPostLinkUri()
        {
            await AssertUri(new PostLink()
            {
                Engine = "makaba",
                Board = "b",
                OpPostNum = 1234,
                PostNum = 4321
            }, UriGetterContext.HtmlLink, "https://2ch.hk/b/res/1234.html#4321");
            await AssertUri(new PostLink()
            {
                Engine = "makaba",
                Board = "b",
                OpPostNum = 1234,
                PostNum = 4321
            }, UriGetterContext.HtmlLink, "http://2ch.so/b/res/1234.html#4321", "http://2ch.so/");
        }

        [TestMethod]
        public async Task MakabaThreadPartLinkUri()
        {
            await AssertUri(new ThreadPartLink()
            {
                Engine = "makaba",
                Board = "b",
                OpPostNum = 1234,
                FromPost = 4321
            }, UriGetterContext.HtmlLink, "https://2ch.hk/b/res/1234.html");
            await AssertUri(new ThreadPartLink()
            {
                Engine = "makaba",
                Board = "b",
                OpPostNum = 1234,
                FromPost = 4321
            }, UriGetterContext.ApiGet, "https://2ch.hk/makaba/mobile.fcgi?task=get_thread&board=b&thread=1234&num=4321");
        }

        [TestMethod]
        public async Task MakabaThreadLinkUri()
        {
            await AssertUri(new ThreadLink()
            {
                Engine = "makaba",
                Board = "b",
                OpPostNum = 1234,
            }, UriGetterContext.HtmlLink, "https://2ch.hk/b/res/1234.html");
            await AssertUri(new ThreadLink()
            {
                Engine = "makaba",
                Board = "b",
                OpPostNum = 1234,
            }, UriGetterContext.ApiGet, "https://2ch.hk/b/res/1234.json");
            await AssertUri(new ThreadLink()
            {
                Engine = "makaba",
                Board = "b",
                OpPostNum = 1234,
            }, UriGetterContext.ApiThreadPostCount, "https://2ch.hk/makaba/mobile.fcgi?task=get_thread_last_info&board=b&thread=1234");
        }

        [TestMethod]
        public async Task MakabaBoardPageLink()
        {
            await AssertUri(new BoardPageLink()
            {
                Engine = "makaba",
                Board = "b",
                Page = 0
            }, UriGetterContext.HtmlLink, "https://2ch.hk/b");
            await AssertUri(new BoardPageLink()
            {
                Engine = "makaba",
                Board = "b",
                Page = 0
            }, UriGetterContext.ApiGet, "https://2ch.hk/b/index.json");
            await AssertUri(new BoardPageLink()
            {
                Engine = "makaba",
                Board = "b",
                Page = 1
            }, UriGetterContext.HtmlLink, "https://2ch.hk/b/1.html");
            await AssertUri(new BoardPageLink()
            {
                Engine = "makaba",
                Board = "b",
                Page = 1
            }, UriGetterContext.ApiGet, "https://2ch.hk/b/1.json");
        }

        [TestMethod]
        public async Task MakabaBoardLink()
        {
            await AssertUri(new BoardLink()
            {
                Engine = "makaba",
                Board = "b",
            }, UriGetterContext.HtmlLink, "https://2ch.hk/b");
            await AssertUri(new BoardLink()
            {
                Engine = "makaba",
                Board = "b",
            }, UriGetterContext.ApiGet, "https://2ch.hk/b/index.json");
        }

        [TestMethod]
        public async Task BoardMediaLink()
        {
            await AssertUri(new BoardMediaLink()
            {
                Engine = "makaba",
                Board = "b",
                Uri = "/test.gif"
            }, UriGetterContext.HtmlLink, "https://2ch.hk/b/test.gif");
            await AssertUri(new BoardMediaLink()
            {
                Engine = "makaba",
                Board = "b",
                Uri = "test.gif"
            }, UriGetterContext.HtmlLink, "https://2ch.hk/b/test.gif");
        }

        [TestMethod]
        public async Task EngineMediaLink()
        {
            await AssertUri(new EngineMediaLink()
            {
                Engine = "makaba",
                Uri = "/test.gif"
            }, UriGetterContext.HtmlLink, "https://2ch.hk/test.gif");
            await AssertUri(new EngineMediaLink()
            {
                Engine = "makaba",
                Uri = "test.gif"
            }, UriGetterContext.HtmlLink, "https://2ch.hk/test.gif");
        }

        [TestMethod]
        public async Task EngineUriLink()
        {
            await AssertUri(new EngineUriLink()
            {
                Engine = "makaba",
                Uri = "/test.html"
            }, UriGetterContext.HtmlLink, "https://2ch.hk/test.html");
            await AssertUri(new EngineUriLink()
            {
                Engine = "makaba",
                Uri = "test.html"
            }, UriGetterContext.HtmlLink, "https://2ch.hk/test.html");
        }

        [TestMethod]
        public async Task MakabaCatalogLinkUri()
        {
            await AssertUri(new CatalogLink()
            {
                Engine = "makaba",
                Board = "b",
                SortMode = BoardCatalogSort.Bump
            }, UriGetterContext.HtmlLink, "https://2ch.hk/b/catalog.html");
            await AssertUri(new CatalogLink()
            {
                Engine = "makaba",
                Board = "b",
                SortMode = BoardCatalogSort.CreateDate
            }, UriGetterContext.HtmlLink, "https://2ch.hk/b/catalog_num.html");
            await AssertUri(new CatalogLink()
            {
                Engine = "makaba",
                Board = "b",
                SortMode = BoardCatalogSort.Bump
            }, UriGetterContext.ApiGet, "https://2ch.hk/b/catalog.json");
            await AssertUri(new CatalogLink()
            {
                Engine = "makaba",
                Board = "b",
                SortMode = BoardCatalogSort.CreateDate
            }, UriGetterContext.ApiGet, "https://2ch.hk/b/catalog_num.json");
        }

        [TestMethod]
        public async Task MakabaRootLinkUri()
        {
            await AssertUri(new RootLink()
            {
                Engine = "makaba",
            }, UriGetterContext.HtmlLink, "https://2ch.hk/");
            await AssertUri(new RootLink()
            {
                Engine = "makaba",
            }, UriGetterContext.ApiBoardsList, "https://2ch.hk/makaba/mobile.fcgi?task=get_boards");
        }

        [TestMethod]
        public async Task MakabaCaptchaLinkUri()
        {
            await AssertUri(new CaptchaLink()
            {
                Engine = "makaba",
                Board = "b",
                CaptchaType = MakabaConstants.CaptchaTypes.DvachCaptcha,
                CaptchaId = "xxx_id_xxx",
                ThreadId = 10,
                CaptchaContext = CaptchaLinkContext.Thread
            }, UriGetterContext.ThumbnailLink, "https://2ch.hk/api/captcha/2chaptcha/image/xxx_id_xxx");
            await AssertUri(new CaptchaLink()
            {
                Engine = "makaba",
                Board = "b",
                CaptchaType = MakabaConstants.CaptchaTypes.DvachCaptcha,
                CaptchaId = "xxx_id_xxx",
                ThreadId = 10,
                CaptchaContext = CaptchaLinkContext.NewThread
            }, UriGetterContext.ThumbnailLink, "https://2ch.hk/api/captcha/2chaptcha/image/xxx_id_xxx");
            await AssertUri(new CaptchaLink()
            {
                Engine = "makaba",
                Board = "b",
                CaptchaType = MakabaConstants.CaptchaTypes.DvachCaptcha,
                CaptchaId = "xxx_id_xxx",
                ThreadId = 10,
                CaptchaContext = CaptchaLinkContext.Thread
            }, UriGetterContext.ApiGet, "https://2ch.hk/api/captcha/2chaptcha/id/?board=b&thread=10");
            await AssertUri(new CaptchaLink()
            {
                Engine = "makaba",
                Board = "b",
                CaptchaType = MakabaConstants.CaptchaTypes.DvachCaptcha,
                CaptchaId = "xxx_id_xxx",
                ThreadId = 10,
                CaptchaContext = CaptchaLinkContext.NewThread
            }, UriGetterContext.ApiGet, "https://2ch.hk/api/captcha/2chaptcha/id/?board=b");
            await AssertUri(new CaptchaLink()
            {
                Engine = "makaba",
                Board = "b",
                CaptchaType = MakabaConstants.CaptchaTypes.NoCaptcha,
                CaptchaId = "xxx_id_xxx",
                ThreadId = 10,
                CaptchaContext = CaptchaLinkContext.Thread
            }, UriGetterContext.ApiCheck, "https://2ch.hk/api/captcha/app/check/xxx_id_xxx");
            await AssertUri(new CaptchaLink()
            {
                Engine = "makaba",
                Board = "b",
                CaptchaType = MakabaConstants.CaptchaTypes.NoCaptcha,
                CaptchaId = "xxx_id_xxx",
                ThreadId = 10,
                CaptchaContext = CaptchaLinkContext.Thread
            }, UriGetterContext.ApiGet, "https://2ch.hk/api/captcha/app/id/xxx_id_xxx");
            await AssertUri(new CaptchaLink()
            {
                Engine = "makaba",
                Board = "b",
                CaptchaType = MakabaConstants.CaptchaTypes.NoCaptcha,
                CaptchaId = "xxx_id_xxx",
                ThreadId = 10,
                CaptchaContext = CaptchaLinkContext.NewThread
            }, UriGetterContext.ApiCheck, null);
            await AssertUri(new CaptchaLink()
            {
                Engine = "makaba",
                Board = "b",
                CaptchaType = MakabaConstants.CaptchaTypes.NoCaptcha,
                CaptchaId = "xxx_id_xxx",
                ThreadId = 10,
                CaptchaContext = CaptchaLinkContext.NewThread
            }, UriGetterContext.ApiGet, null);
        }

        [TestMethod]
        public async Task YoutubeLinkUri()
        {
            await AssertUri(new YoutubeLink()
            {
                YoutubeId = "xxx_id_xxx"
            }, UriGetterContext.HtmlLink, "http://www.youtube.com/watch?v=xxx_id_xxx");
            await AssertUri(new YoutubeLink()
            {
                YoutubeId = "xxx_id_xxx"
            }, UriGetterContext.ThumbnailLink, "http://i.ytimg.com/vi/xxx_id_xxx/0.jpg");
            await AssertUri(new YoutubeLink()
            {
                YoutubeId = "xxx_id_xxx"
            }, UriGetterContext.AppLaunchLink, "vnd.youtube:xxx_id_xxx?vndapp=youtube_mobile&vndclient=mv-google&vndel=watch");
        }

        private async Task AssertUri(ILink link, Guid context, string expectedResult, string baseUri = null)
        {
            if (baseUri != null)
            {
                var config = _provider.QueryModule<IMakabaNetworkConfig>();
                config.BaseUri = new Uri(baseUri);
                await config.Save();
            }
            else
            {
                var config = _provider.QueryModule<IMakabaNetworkConfig>();
                config.BaseUri = null;
                await config.Save();
            }
            var http = link.GetUri(context, _provider);
            Assert.AreEqual(expectedResult != null ? new Uri(expectedResult) : null, http, "Получена неправильная ссылка");
        }
    }
}