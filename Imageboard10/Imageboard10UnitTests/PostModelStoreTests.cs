using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Imageboard10.Core;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Links;
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

        private async Task<IBoardPostCollection> ReadThread(string resourceFile, ThreadLink link = null)
        {
            var jsonStr = await TestResources.ReadTestTextFile(resourceFile);
            var dto = JsonConvert.DeserializeObject<BoardEntity2>(jsonStr);
            Assert.IsNotNull(dto, "dto != null");
            var parser = _provider.FindNetworkDtoParser<ThreadData, IBoardPostCollectionEtag>();
            Assert.IsNotNull(parser, "parser != null");
            var param = new ThreadData()
            {
                Link = link ?? new ThreadLink() { Board = "mobi", Engine = MakabaConstants.MakabaEngineId, OpPostNum = 1153568 },
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
            var collection = await ReadThread("mobi_thread_2.json");

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

            var newFlag1 = new Guid("{434B121C-DA42-443E-BFFC-D0D066DA643E}");
            var newFlag2 = new Guid("{9B67D253-5E45-4797-9A96-5833080BE7F0}");

            srcFlagsInfo = collection.Posts[0].Flags?.Distinct()?.ToList() ?? throw new NullReferenceException();
            srcFlagsInfo.Add(newFlag1);
            srcFlagsInfo.Add(newFlag2);
            await _store.UpdateFlags(new List<FlagUpdateAction>()
            {
                new FlagUpdateAction()
                {
                    Id = firstPostId,
                    Action = FlagUpdateOperation.Add,
                    Flag = newFlag1
                },
                new FlagUpdateAction()
                {
                    Id = firstPostId,
                    Action = FlagUpdateOperation.Add,
                    Flag = newFlag2
                },
            });
            flagsInfo = (await _store.LoadFlags(firstPostId))?.Distinct()?.ToList();
            Assert.IsNotNull(flagsInfo, "Найдены флаги поста");
            CollectionAssert.AreEquivalent(srcFlagsInfo, flagsInfo, "Флаги поста не совпадают");
            await _store.UpdateFlags(new List<FlagUpdateAction>()
            {
                new FlagUpdateAction()
                {
                    Id = firstPostId,
                    Action = FlagUpdateOperation.Remove,
                    Flag = newFlag1
                },
            });
            srcFlagsInfo.Remove(newFlag1);
            flagsInfo = (await _store.LoadFlags(firstPostId))?.Distinct()?.ToList();
            Assert.IsNotNull(flagsInfo, "Найдены флаги поста");
            CollectionAssert.AreEquivalent(srcFlagsInfo, flagsInfo, "Флаги поста не совпадают");

            void AssertMedia(IPostMedia src, IPostMedia test)
            {
                Assert.IsNotNull(test, "Медиа не равно null");
                Assert.AreEqual(src.MediaLink.GetLinkHash(), test.MediaLink?.GetLinkHash(), "Ссылка на медиа совпадает");
            }

            Assert.AreEqual(1, await _store.GetMediaCount(firstPostId), "Совпадает количество медиа файлов поста");
            var firstPostMedia = await _store.GetPostMedia(firstPostId, 0, null);
            Assert.AreEqual(1, firstPostMedia.Count, "Совпадает количество медиа файлов поста (loaded)");
            AssertMedia(collection.Posts[0].MediaFiles[0], firstPostMedia[0]);
            Assert.AreEqual(0, (await _store.GetPostMedia(firstPostId, 1, null)).Count, "Должно быть 0 медиа при сдвиге");

            var origPost92 = collection.Posts.First(p => (p.Link as PostLink)?.PostNum == 1154272);
            var post92idn = await _store.FindEntity(PostStoreEntityType.Post, origPost92.Link);
            Assert.IsNotNull(post92idn, "Пост №92 найден");
            var post92id = post92idn.Value;

            Assert.AreEqual(2, await _store.GetMediaCount(post92id), "Совпадает количество медиа файлов поста");
            var pos92Media = await _store.GetPostMedia(post92id, 0, null);
            Assert.AreEqual(2, pos92Media.Count, "Совпадает количество медиа файлов поста (loaded)");
            AssertMedia(origPost92.MediaFiles[0], pos92Media[0]);
            AssertMedia(origPost92.MediaFiles[1], pos92Media[1]);
            Assert.AreEqual(1, (await _store.GetPostMedia(post92id, 1, null)).Count, "Должно быть 1 медиа при сдвиге");
            Assert.AreEqual(1, (await _store.GetPostMedia(post92id, 0, 1)).Count, "Должно быть 1 медиа при указании максимума");
            Assert.AreEqual(0, (await _store.GetPostMedia(post92id, 2, null)).Count, "Должно быть 0 медиа при сдвиге на 2");

            var totalMediaCount = collection.Posts.Sum(p => p.MediaFiles.Count);
            Assert.AreEqual(totalMediaCount, await _store.GetMediaCount(collectionId), "Количество медиа совпадает");

            var totalMedia = collection.Posts.SelectMany(p => p.MediaFiles).ToArray();

            var gotMedia = await _store.GetPostMedia(collectionId, 20, 5);
            Assert.AreEqual(5, gotMedia.Count, "Совпадает количество медиа");

            var origMedia = totalMedia.Skip(20).Take(5).ToArray();
            for (var i = 0; i < origMedia.Length; i++)
            {
                AssertMedia(origMedia[i], gotMedia[i]);
            }

            gotMedia = await _store.GetPostMedia(collectionId, 30, 10);
            Assert.AreEqual(10, gotMedia.Count, "Совпадает количество медиа");

            origMedia = totalMedia.Skip(30).Take(10).ToArray();
            for (var i = 0; i < origMedia.Length; i++)
            {
                AssertMedia(origMedia[i], gotMedia[i]);
            }

            var accessInfo = await _store.GetAccessInfo(collectionId);
            Assert.IsNotNull(accessInfo, "accessInfo != null");
            Assert.AreEqual("##etag##-2", accessInfo.Etag, "accessInfo.Etag");
            Assert.IsFalse(accessInfo.IsArchived, "accessInfo.IsArchived");
            Assert.IsFalse(accessInfo.IsFavorite, "accessInfo.IsFavorite");
            var lastPost = collection.Posts.OrderByDescending(p => p.Link, BoardLinkComparer.Instance).First();
            Assert.IsNotNull(accessInfo.LastDownload, "accessInfo.LastDownload");
            var loadedTimeDiff = lastPost.LoadedTime - accessInfo.LastDownload.Value;
            Assert.IsTrue(Math.Abs(loadedTimeDiff.TotalSeconds) < 1.5, "accessInfo.LastDownload");
            Assert.AreEqual(lastPost.Link.GetLinkHash(), accessInfo.LastLoadedPost?.GetLinkHash(), "accessInfo.LastLoadedPost");
            Assert.AreEqual(lastPost.Link.GetLinkHash(), accessInfo.LastPost?.GetLinkHash(), "accessInfo.LastLoadedPost");
            Assert.IsNotNull(accessInfo.LastUpdate, "accessInfo.LastUpdate");
            var updateTimeDiff = lastPost.LoadedTime - accessInfo.LastUpdate.Value;
            Assert.IsTrue(Math.Abs(updateTimeDiff.TotalSeconds) < 1.5, "accessInfo.LastUpdate");
            Assert.AreEqual(collection.Posts.Count, accessInfo.NumberOfLoadedPosts, "accessInfo.NumberOfLoadedPosts");

            Assert.AreEqual(1, await _store.GetPostCounterNumber(collection.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).First().Link, collectionId), "Post counter 1");
            Assert.AreEqual(10, await _store.GetPostCounterNumber(collection.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).Skip(9).First().Link, collectionId), "Post counter 10");
            Assert.AreEqual(20, await _store.GetPostCounterNumber(collection.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).Skip(19).First().Link, collectionId), "Post counter 20");
        }

        [TestMethod]
        public async Task CheckThreadAccessInfo()
        {
            async Task<(IBoardPostCollection collection, PostStoreEntityId collectionId)> LoadCollection(string fileName, ThreadLink link = null)
            {
                var collection = await ReadThread("mobi_thread_2.json", link);
                var collectionId = await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null, null);
                return (collection, collectionId);
            }

            var tasks = new[]
            {
                LoadCollection("mobi_thread_2.json"),
                LoadCollection("po_thread.json", new ThreadLink() { Engine = MakabaConstants.MakabaEngineId, Board = "po", OpPostNum = 23334842 })
            };

            var taskResults = await Task.WhenAll(tasks);

            var collection1 = taskResults[0].collection;
            var collectionId1 = taskResults[0].collectionId;
            var collection2 = taskResults[1].collection;
            var collectionId2 = taskResults[1].collectionId;

            var accessInfo1 = await _store.GetAccessInfo(collectionId1);
            Assert.IsNotNull(accessInfo1, "accessInfo1 != null");
            var accessInfo2 = await _store.GetAccessInfo(collectionId2);
            Assert.IsNotNull(accessInfo2, "accessInfo2 != null");

            var lastPost1 = collection1.Posts.OrderByDescending(p => p.Link, BoardLinkComparer.Instance).First();
            var lastPost2 = collection2.Posts.OrderByDescending(p => p.Link, BoardLinkComparer.Instance).First();
            
            Assert.AreEqual(lastPost1.Link.GetLinkHash(), accessInfo1.LastLoadedPost?.GetLinkHash(), "accessInfo1.LastLoadedPost");
            Assert.AreEqual(lastPost2.Link.GetLinkHash(), accessInfo2.LastLoadedPost?.GetLinkHash(), "accessInfo2.LastLoadedPost");
            Assert.AreEqual(collection1.Posts.Count, accessInfo1.NumberOfLoadedPosts, "accessInfo1.NumberOfLoadedPosts");
            Assert.AreEqual(collection2.Posts.Count, accessInfo2.NumberOfLoadedPosts, "accessInfo2.NumberOfLoadedPosts");
            Assert.AreEqual(collection1.Posts.Count, accessInfo1.NumberOfPosts, "accessInfo1.NumberOfPosts");
            Assert.AreEqual(collection2.Posts.Count, accessInfo2.NumberOfPosts, "accessInfo2.NumberOfPosts");
            Assert.IsNull(accessInfo1.NumberOfReadPosts, "accessInfo1.NumberOfReadPosts");
            Assert.IsNull(accessInfo2.NumberOfReadPosts, "accessInfo2.NumberOfReadPosts");

            Assert.IsNotNull(accessInfo1.Entity, "accessInfo1.Entity != null");
            Assert.IsNotNull(accessInfo2.Entity, "accessInfo1.Entity != null");

            var thumbnail1 = collection1.Thumbnail;
            var thumbnail2 = collection2.Thumbnail;

            Assert.IsNotNull(accessInfo1.Entity.Thumbnail, "accessInfo1.Entity.Thumbnail != null");
            Assert.IsNotNull(accessInfo2.Entity.Thumbnail, "accessInfo2.Entity.Thumbnail != null");
            Assert.AreEqual(thumbnail1.Size, accessInfo1.Entity.Thumbnail.Size, "accessInfo1.Entity.Thumbnail.Size");
            Assert.AreEqual(thumbnail2.Size, accessInfo2.Entity.Thumbnail.Size, "accessInfo2.Entity.Thumbnail.Size");
            Assert.AreEqual(thumbnail1.FileSize, accessInfo1.Entity.Thumbnail.FileSize, "accessInfo1.Entity.Thumbnail.FileSize");
            Assert.AreEqual(thumbnail2.FileSize, accessInfo2.Entity.Thumbnail.FileSize, "accessInfo2.Entity.Thumbnail.FileSize");
            Assert.AreEqual(thumbnail1.MediaLink.GetLinkHash(), accessInfo1.Entity.Thumbnail.MediaLink?.GetLinkHash(), "accessInfo1.Entity.Thumbnail.MediaLink");
            Assert.AreEqual(thumbnail2.MediaLink.GetLinkHash(), accessInfo2.Entity.Thumbnail.MediaLink?.GetLinkHash(), "accessInfo2.Entity.Thumbnail.MediaLink");
            Assert.AreEqual(thumbnail1.MediaType, accessInfo1.Entity.Thumbnail.MediaType, "accessInfo1.Entity.Thumbnail.MediaType");
            Assert.AreEqual(thumbnail2.MediaType, accessInfo2.Entity.Thumbnail.MediaType, "accessInfo2.Entity.Thumbnail.MediaType");
            Assert.AreEqual(collection1.Subject, accessInfo1.Entity.Subject, "accessInfo1.Entity.Subject");
            Assert.AreEqual(collection2.Subject, accessInfo2.Entity.Subject, "accessInfo2.Entity.Subject");
            Assert.AreEqual(collection1.Link.GetLinkHash(), accessInfo1.Entity.Link?.GetLinkHash(), "accessInfo1.Entity.Link");
            Assert.AreEqual(collection2.Link.GetLinkHash(), accessInfo2.Entity.Link?.GetLinkHash(), "accessInfo2.Entity.Link");
            Assert.AreEqual(collection1.ParentLink.GetLinkHash(), accessInfo1.Entity.ParentLink?.GetLinkHash(), "accessInfo1.Entity.ParentLink");
            Assert.AreEqual(collection2.ParentLink.GetLinkHash(), accessInfo2.Entity.ParentLink?.GetLinkHash(), "accessInfo1.Entity.ParentLink");
            Assert.AreEqual(collectionId1, accessInfo1.Entity.StoreId, "accessInfo1.Entity.StoreId");
            Assert.AreEqual(collectionId2, accessInfo2.Entity.StoreId, "accessInfo2.Entity.StoreId");
            Assert.IsNull(accessInfo1.Entity.StoreParentId, "accessInfo1.Entity.StoreParentId");
            Assert.IsNull(accessInfo2.Entity.StoreParentId, "accessInfo2.Entity.StoreParentId");
            Assert.IsNull(accessInfo1.LogEntryId, "accessInfo1.LogEntryId");
            Assert.IsNull(accessInfo2.LogEntryId, "accessInfo2.LogEntryId");

            var etag1 = Guid.NewGuid().ToString();
            var etag2 = Guid.NewGuid().ToString();
            await _store.UpdateEtag(collectionId1, etag1);
            await _store.UpdateEtag(collectionId2, etag2);

            accessInfo1 = await _store.GetAccessInfo(collectionId1);
            accessInfo2 = await _store.GetAccessInfo(collectionId2);
            Assert.AreEqual(etag1, accessInfo1.Etag, "accessInfo1.Etag");
            Assert.AreEqual(etag2, accessInfo2.Etag, "accessInfo2.Etag");

            var dt1 = collection1.Posts[0].Date;
            var dt2 = collection2.Posts[0].Date;
            var logId1 = await _store.Touch(collectionId1, dt1);
            var logId2 = await _store.Touch(collectionId2, dt2);
            Assert.IsNotNull(logId1, "logId1 != null");
            Assert.IsNotNull(logId2, "logId2 != null");
            accessInfo1 = await _store.GetAccessInfo(collectionId1);
            accessInfo2 = await _store.GetAccessInfo(collectionId2);
            Assert.AreEqual(logId1.Value, accessInfo1.LogEntryId, "accessInfo1.LogEntryId");
            Assert.AreEqual(logId2.Value, accessInfo2.LogEntryId, "accessInfo2.LogEntryId");
            Assert.IsNotNull(accessInfo1.AccessTime, "accessInfo1.AccessTime != null");
            Assert.IsNotNull(accessInfo2.AccessTime, "accessInfo2.AccessTime != null");
            Assert.IsTrue(Math.Abs((dt1 - accessInfo1.AccessTime.Value).TotalSeconds) < 1.5, "accessInfo1.AccessTime");
            Assert.IsTrue(Math.Abs((dt2 - accessInfo2.AccessTime.Value).TotalSeconds) < 1.5, "accessInfo2.AccessTime");

            dt1 = dt1.AddDays(1);
            dt2 = dt2.AddDays(1);
            logId1 = await _store.Touch(collectionId1, dt1);
            logId2 = await _store.Touch(collectionId2, dt2);
            Assert.IsNotNull(logId1, "logId1 != null (second update)");
            Assert.IsNotNull(logId2, "logId2 != null (second update)");
            accessInfo1 = await _store.GetAccessInfo(collectionId1);
            accessInfo2 = await _store.GetAccessInfo(collectionId2);
            Assert.AreEqual(logId1.Value, accessInfo1.LogEntryId, "accessInfo1.LogEntryId (second update)");
            Assert.AreEqual(logId2.Value, accessInfo2.LogEntryId, "accessInfo2.LogEntryId (second update)");
            Assert.IsNotNull(accessInfo1.AccessTime, "accessInfo1.AccessTime != null (second update)");
            Assert.IsNotNull(accessInfo2.AccessTime, "accessInfo2.AccessTime != null (second update)");
            Assert.IsTrue(Math.Abs((dt1 - accessInfo1.AccessTime.Value).TotalSeconds) < 1.5, "accessInfo1.AccessTime (second update)");
            Assert.IsTrue(Math.Abs((dt2 - accessInfo2.AccessTime.Value).TotalSeconds) < 1.5, "accessInfo2.AccessTime (second update)");
        }

        [TestMethod]
        public async Task CheckThreadChildPositions()
        {
            async Task<(IBoardPostCollection collection, PostStoreEntityId collectionId)> LoadCollection(string fileName, ThreadLink link = null)
            {
                var collection = await ReadThread("mobi_thread_2.json", link);
                var collectionId = await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null, null);
                return (collection, collectionId);
            }

            var tasks = new[]
            {
                LoadCollection("mobi_thread_2.json"),
                LoadCollection("po_thread.json", new ThreadLink() { Engine = MakabaConstants.MakabaEngineId, Board = "po", OpPostNum = 23334842 })
            };

            var taskResults = await Task.WhenAll(tasks);

            var collection1 = taskResults[0].collection;
            var collectionId1 = taskResults[0].collectionId;
            var collection2 = taskResults[1].collection;
            var collectionId2 = taskResults[1].collectionId;

            Assert.AreEqual(1, await _store.GetPostCounterNumber(collection1.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).First().Link, collectionId1), "Post counter 1:1");
            Assert.AreEqual(10, await _store.GetPostCounterNumber(collection1.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).Skip(9).First().Link, collectionId1), "Post counter 1:10");
            Assert.AreEqual(20, await _store.GetPostCounterNumber(collection1.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).Skip(19).First().Link, collectionId1), "Post counter 1:20");

            Assert.AreEqual(1, await _store.GetPostCounterNumber(collection2.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).First().Link, collectionId2), "Post counter 2:1");
            Assert.AreEqual(10, await _store.GetPostCounterNumber(collection2.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).Skip(9).First().Link, collectionId2), "Post counter 2:10");
            Assert.AreEqual(20, await _store.GetPostCounterNumber(collection2.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).Skip(19).First().Link, collectionId2), "Post counter 2:20");

            await AssertChildrenPositions(collection1, collectionId1, 10, "collection1");
            await AssertChildrenPositions(collection2, collectionId2, 10, "collection2");
        }

        [TestMethod]
        public async Task CheckThreadMediaPositions()
        {
            async Task<(IBoardPostCollection collection, PostStoreEntityId collectionId)> LoadCollection(string fileName, ThreadLink link = null)
            {
                var collection = await ReadThread("mobi_thread_2.json", link);
                var collectionId = await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null, null);
                return (collection, collectionId);
            }

            var tasks = new[]
            {
                LoadCollection("mobi_thread_2.json"),
                LoadCollection("po_thread.json", new ThreadLink() { Engine = MakabaConstants.MakabaEngineId, Board = "po", OpPostNum = 23334842 })
            };

            var taskResults = await Task.WhenAll(tasks);

            var collection1 = taskResults[0].collection;
            var collectionId1 = taskResults[0].collectionId;
            var collection2 = taskResults[1].collection;
            var collectionId2 = taskResults[1].collectionId;

            await AssertMediaPositions(collection1, collectionId1, 12, "collection1");
            await AssertMediaPositions(collection2, collectionId2, 12, "collection2");
        }

        private async Task AssertChildrenPositions(IBoardPostCollection collection, PostStoreEntityId collectionId, int windowCount, string collectionName)
        {
            Assert.AreEqual(collection.Posts.Count, await _store.GetCollectionSize(collectionId), $"{collectionName}.Size");
            var originalLinks = new List<string>();
            var expectedLinks = new List<string>();
            foreach (var item in collection.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance))
            {
                originalLinks.Add(item.Link.GetLinkHash());
            }
            int maxIterations = 1000;
            do
            {
                var ids = await _store.GetChildren(collectionId, expectedLinks.Count, windowCount);
                var links = await _store.GetEntityLinks(ids.ToArray());
                foreach (var l in links)
                {
                    expectedLinks.Add(l.Link.GetLinkHash());
                }
                maxIterations--;
            } while (expectedLinks.Count < originalLinks.Count && maxIterations > 0);
            Assert.AreEqual(originalLinks.Count, expectedLinks.Count, $"{collectionName}->Количество полученных ссылок");
            for (var i = 0; i < originalLinks.Count; i++)
            {
                Assert.AreEqual(originalLinks[i], expectedLinks[i], $"{collectionName}:{i}->Ссылка");
            }
        }

        private async Task AssertMediaPositions(IBoardPostCollection collection, PostStoreEntityId collectionId, int windowCount, string collectionName)
        {
            var originalLinks = new List<string>();
            var expectedLinks = new List<string>();
            foreach (var item in collection.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance))
            {
                foreach (var m in item.MediaFiles)
                {
                    originalLinks.Add(m.MediaLink.GetLinkHash());
                }
            }
            Assert.AreEqual(originalLinks.Count, await _store.GetMediaCount(collectionId), $"{collectionName}.Size");
            int maxIterations = 5000;
            do
            {
                var media = await _store.GetPostMedia(collectionId, expectedLinks.Count, windowCount);
                foreach (var m in media)
                {
                    expectedLinks.Add(m.MediaLink.GetLinkHash());
                }
                maxIterations--;
            } while (expectedLinks.Count < originalLinks.Count && maxIterations > 0);
            Assert.AreEqual(originalLinks.Count, expectedLinks.Count, $"{collectionName}->Количество полученных медиа");
            for (var i = 0; i < originalLinks.Count; i++)
            {
                Assert.AreEqual(originalLinks[i], expectedLinks[i], $"{collectionName}:{i}->Медиа");
            }
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