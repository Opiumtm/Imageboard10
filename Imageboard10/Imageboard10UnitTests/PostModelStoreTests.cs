﻿using System;
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
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.ModelStorage.Posts;
using Imageboard10.Core.ModelStorage.UnitTests;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Core.Network.Html;
using Imageboard10.Core.NetworkInterface;
using Imageboard10.Core.NetworkInterface.Html;
using Imageboard10.Core.Utility;
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
            var isChildrenLoaded = await _store.IsChildrenLoaded(collectionId);

            Assert.AreEqual(count, collectionSize, "Размер коллекции");
            Assert.AreEqual(count, postsSize, "Количество постов");
            Assert.AreEqual(1, threadsSize, "Количество тредов");
            Assert.AreEqual(count + 1, totalSize, "Общее количество сущностей");
            Assert.IsTrue(isChildrenLoaded, "Флаг успешной загрузки постов");

            var allLinks = collection.Posts.Select(p => p.Link).Distinct(BoardLinkEqualityComparer.Instance).ToArray();

            var childrenIds = (await _store.FindEntities(collectionId, allLinks)).ToDictionary(ll => ll.Link, BoardLinkEqualityComparer.Instance);
            Assert.AreEqual(allLinks.Length, childrenIds.Count, "Количество загруженных постов");

            foreach (var link in allLinks)
            {
                Assert.IsTrue(childrenIds.ContainsKey(link), $"Найдена ссыылка {link.GetLinkHash()}");
                Assert.AreEqual(PostStoreEntityType.Post, childrenIds[link].EntityType, $"Тип сущности для ссылки {link.GetLinkHash()}");
            }

            var etag = await _store.GetEtag(collectionId);
            Assert.AreEqual("##etag##", etag, "ETAG соответствует");
            await _store.UpdateEtag(collectionId, "##etag##-2");
            etag = await _store.GetEtag(collectionId);
            Assert.AreEqual("##etag##-2", etag, "Новый ETAG установлен");

            var l = collection.Posts[0].Link;
            var firstPostIdn = await _store.FindEntity(PostStoreEntityType.Post, l);
            Assert.IsNotNull(firstPostIdn, "Найден первый пост");

            var firstPostId = firstPostIdn.Value;
            var firstPostDoc = await _store.GetDocument(firstPostId);

            var infoSet = await _store.LoadCollectionInfoSet(collectionId);
            Assert.IsNotNull(infoSet, "Загружена информация о треде");

            AssertCollectionInfo<IBoardPostCollectionInfoBoard>(infoSet, info =>
            {
                Assert.AreEqual("mobi", info.Board, "info,Board->Board");
                Assert.AreEqual("Мобильные устройства и приложения", info.BoardName, "info, Board->BoardName");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoBoardLimits>(infoSet, info =>
            {
                Assert.AreEqual("Аноним", info.DefaultName, "info,Limits->DefaultName");
                Assert.AreEqual((ulong)(40960 * 1024), info.MaxFilesSize, "info,Limits->MaxFilesSize");
                Assert.AreEqual(15000, info.MaxComment, "info,Limits->MaxComment");
                Assert.IsNull(info.Pages, "info,Limits->Pages != null");
            });

            PostModelsTests.AssertDocuments(_provider, collection.Posts[0].Comment, firstPostDoc);

            var srcFlagsInfo = collection.Info.GetCollectionInfo<IBoardPostCollectionInfoFlags>().Flags?.Distinct()?.ToList() ?? throw new NullReferenceException();
            var flagsInfo = (await _store.LoadFlags(collectionId))?.Distinct()?.ToList();
            Assert.IsNotNull(flagsInfo, "Найдены флаги коллекции");
            CollectionAssert.AreEquivalent(srcFlagsInfo, flagsInfo, "Флаги коллекции совпадают");

            srcFlagsInfo = collection.Posts[0].Flags?.Distinct()?.ToList() ?? throw new NullReferenceException();
            flagsInfo = (await _store.LoadFlags(firstPostId))?.Distinct()?.ToList();
            Assert.IsNotNull(flagsInfo, "Найдены флаги поста");
            CollectionAssert.AreEquivalent(srcFlagsInfo, flagsInfo, "Флаги поста не совпадают");
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

        private void AssertCollectionInfo<T>(IBoardPostCollectionInfoSet infoSet, Action<T> asserts)
            where T : class, IBoardPostCollectionInfo
        {
            Assert.IsNotNull(infoSet, $"{typeof(T).Name}: infoSet != null");
            Assert.IsNotNull(infoSet.Items, $"{typeof(T).Name}: infoSet.Items != null");
            var info = infoSet.Items.FirstOrDefault(i => i.GetInfoInterfaceTypes().Any(it => it == typeof(T))) as T;
            Assert.IsNotNull(info, $"info is {typeof(T).Name}");
            asserts?.Invoke(info);
        }
    }
}