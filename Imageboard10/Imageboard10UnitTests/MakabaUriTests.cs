using System;
using System.Threading.Tasks;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Core.NetworkInterface;
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
    }
}