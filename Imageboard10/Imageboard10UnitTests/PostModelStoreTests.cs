using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Imageboard10.Core;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;
using Imageboard10.Core.Models;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.ModelStorage.Posts;
using Imageboard10.Core.ModelStorage.UnitTests;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Core.Network.Html;
using Imageboard10.Core.NetworkInterface;
using Imageboard10.Core.NetworkInterface.Html;
using Imageboard10.Makaba;
using Imageboard10.Makaba.Models;
using Imageboard10.Makaba.Network.Html;
using Imageboard10.Makaba.Network.Json;
using Imageboard10.Makaba.Network.JsonParsers;
using Imageboard10.Makaba.Network.Uri;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Newtonsoft.Json;

namespace Imageboard10UnitTests
{
    [TestClass]
    [TestCategory("ModelStore")]
    public class PostModelStoreTests
    {
        private ModuleCollection _collection;
        private IModuleProvider _provider;
        private IBoardPostStore _store;

        [TestInitialize]
        public async Task Initialize()
        {
            _collection = new ModuleCollection();

            ModelsRegistration.RegisterModules(_collection);
            PostModelsRegistration.RegisterModules(_collection);
            _collection.RegisterModule<EsentInstanceProvider, IEsentInstanceProvider>(new EsentInstanceProvider(true));
            _collection.RegisterModule<PostModelStore, IBoardPostStore>(new PostModelStore("makaba"));
            _collection.RegisterModule<MakabaBoardReferenceDtoParsers, INetworkDtoParsers>();
            _collection.RegisterModule<MakabaBoardReferenceDtoParsers, INetworkDtoParsers>();
            _collection.RegisterModule<YoutubeIdService, IYoutubeIdService>();
            _collection.RegisterModule<MakabaLinkParser, IEngineLinkParser>();
            _collection.RegisterModule<AgilityHtmlDocumentFactory, IHtmlDocumentFactory>();
            _collection.RegisterModule<MakabaHtmlParser, IHtmlParser>();
            _collection.RegisterModule<MakabaPostDtoParsers, INetworkDtoParsers>();
            MakabaModelsRegistration.RegisterModules(_collection);

            TableVersionStatusForTests.ClearInstance();
            await _collection.Seal();
            _provider = _collection.GetModuleProvider();
            var module = _provider.QueryModule(typeof(IBoardPostStore), "makaba") ?? throw new ModuleNotFoundException();
            _store = module.QueryView<IBoardPostStore>() ?? throw new ModuleNotFoundException();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await _collection.Dispose();
            _collection = null;
            _provider = null;
            _store = null;
        }

        private async Task<IBoardPostCollection> ReadThread(string resourceFile)
        {
            var jsonStr = await TestResources.ReadTestTextFile(resourceFile);
            var dto = JsonConvert.DeserializeObject<BoardEntity2>(jsonStr);
            Assert.IsNotNull(dto, "dto != null");
            var parser = _provider.FindNetworkDtoParser<ThreadData, IBoardPostCollectionEtag>();
            Assert.IsNotNull(parser, "parser != null");
            var param = new ThreadData()
            {
                Link = new ThreadLink() { Board = "mobi", Engine = MakabaConstants.MakabaEngineId, OpPostNum = 1153568 },
                Etag = "##etag##",
                LoadedTime = DateTimeOffset.Now,
                Entity = dto
            };

            var collection = parser.Parse(param);
            return collection;
        }

        [TestMethod]
        public async Task SaveThreadToStore()
        {
            var collection = await ReadThread("mobi_thread.json");

            var count = collection.Posts.Count;

            (var backgroundTask, var backgroundCallback) = CreateStoreBackgroundTask();

            var collectionId = await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null, backgroundCallback);
            await backgroundTask;

            var collectionSize = await _store.GetCollectionSize(collectionId);
            var postsSize = await _store.GetTotalSize(PostStoreEntityType.Post);
            var threadsSize = await _store.GetTotalSize(PostStoreEntityType.Thread);
            var totalSize = await _store.GetTotalSize(null);

            Assert.AreEqual(count, collectionSize, "Размер коллекции");
            Assert.AreEqual(count, postsSize, "Количество постов");
            Assert.AreEqual(1, threadsSize, "Количество тредов");
            Assert.AreEqual(count + 1, totalSize, "Общее количество сущностей");
        }

        [TestMethod]
        public async Task SaveThreadToStoreBenhcmark()
        {
            const int iterations = 10;
            var collection = await ReadThread("mobi_thread_2.json");
            (collection.Info.Items.FirstOrDefault(f => f.GetInfoInterfaceTypes().Any(i => i == typeof(IBoardPostCollectionInfoFlags))) as IBoardPostCollectionInfoFlags).Flags.Add(UnitTestStoreFlags.AlwaysInsert);
            foreach (var p in collection.Posts)
            {
                p.Flags.Add(UnitTestStoreFlags.AlwaysInsert);
            }
            var st = new Stopwatch();
            st.Start();            
            for (var i = 0; i < iterations; i++)
            {
                await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null);
            }
            st.Stop();
            var count = collection.Posts.Count;
            Logger.LogMessage("Время загрузки треда в базу: {0:F2} сек. всего, {1:F2} мс на итерацию, {2} постов, {3:F2} мс/пост", st.Elapsed.TotalSeconds, st.Elapsed.TotalMilliseconds / iterations, collection.Posts.Count, st.Elapsed.TotalMilliseconds / iterations / collection.Posts.Count);
            var postsSize = await _store.GetTotalSize(PostStoreEntityType.Post);
            var threadsSize = await _store.GetTotalSize(PostStoreEntityType.Thread);
            var totalSize = await _store.GetTotalSize(null);
            Assert.AreEqual(count*iterations, postsSize, "Количество постов");
            Assert.AreEqual(1*iterations, threadsSize, "Количество тредов");
            Assert.AreEqual((count + 1)*iterations, totalSize, "Общее количество сущностей");
        }

        [TestMethod]
        public async Task SaveThreadToStoreMergeBenhcmark()
        {
            const int iterations = 10;
            var collection = await ReadThread("mobi_thread_2.json");
            await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null);
            var st = new Stopwatch();
            st.Start();
            for (var i = 0; i < iterations; i++)
            {
                await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Merge, null);
            }
            st.Stop();
            var count = collection.Posts.Count;
            Logger.LogMessage("Время загрузки треда в базу: {0:F2} сек. всего, {1:F2} мс на итерацию, {2} постов, {3:F2} мс/пост", st.Elapsed.TotalSeconds, st.Elapsed.TotalMilliseconds / iterations, collection.Posts.Count, st.Elapsed.TotalMilliseconds / iterations / collection.Posts.Count);
            var postsSize = await _store.GetTotalSize(PostStoreEntityType.Post);
            var threadsSize = await _store.GetTotalSize(PostStoreEntityType.Thread);
            var totalSize = await _store.GetTotalSize(null);
            Assert.AreEqual(count, postsSize, "Количество постов");
            Assert.AreEqual(1, threadsSize, "Количество тредов");
            Assert.AreEqual(count + 1, totalSize, "Общее количество сущностей");
        }

        private (Task task, BoardPostStoreBackgroundFinishedCallback callback) CreateStoreBackgroundTask()
        {
            var tcs = new TaskCompletionSource<Nothing>();

            void Callback(Exception e)
            {
                if (e == null)
                {
                    tcs.TrySetResult(Nothing.Value);
                }
                else
                {
                    tcs.TrySetException(e);
                }
            }

            return (tcs.Task, Callback);
        }
    }
}