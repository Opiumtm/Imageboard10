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
    public class PostModelStoreTests : PostModelStoreTestsBase
    {
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
            var updateTimeDiff = lastPost.Date - accessInfo.LastUpdate.Value;
            Assert.IsTrue(Math.Abs(updateTimeDiff.TotalSeconds) < 1.5, "accessInfo.LastUpdate");
            Assert.AreEqual(collection.Posts.Count, accessInfo.NumberOfLoadedPosts, "accessInfo.NumberOfLoadedPosts");

            Assert.AreEqual(1, await _store.GetPostCounterNumber(collection.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).First().Link, collectionId), "Post counter 1");
            Assert.AreEqual(10, await _store.GetPostCounterNumber(collection.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).Skip(9).First().Link, collectionId), "Post counter 10");
            Assert.AreEqual(20, await _store.GetPostCounterNumber(collection.Posts.OrderBy(p => p.Link, BoardLinkComparer.Instance).Skip(19).First().Link, collectionId), "Post counter 20");

            var expectedQuotes = new ILink[]
            {
                new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1153591 },
                new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1153686 },
                new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1153955 },
                new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1156060 },
                new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1156467 },
                new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1156598 },
                new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1157715 },
                new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1162624 },
            };

            var opLink = new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1153568 };
            var opId = await _store.FindEntity(PostStoreEntityType.Post, opLink);
            Assert.IsNotNull(opId, "opId != null");
            Assert.AreEqual(PostStoreEntityType.Thread, await _store.GetCollectionType(collectionId), "collection type = Thread");
            await AssertQuotes(PostStoreEntityType.Post, opLink, expectedQuotes, "OP");

            var post2Link = new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1153574 };
            var expectedQuotes2 = new ILink[]
            {
                new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1153584 },
                new PostLink() { Engine = MakabaConstants.MakabaEngineId, Board = "mobi", OpPostNum = 1153568, PostNum = 1153999 },
            };

            await AssertQuotes(PostStoreEntityType.Post, post2Link, expectedQuotes2, "№2");

            var testCollectionLink = await _store.GetEntityLink(collectionId);
            Assert.IsNotNull(testCollectionLink, "testCollectionLink != null");
            Assert.AreEqual(collection.Link.GetLinkHash(), testCollectionLink.GetLinkHash(), "_store.GetEntityLink link");

            var postLinksToTest = new List<ILink>() {collection.Posts[0].Link, collection.Posts[1].Link};
            var testIds = await _store.FindEntities(collectionId, postLinksToTest);
            Assert.AreEqual(2, testIds.Count, "testIds.Count");

            var testPostLinks = await _store.GetEntityLinks(testIds.Select(i => i.Id).ToArray());
            Assert.IsNotNull(testPostLinks, "testPostLinks != null");
            Assert.AreEqual(2, testPostLinks.Count, "testPostLinks.Count");

            CollectionAssert.AreEquivalent(postLinksToTest.Select(ll => ll.GetLinkHash()).ToList(), testPostLinks.Select(ll => ll.Link.GetLinkHash()).ToList(), "_store.GetEntityLinks links");
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

        [TestMethod]
        public async Task LoadThreadPosts()
        {
            var collection = await ReadThread("mobi_thread_2.json");
            ((Imageboard10.Core.Models.Posts.BoardPost)collection.Posts[0]).Likes = new BoardPostLikes() { Likes = 2, Dislikes = 5 };
            ((Imageboard10.Core.Models.Posts.BoardPost)collection.Posts[0]).Tags = new BoardPostTags()
            {
                Tags = new List<string>() { "tag1", "tag2", "tag3" }
            };
            ((Imageboard10.Core.Models.Posts.BoardPost)collection.Posts[0]).Country = new BoardPostCountryFlag()
            {
                ImageLink = new BoardMediaLink() { Engine = "makaba", Board = "mobi", Uri = "### uri1 ###"}
            };
            ((Imageboard10.Core.Models.Posts.BoardPost)collection.Posts[0]).Icon = new BoardPostIcon()
            {
                ImageLink = new BoardMediaLink() { Engine = "makaba", Board = "mobi", Uri = "### uri2 ###" },
                Description = "### icon description ###"
            };

            var collectionId = await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null, null);
            var byId = collection.Posts.ToDictionary(p => p.Link, BoardLinkEqualityComparer.Instance);
            var quotes = collection.GetQuotesLookup();
            foreach (var q in quotes)
            {
                foreach (var l in q)
                {
                    if (byId.ContainsKey(q.Key))
                    {
                        byId[q.Key].Quotes.Add(l);
                    }
                }
            }

            var collectionId2 = await _store.FindEntity(PostStoreEntityType.Thread, collection.Link);
            Assert.IsNotNull(collectionId2, "collectionId2 != null");
            Assert.AreEqual(collectionId.Id, collectionId2.Value.Id, "collectionId2 = collectionId");
            Assert.AreEqual(PostStoreEntityType.Thread, await _store.GetCollectionType(collectionId), "collection type = Thread");
            await AssertLoadedPost(collectionId, PostStoreEntityType.Post, collection.Posts[0], 1, "OP");
            await AssertLoadedPost(collectionId, PostStoreEntityType.Post, collection.Posts[1], 2, "№2");
            await AssertLoadedPost(collectionId, PostStoreEntityType.Post, collection.Posts[2], 3, "№3");
            await AssertLoadedPost(collectionId, PostStoreEntityType.Post, collection.Posts[92], 93, "№93");

            var counters = new[]
            {
                1, 2, 3, 93
            };
            var testIds = (await _store.FindEntities(collectionId, new ILink[]
                {
                    collection.Posts[0].Link,
                    collection.Posts[1].Link,
                    collection.Posts[2].Link,
                    collection.Posts[92].Link
                }))
                .OrderBy(l => l.Link, BoardLinkComparer.Instance)
                .Select((l, idx) => new KeyValuePair<int, PostStoreEntityId>(counters[idx], l.Id))
                .ToArray();
            var ids = testIds.Select(i => i.Value).ToArray();
            Assert.AreEqual(4, testIds.Length, "testIds.Length");

            var loadedById = new LoadedPost[4];
            for (var i = 0; i < 4; i++)
            {
                loadedById[i].Id = testIds[i].Value;
                loadedById[i].Counter = testIds[i].Key;
                loadedById[i].EntityType = PostStoreEntityType.Post;
                loadedById[i].MessagePrefix = $"ByList,№{testIds[i].Key}";
                loadedById[i].Post = collection.Posts[testIds[i].Key - 1];
                loadedById[i].WithCounter = true;
            }
            foreach (var p in await _store.Load(ids, new PostStoreLoadMode() {EntityLoadMode = PostStoreEntityLoadMode.LinkOnly, RetrieveCounterNumber = false}))
            {
                Assert.IsNotNull(p.StoreId, "p.StoreId != null");
                var id = p.StoreId.Value;
                var idx = Array.FindIndex(loadedById, lp => lp.Id.Id == id.Id);
                Assert.IsTrue(idx >= 0, "idx >= 0");
                loadedById[idx].LinkOnly = p;
            }
            foreach (var p in await _store.Load(ids, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.EntityOnly, RetrieveCounterNumber = false }))
            {
                Assert.IsNotNull(p.StoreId, "p.StoreId != null");
                var id = p.StoreId.Value;
                var idx = Array.FindIndex(loadedById, lp => lp.Id.Id == id.Id);
                Assert.IsTrue(idx >= 0, "idx >= 0");
                loadedById[idx].BareEntity = p;
            }
            foreach (var p in await _store.Load(ids, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.Light, RetrieveCounterNumber = true }))
            {
                Assert.IsNotNull(p.StoreId, "p.StoreId != null");
                var id = p.StoreId.Value;
                var idx = Array.FindIndex(loadedById, lp => lp.Id.Id == id.Id);
                Assert.IsTrue(idx >= 0, "idx >= 0");
                loadedById[idx].Light = p;
            }
            foreach (var p in await _store.Load(ids, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.Full, RetrieveCounterNumber = true }))
            {
                Assert.IsNotNull(p.StoreId, "p.StoreId != null");
                var id = p.StoreId.Value;
                var idx = Array.FindIndex(loadedById, lp => lp.Id.Id == id.Id);
                Assert.IsTrue(idx >= 0, "idx >= 0");
                loadedById[idx].Full = p;
            }
            AssertLoadedPosts(collectionId, loadedById);

            var loadedByOffset = new LoadedPost[10];
            const int loadOffset = 10;
            var loadedByOffsetIds = await _store.GetChildren(collectionId, loadOffset, 10);
            Assert.AreEqual(10, loadedByOffsetIds.Count, "loadedByOffsetIds.Count");
            for (var i = 0; i < loadedByOffset.Length; i++)
            {
                loadedByOffset[i].Id = loadedByOffsetIds[i];
                loadedByOffset[i].Counter = loadOffset + i + 1;
                loadedByOffset[i].EntityType = PostStoreEntityType.Post;
                loadedByOffset[i].MessagePrefix = $"ByOffset,№{loadOffset + i + 1}";
                loadedByOffset[i].Post = collection.Posts[loadOffset + i];
                loadedByOffset[i].WithCounter = true;
            }
            foreach (var p in await _store.Load(collectionId, loadOffset, 10, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.LinkOnly, RetrieveCounterNumber = false }))
            {
                Assert.IsNotNull(p.StoreId, "p.StoreId != null");
                var id = p.StoreId.Value;
                var idx = Array.FindIndex(loadedByOffset, lp => lp.Id.Id == id.Id);
                Assert.IsTrue(idx >= 0, "idx >= 0");
                loadedByOffset[idx].LinkOnly = p;
            }
            foreach (var p in await _store.Load(collectionId, loadOffset, 10, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.EntityOnly, RetrieveCounterNumber = false }))
            {
                Assert.IsNotNull(p.StoreId, "p.StoreId != null");
                var id = p.StoreId.Value;
                var idx = Array.FindIndex(loadedByOffset, lp => lp.Id.Id == id.Id);
                Assert.IsTrue(idx >= 0, "idx >= 0");
                loadedByOffset[idx].BareEntity = p;
            }
            foreach (var p in await _store.Load(collectionId, loadOffset, 10, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.Light, RetrieveCounterNumber = true }))
            {
                Assert.IsNotNull(p.StoreId, "p.StoreId != null");
                var id = p.StoreId.Value;
                var idx = Array.FindIndex(loadedByOffset, lp => lp.Id.Id == id.Id);
                Assert.IsTrue(idx >= 0, "idx >= 0");
                loadedByOffset[idx].Light = p;
            }
            foreach (var p in await _store.Load(collectionId, loadOffset, 10, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.Full, RetrieveCounterNumber = true }))
            {
                Assert.IsNotNull(p.StoreId, "p.StoreId != null");
                var id = p.StoreId.Value;
                var idx = Array.FindIndex(loadedByOffset, lp => lp.Id.Id == id.Id);
                Assert.IsTrue(idx >= 0, "idx >= 0");
                loadedByOffset[idx].Full = p;
            }
            AssertLoadedPosts(collectionId, loadedByOffset);
        }

        [TestMethod]
        public async Task LoadThread()
        {
            var collection = await ReadThread("mobi_thread_2.json");
            var collectionId = await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null, null);
            var linkOnly = await _store.Load(collectionId, new PostStoreLoadMode() {EntityLoadMode = PostStoreEntityLoadMode.LinkOnly});
            Assert.IsNotNull(linkOnly, "linkOnly != null");

            Assert.AreEqual(collection.Link.GetLinkHash(), linkOnly.Link?.GetLinkHash(), "linkOnly.Link");

            var bareEntity = await _store.Load(collectionId, new PostStoreLoadMode() {EntityLoadMode = PostStoreEntityLoadMode.EntityOnly});

            Assert.IsNotNull(bareEntity, "bareEntity != null");
            Assert.AreEqual(collection.Link.GetLinkHash(), bareEntity.Link?.GetLinkHash(), "bareEntity.Link");
            Assert.AreEqual(collection.Subject, bareEntity.Subject, "bareEntity.Subject");
            Assert.IsNotNull(bareEntity.StoreId, "bareEntity.StoreId != null");
            Assert.AreEqual(collectionId.Id, bareEntity.StoreId.Value.Id, "bareEntity.StoreId");
            Assert.IsNull(bareEntity.StoreParentId, "bareEntity.StoreParentId == null");
            Assert.AreEqual(collection.ParentLink.GetLinkHash(), bareEntity.ParentLink?.GetLinkHash(), "bareEntity.ParentLink");
            Assert.IsNotNull(bareEntity.Thumbnail, "bareEntity.Thumbnail != null");
            Assert.AreEqual(collection.Thumbnail.Size, bareEntity.Thumbnail.Size, "bareEntity.Thumbnail.Size");
            Assert.AreEqual(collection.Thumbnail.MediaLink.GetLinkHash(), bareEntity.Thumbnail.MediaLink?.GetLinkHash(), "collection.Thumbnail.MediaLink");

            var light = await _store.Load(collectionId, new PostStoreLoadMode() {EntityLoadMode = PostStoreEntityLoadMode.Light});
            Assert.IsNotNull(light, "light != null");
            Assert.AreEqual(collection.Link.GetLinkHash(), light.Link?.GetLinkHash(), "light.Link");
            Assert.AreEqual(collection.Subject, light.Subject, "light.Subject");
            Assert.IsNotNull(light.StoreId, "light.StoreId != null");
            Assert.AreEqual(collectionId.Id, light.StoreId.Value.Id, "light.StoreId");
            Assert.IsNull(light.StoreParentId, "light.StoreParentId == null");
            Assert.AreEqual(collection.ParentLink.GetLinkHash(), light.ParentLink?.GetLinkHash(), "light.ParentLink");
            Assert.IsNotNull(light.Thumbnail, "light.Thumbnail != null");
            Assert.AreEqual(collection.Thumbnail.Size, light.Thumbnail.Size, "light.Thumbnail.Size");
            Assert.AreEqual(collection.Thumbnail.MediaLink.GetLinkHash(), light.Thumbnail.MediaLink?.GetLinkHash(), "light.Thumbnail.MediaLink");

            var lightCollection = light as IBoardPostCollection;
            Assert.IsNotNull(lightCollection, "light is IBoardPostCollection");
            Assert.AreEqual(0, lightCollection.Posts.Count, "Posts not loaded in light mode");
            Assert.IsNotNull(lightCollection.Info, "lightCollection.Info != null");            
            Assert.IsNotNull(lightCollection.Info.Items, "lightCollection.Info.Items != null");
            AssertCollectionInfo<IBoardPostCollectionInfoBoard>(lightCollection.Info, info =>
            {
                Assert.AreEqual("mobi", info.Board, "lightCollection.Info.Board = mobi");
            });
        }

        [TestMethod]
        public async Task FindPostsByFlags()
        {
            var collection = await ReadThread("mobi_thread_2.json");
            var collectionId = await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null, null);

            var testFlag1 = new Guid("{6BC3BB49-B97F-40B4-B42D-164C7A286DAD}");
            var testFlag2 = new Guid("{72F3536D-9C0D-47A0-8959-5108DBFE1172}");
            var testFlag3 = new Guid("{AE85ECD1-1DF1-416E-B75D-5FD7314DEC34}");

            async Task<PostStoreEntityId> GetPostId(ILink link)
            {
                Assert.IsNotNull(link, "link != null");
                var id = await _store.FindEntity(PostStoreEntityType.Post, link);
                Assert.IsNotNull(id, $"Поиск ID поста {link.GetLinkHash()}");
                return id.Value;
            }

            var test1FlagPosts = new PostStoreEntityId[]
            {
                await GetPostId(collection.Posts[0].Link),
                await GetPostId(collection.Posts[2].Link),
                await GetPostId(collection.Posts[10].Link),
                await GetPostId(collection.Posts[50].Link),
            };

            var test2FlagPosts = new PostStoreEntityId[]
            {
                await GetPostId(collection.Posts[1].Link),
                await GetPostId(collection.Posts[3].Link),
                await GetPostId(collection.Posts[15].Link),
                await GetPostId(collection.Posts[40].Link),
            };

            var test3FlagPosts = new PostStoreEntityId[]
            {
                await GetPostId(collection.Posts[0].Link),
                await GetPostId(collection.Posts[2].Link),
                await GetPostId(collection.Posts[15].Link),
                await GetPostId(collection.Posts[40].Link),
            };

            var fp = new List<FlagUpdateAction>();
            fp.AddRange(test1FlagPosts.Select(id => new FlagUpdateAction() { Flag = testFlag1, Action = FlagUpdateOperation.Add, Id = id }));
            fp.AddRange(test2FlagPosts.Select(id => new FlagUpdateAction() { Flag = testFlag2, Action = FlagUpdateOperation.Add, Id = id }));
            fp.AddRange(test3FlagPosts.Select(id => new FlagUpdateAction() { Flag = testFlag3, Action = FlagUpdateOperation.Add, Id = id }));

            await _store.UpdateFlags(fp);

            void AssertFound(IList<PostStoreEntityId> result, string msg, params PostStoreEntityId[] required)
            {
                CollectionAssert.AreEquivalent(required, result.ToArray(), msg);
            }

            var f1r = await _store.QueryByFlags(PostStoreEntityType.Post, collectionId, new List<Guid>() {testFlag1}, null);
            AssertFound(f1r, "Есть флаг 1", test1FlagPosts);

            var f2r = await _store.QueryByFlags(PostStoreEntityType.Post, collectionId, new List<Guid>() {testFlag2}, null);
            AssertFound(f2r, "Есть флаг 2", test2FlagPosts);

            var f3r = await _store.QueryByFlags(PostStoreEntityType.Post, collectionId, new List<Guid>() {testFlag3}, null);
            AssertFound(f3r, "Есть флаг 3", test3FlagPosts);

            var f12r = await _store.QueryByFlags(PostStoreEntityType.Post, collectionId, new List<Guid>() {testFlag1, testFlag2}, null);
            Assert.AreEqual(0, f12r.Count, "Флаги 1 и 2 не пересекаются");

            var f13r = await _store.QueryByFlags(PostStoreEntityType.Post, collectionId, new List<Guid>() { testFlag1, testFlag3 }, null);
            AssertFound(f13r, "Есть флаги 1 и 3", test3FlagPosts[0], test3FlagPosts[1]);

            var f23r = await _store.QueryByFlags(PostStoreEntityType.Post, collectionId, new List<Guid>() { testFlag2, testFlag3 }, null);
            AssertFound(f23r, "Есть флаги 2 и 3", test3FlagPosts[2], test3FlagPosts[3]);

            var f123r = await _store.QueryByFlags(PostStoreEntityType.Post, collectionId, new List<Guid>() { testFlag1, testFlag2, testFlag3 }, null);
            Assert.AreEqual(0, f123r.Count, "Флаги 1, 2, 3 не пересекаются");

            var f1m2r = await _store.QueryByFlags(PostStoreEntityType.Post, collectionId, new List<Guid>() { testFlag1 }, new List<Guid>() { testFlag2 });
            AssertFound(f1m2r, "Есть флаг 1, нет 2", test1FlagPosts);

            var f2m1r = await _store.QueryByFlags(PostStoreEntityType.Post, collectionId, new List<Guid>() { testFlag2 }, new List<Guid>() { testFlag1 });
            AssertFound(f2m1r, "Есть флаг 2, нет 1", test2FlagPosts);

            var f1m3r = await _store.QueryByFlags(PostStoreEntityType.Post, collectionId, new List<Guid>() { testFlag1 }, new List<Guid>() { testFlag3 });
            AssertFound(f1m3r, "Есть флаг 1, нет 3", test1FlagPosts[2], test1FlagPosts[3]);

            var f2m3r = await _store.QueryByFlags(PostStoreEntityType.Post, collectionId, new List<Guid>() { testFlag2 }, new List<Guid>() { testFlag3 });
            AssertFound(f2m3r, "Есть флаг 2, нет 3", test2FlagPosts[0], test2FlagPosts[1]);

            var f1rt = await _store.QueryByFlags(PostStoreEntityType.Post, null, new List<Guid>() { testFlag1 }, null);
            AssertFound(f1rt, "Есть флаг 1 (только по типу)", test1FlagPosts);
        }

        private class LikesInfo : IBoardPostLikesStoreInfo
        {
            public PostStoreEntityId Id { get; set; }

            public IBoardPostLikes Likes { get; set; }
        }

        [TestMethod]
        public async Task UpdatePostLikes()
        {
            var collection = await ReadThread("mobi_thread_2.json", null, 1);
            var collectionId = await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null, null);

            async Task<PostStoreEntityId> GetPostId(ILink link)
            {
                Assert.IsNotNull(link, "link != null");
                var id = await _store.FindEntity(PostStoreEntityType.Post, link);
                Assert.IsNotNull(id, $"Поиск ID поста {link.GetLinkHash()}");
                return id.Value;
            }

            var postId = await GetPostId(collection.Posts[0].Link);

            var likes = await _store.LoadLikes(new List<PostStoreEntityId>() { postId });
            Assert.AreEqual(0, likes.Count, "Лайков не найдено");
            var p = (await _store.Load(postId, new PostStoreLoadMode() {EntityLoadMode = PostStoreEntityLoadMode.Light})) as IBoardPostLight;
            Assert.IsNotNull(p, "p != null");
            Assert.IsNull(p.Likes, "p.Likes == null");

            await _store.UpdateLikes(new List<IBoardPostLikesStoreInfo>()
            {
                new LikesInfo() {Id = postId, Likes = new BoardPostLikes() {Likes = 3, Dislikes = 4}}
            });

            likes = await _store.LoadLikes(new List<PostStoreEntityId>() { postId });
            Assert.AreEqual(1, likes.Count, "Лайки найдены");

            Assert.IsNotNull(likes[0], "likes[0] != null");
            Assert.IsNotNull(likes[0].Likes, "likes[0].Likes != null");
            Assert.AreEqual(postId.Id, likes[0].StoreId.Id, "PostId соответствует");
            Assert.AreEqual(3, likes[0].Likes.Likes, "Likes == 3");
            Assert.AreEqual(4, likes[0].Likes.Dislikes, "Dislikes == 4");

            p = (await _store.Load(postId, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.Light })) as IBoardPostLight;

            Assert.IsNotNull(p, "p != null");
            Assert.IsNotNull(p.Likes, "p.Likes != null");
            Assert.AreEqual(3, p.Likes.Likes, "Likes == 3");
            Assert.AreEqual(4, p.Likes.Dislikes, "Dislikes == 4");
        }
    }
}