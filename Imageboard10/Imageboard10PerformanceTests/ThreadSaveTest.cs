using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Imageboard10.Core.Database;
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
using Newtonsoft.Json;

namespace Imageboard10PerformanceTests
{
    public class ThreadSaveTest
    {
        private ModuleCollection _collection;
        private IModuleProvider _provider;
        private IBoardPostStore _store;

        public async Task Initilize(int parallelTasks)
        {
            _collection = new ModuleCollection();

            ModelsRegistration.RegisterModules(_collection);
            PostModelsRegistration.RegisterModules(_collection);
            _collection.RegisterModule<EsentInstanceProvider, IEsentInstanceProvider>(new EsentInstanceProvider(true));
            _collection.RegisterModule<PostModelStore, IBoardPostStore>(new PostModelStore("makaba", parallelTasks));
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

        public async Task Cleanup()
        {
            await _collection.Dispose();
            _collection = null;
            _provider = null;
            _store = null;
        }

        /// <summary>
        /// Прочитать файл.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <returns>Результат.</returns>
        public static async Task<string> ReadTestTextFile(string fileName)
        {
            var uri = new Uri($"ms-appx:///Resources/{fileName}");
            StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var text = await FileIO.ReadTextAsync(f);
            return text;
        }

        private async Task<IBoardPostCollection> ReadThread(string resourceFile)
        {
            var jsonStr = await ReadTestTextFile(resourceFile);
            var dto = JsonConvert.DeserializeObject<BoardEntity2>(jsonStr);
            var parser = _provider.FindNetworkDtoParser<ThreadData, IBoardPostCollectionEtag>();
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

        public async Task SaveThreadToStoreBenhcmark(Action<string> logger, int iterations)
        {
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
            logger($"Время загрузки треда в базу: {st.Elapsed.TotalSeconds:F2} сек. всего, {st.Elapsed.TotalMilliseconds / iterations:F2} мс на итерацию, {collection.Posts.Count} постов, {st.Elapsed.TotalMilliseconds / iterations / collection.Posts.Count:F2} мс/пост");
        }
    }
}