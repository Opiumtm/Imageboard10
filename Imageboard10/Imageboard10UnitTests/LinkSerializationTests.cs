using System;
using System.Threading.Tasks;
using Imageboard10.Core;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imageboard10UnitTests
{
    [TestClass]
    [TestCategory("Links")]
    public class LinkSerializationTests
    {
        private ModuleCollection _collection;
        private IModuleProvider _modules;


        [TestInitialize]
        public async Task Initialize()
        {
            _collection = new ModuleCollection();
            LinkModelsRegistration.RegisterModules(_collection);
            await _collection.Seal();
            _modules = _collection.GetModuleProvider();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await _collection.Dispose();
            _modules = null;
            _collection = null;
        }

        private void TestSerialize<T>(Action<T> fillValues)
            where T : BoardLinkBase, IDeepCloneable<BoardLinkBase>, new()
        {
            var link = new T();
            fillValues(link);
            var str = link.Serialize(_modules);
            Assert.IsNotNull(str, "Сериализованная строка = null");
            var link2 = _modules.DeserializeLink(str);
            Assert.IsInstanceOfType(link2, typeof(T), "Тип десериализованной ссылки не совпадает с типом исходной ссылки");
            var link2t = (T) link2;
            Assert.IsTrue(BoardLinkEqualityComparer.Instance.Equals(link, link2t), "Десериализованная ссылка не равна исходной по значениям");
            var link3 = link.DeepClone();
            Assert.IsNotNull(link3, "Полная копия = null");
            Assert.IsInstanceOfType(link3, typeof(T), "Тип полной копии не совпадает с типом исходной ссылки");
            var link3t = (T) link3;
            Assert.IsTrue(BoardLinkEqualityComparer.Instance.Equals(link, link3t), "Полная копия ссылки не равна исходной по значениям");
        }

        [TestMethod]
        public void SerializeBoardLink()
        {
            TestSerialize<BoardLink>(l =>
            {
                l.Engine = "makaba";
                l.Board = "b";
            });
        }

        [TestMethod]
        public void SerializeBoardMediaLink()
        {
            TestSerialize<BoardMediaLink>(l =>
            {
                l.Engine = "makaba";
                l.Board = "b";
                l.Uri = "***uri***";
            });
        }

        [TestMethod]
        public void SerializeBoardPageLink()
        {
            TestSerialize<BoardPageLink>(l =>
            {
                l.Engine = "makaba";
                l.Board = "b";
                l.Page = 5;
            });
        }

        [TestMethod]
        public void SerializeCatalogLink()
        {
            TestSerialize<CatalogLink>(l =>
            {
                l.Engine = "makaba";
                l.Board = "b";
                l.SortMode = BoardCatalogSort.Bump;
            });
            TestSerialize<CatalogLink>(l =>
            {
                l.Engine = "makaba";
                l.Board = "po";
                l.SortMode = BoardCatalogSort.CreateDate;
            });
        }

        [TestMethod]
        public void SerializeEngineMediaLink()
        {
            TestSerialize<EngineMediaLink>(l =>
            {
                l.Engine = "makaba";
                l.Uri = "***uri***";
            });
        }

        [TestMethod]
        public void SerializeEngineUriLink()
        {
            TestSerialize<EngineUriLink>(l =>
            {
                l.Engine = "makaba";
                l.Uri = "***uri***";
            });
        }

        [TestMethod]
        public void SerializeMediaLink()
        {
            TestSerialize<MediaLink>(l =>
            {
                l.Uri = "***uri***";
            });
        }

        [TestMethod]
        public void SerializePostLink()
        {
            TestSerialize<PostLink>(l =>
            {
                l.Engine = "makaba";
                l.Board = "b";
                l.OpPostNum = 1234;
                l.PostNum = 4321;
            });
        }

        [TestMethod]
        public void SerializeRootLink()
        {
            TestSerialize<RootLink>(l =>
            {
                l.Engine = "makaba";
            });
        }

        [TestMethod]
        public void SerializeThreadLink()
        {
            TestSerialize<ThreadLink>(l =>
            {
                l.Engine = "makaba";
                l.Board = "b";
                l.OpPostNum = 1234;
            });
        }

        [TestMethod]
        public void SerializeThreadPartLink()
        {
            TestSerialize<ThreadPartLink>(l =>
            {
                l.Engine = "makaba";
                l.Board = "b";
                l.OpPostNum = 1234;
                l.FromPost = 4321;
            });
        }

        [TestMethod]
        public void SerializeThreadTagLink()
        {
            TestSerialize<ThreadTagLink>(l =>
            {
                l.Engine = "makaba";
                l.Board = "b";
                l.Tag = "windows";
            });
        }

        [TestMethod]
        public void SerializeUriLink()
        {
            TestSerialize<UriLink>(l =>
            {
                l.Uri = "***uri***";
            });
        }

        [TestMethod]
        public void SerializeYoutubeLink()
        {
            TestSerialize<YoutubeLink>(l =>
            {
                l.YoutubeId = "***youtube***";
            });
        }
    }
}