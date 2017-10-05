using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Imageboard10.Core;
using Imageboard10.Core.Database;
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
using Newtonsoft.Json;


namespace Imageboard10UnitTests
{
    public abstract class PostModelStoreTestsBase
    {
        // ReSharper disable InconsistentNaming
        protected ModuleCollection _collection;
        protected IModuleProvider _provider;
        protected IBoardPostStore _store;
        // ReSharper enable InconsistentNaming

        [TestInitialize]
        public virtual async Task Initialize()
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
        public virtual async Task Cleanup()
        {
            await _collection.Dispose();
            _collection = null;
            _provider = null;
            _store = null;
        }

        protected (Task task, BoardPostStoreBackgroundFinishedCallback callback) CreateStoreBackgroundTask()
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

        protected void AssertCollectionInfo<T>(IBoardPostCollectionInfoSet infoSet, Action<T> asserts)
            where T : class, IBoardPostCollectionInfo
        {
            Assert.IsNotNull(infoSet, $"{typeof(T).Name}: infoSet != null");
            Assert.IsNotNull(infoSet.Items, $"{typeof(T).Name}: infoSet.Items != null");
            var info = infoSet.Items.FirstOrDefault(i => i.GetInfoInterfaceTypes().Any(it => it == typeof(T))) as T;
            Assert.IsNotNull(info, $"info is {typeof(T).Name}");
            asserts?.Invoke(info);
        }

        protected async Task<IBoardPostCollection> ReadThread(string resourceFile, ThreadLink link = null, int? firstPosts = null)
        {
            var jsonStr = await TestResources.ReadTestTextFile(resourceFile);
            var dto = JsonConvert.DeserializeObject<BoardEntity2>(jsonStr);
            if (firstPosts != null)
            {
                dto.Threads[0].Posts = dto.Threads[0].Posts.Take(firstPosts.Value).ToArray();
            }
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

        protected async Task AssertQuotes(PostStoreEntityType entityType, ILink opLink, ILink[] expectedQuotes, string msgPrefix)
        {
            var opId = await _store.FindEntity(PostStoreEntityType.Post, opLink);
            Assert.IsNotNull(opId, $"{msgPrefix}: opId != null");
            Assert.AreEqual(PostStoreEntityType.Post, await _store.GetCollectionType(opId.Value), $"{msgPrefix}: post type = {entityType}");
            var quoteIds = await _store.GetPostQuotes(opId.Value);
            Assert.IsNotNull(quoteIds, $"{msgPrefix}: quoteIds != null");
            Assert.AreEqual(expectedQuotes.Length, quoteIds.Count, $"{msgPrefix}: quoteIds.Count");

            var quotes = await _store.GetEntityLinks(quoteIds.ToArray());
            Assert.IsNotNull(quotes, $"{msgPrefix}: quotes != null");
            Assert.AreEqual(expectedQuotes.Length, quotes.Count, $"{msgPrefix}: quotes.Count");
            var quotesToCheck = quotes.Select(q => q.Link).OrderBy(q => q, BoardLinkComparer.Instance).ToArray();
            for (var i = 0; i < expectedQuotes.Length; i++)
            {
                Assert.AreEqual(expectedQuotes[i].GetLinkHash(), quotesToCheck[i].GetLinkHash(), $"{msgPrefix}: quote {i}");
            }
        }

        protected struct LoadedPost
        {
            public bool WithCounter;
            public PostStoreEntityId Id;
            public string MessagePrefix;
            public int Counter;
            public IBoardPost Post;
            public PostStoreEntityType EntityType;
            public IBoardPostEntity LinkOnly;
            public IBoardPostEntity BareEntity;
            public IBoardPostEntity Light;
            public IBoardPostEntity Full;
        }

        protected async Task<LoadedPost> LoadPost(PostStoreEntityId opId)
        {
            return new LoadedPost()
            {
                WithCounter = true,
                LinkOnly = await _store.Load(opId, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.LinkOnly, RetrieveCounterNumber = false }),
                BareEntity = await _store.Load(opId, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.EntityOnly, RetrieveCounterNumber = false }),
                Light = await _store.Load(opId, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.Light, RetrieveCounterNumber = true }),
                Full = await _store.Load(opId, new PostStoreLoadMode() { EntityLoadMode = PostStoreEntityLoadMode.Full, RetrieveCounterNumber = true }),
            };
        }

        protected void AssertLoadedPosts(PostStoreEntityId collectionId, IEnumerable<LoadedPost> loadedPosts)
        {
            int counter = 0;
            foreach (var p in loadedPosts)
            {
                counter++;
                AssertLinkOnlyLoadedPost(collectionId, p.EntityType, p.Post, p.MessagePrefix + $"({counter},LinkOnly)", p.LinkOnly, p.Id);
                AssertBareEntityLoadedPost(collectionId, p.EntityType, p.Post, p.MessagePrefix + $"({counter},BareEntity)", p.BareEntity, p.Id);
                AssertLightLoadedPost(collectionId, p.EntityType, p.Post, p.MessagePrefix + $"({counter},Light)", p.Light, p.Id, p.WithCounter, p.Counter);
                AssertFullLoadedPost(collectionId, p.EntityType, p.Post, p.MessagePrefix + $"({counter},Full)", p.Full, p.Id, p.WithCounter, p.Counter);
            }
        }

        protected async Task AssertLoadedPost(PostStoreEntityId collectionId, PostStoreEntityType entityType, IBoardPost post, int postCounterNum, string messagePrefix)
        {
            var opLink = post.Link;
            var opIdn = await _store.FindEntity(entityType, opLink);
            Assert.IsNotNull(opIdn, $"{messagePrefix}: opId != null");
            var opId = opIdn.Value;
            var loadedPost = await LoadPost(opId);
            loadedPost.Id = opId;
            loadedPost.Counter = postCounterNum;
            loadedPost.MessagePrefix = messagePrefix;
            loadedPost.EntityType = entityType;
            loadedPost.Post = post;
            var lpa = new LoadedPost[] { loadedPost };
            AssertLoadedPosts(collectionId, lpa);
        }

        protected void AssertLinkOnlyLoadedPost(PostStoreEntityId collectionId, PostStoreEntityType entityType,
            IBoardPost post, string messagePrefix, IBoardPostEntity opLinkOnly, PostStoreEntityId opId)
        {
            Assert.IsNotNull(opLinkOnly, $"{messagePrefix}: opLinkOnly != null");
            Assert.AreEqual(post.Link.GetLinkHash(), opLinkOnly.Link.GetLinkHash(), $"{messagePrefix}: opLinkOnly.Link");
            Assert.AreEqual(post.ParentLink.GetLinkHash(), opLinkOnly.ParentLink.GetLinkHash(),
                $"{messagePrefix}: opLinkOnly.ParentLink");
            Assert.AreEqual(entityType, opLinkOnly.EntityType, $"{messagePrefix}: opLinkOnly.EntityType");
            Assert.IsNotNull(opLinkOnly.StoreId, $"{messagePrefix}: opLinkOnly.StoreId");
            Assert.AreEqual(opId, opLinkOnly.StoreId.Value, $"{messagePrefix}: opLinkOnly.StoreId");
            Assert.IsNotNull(opLinkOnly.StoreParentId, $"{messagePrefix}: opLinkOnly.StoreParentId");
            Assert.AreEqual(collectionId, opLinkOnly.StoreParentId.Value, $"{messagePrefix}: opLinkOnly.StoreParentId");
        }

        protected void AssertBareEntityLoadedPost(PostStoreEntityId collectionId, PostStoreEntityType entityType,
            IBoardPost post, string messagePrefix, IBoardPostEntity opEntity, PostStoreEntityId opId)
        {
            AssertLinkOnlyLoadedPost(collectionId, entityType, post, messagePrefix, opEntity, opId);
            Assert.AreEqual(post.Subject, opEntity.Subject, $"{messagePrefix}: opEntity.Subject");
            if (post.Thumbnail != null)
            {
                Assert.IsNotNull(opEntity.Thumbnail, $"{messagePrefix}: opEntity.Thumbnail != null");
                Assert.AreEqual(post.Thumbnail.MediaLink?.GetLinkHash(), opEntity.Thumbnail.MediaLink?.GetLinkHash(), $"{messagePrefix}: opEntity.Thumbnail.MediaLink");
                Assert.AreEqual(post.Thumbnail.Size, opEntity.Thumbnail.Size, $"{messagePrefix}: opEntity.Thumbnail.Size");
                Assert.AreEqual(post.Thumbnail.FileSize, opEntity.Thumbnail.FileSize, $"{messagePrefix}: opEntity.Thumbnail.FileSize");
                Assert.AreEqual(post.Thumbnail.MediaType, opEntity.Thumbnail.MediaType, $"{messagePrefix}: opEntity.Thumbnail.MediaType");
            }
            else
            {
                Assert.IsNull(opEntity.Thumbnail, "opEntity.Thumbnail == null");
            }
        }

        protected void AssertLightLoadedPost(PostStoreEntityId collectionId, PostStoreEntityType entityType,
            IBoardPost post, string messagePrefix, IBoardPostEntity opEntity, PostStoreEntityId opId, bool counterRetrieved, int postCounterNum)
        {
            AssertBareEntityLoadedPost(collectionId, entityType, post, messagePrefix, opEntity, opId);
            if (opEntity is IBoardPostLight opLight)
            {
                Assert.AreEqual(post.BoardSpecificDate, opLight.BoardSpecificDate, $"{messagePrefix}: opLight.BoardSpecificDate");
                var dateDiff = Math.Abs((post.Date - opLight.Date).TotalSeconds);
                Assert.IsTrue(dateDiff < 1.5, $"{messagePrefix}: opLight.Date");
                if (post.Likes != null)
                {
                    Assert.IsNotNull(opLight.Likes, $"{messagePrefix}: opLight.Likes != null");
                    Assert.AreEqual(post.Likes.Likes, opLight.Likes.Likes, $"{messagePrefix}: opLight.Likes.Likes");
                    Assert.AreEqual(post.Likes.Dislikes, opLight.Likes.Dislikes, $"{messagePrefix}: opLight.Likes.Dislikes");
                }
                else
                {
                    Assert.IsNull(opLight.Likes, $"{messagePrefix}: opLight.Likes == null");
                }
                if (post.Tags != null)
                {
                    Assert.IsNotNull(opLight.Tags, $"{messagePrefix}: opLight.Tags != null");
                    CollectionAssert.AreEquivalent(post.Tags.Tags.Distinct().ToList(), opLight.Tags.Tags.Distinct().ToList(), $"{messagePrefix}: opLight.Tags");
                }
                else
                {
                    Assert.IsNull(opLight.Tags, $"{messagePrefix}: opLight.Tags == null");
                }
                Assert.IsNotNull(opLight.Flags, $"{messagePrefix}: opLight.Flags != null");
                CollectionAssert.AreEquivalent(post.Flags.Distinct().ToList(), opLight.Flags.Distinct().ToList(), $"{messagePrefix}: opLight.Flags");
                if (counterRetrieved)
                {
                    Assert.AreEqual(postCounterNum, opLight.Counter, $"{messagePrefix}: opLight.Counter");
                }
            }
            else
            {
                Assert.Fail($"{messagePrefix}: не типа IBoardPostLight");
            }
        }

        protected void AssertFullLoadedPost(PostStoreEntityId collectionId, PostStoreEntityType entityType,
            IBoardPost post, string messagePrefix, IBoardPostEntity opEntity, PostStoreEntityId opId,
            bool counterRetrieved, int postCounterNum)
        {
            AssertLightLoadedPost(collectionId, entityType, post, messagePrefix, opEntity, opId, counterRetrieved, postCounterNum);
            if (opEntity is IBoardPost opPost)
            {
                Assert.AreEqual(post.UniqueId, opPost.UniqueId, $"{messagePrefix}: opPost.UniqueId");
                PostModelsTests.AssertDocuments(_provider, post.Comment, opPost.Comment);
                if (post.Country != null)
                {
                    Assert.IsNotNull(opPost.Country, $"{messagePrefix}: opPost.Country != null");
                    Assert.AreEqual(post.Country.ImageLink?.GetLinkHash(), opPost.Country.ImageLink?.GetLinkHash(), $"{messagePrefix}: opPost.Country");
                }
                else
                {
                    Assert.IsNull(opPost.Country, $"{messagePrefix}: opPost.Country == null");
                }
                if (post.Icon != null)
                {
                    Assert.IsNotNull(opPost.Icon, $"{messagePrefix}: opPost.Icon != null");
                    Assert.AreEqual(post.Icon.ImageLink?.GetLinkHash(), opPost.Icon.ImageLink?.GetLinkHash(), $"{messagePrefix}: opPost.Icon.ImageLink");
                    Assert.AreEqual(post.Icon.Description, opPost.Icon.Description, $"{messagePrefix}: opPost.Icon.Description");
                }
                else
                {
                    Assert.IsNull(opPost.Icon, $"{messagePrefix}: opPost.Icon == null");
                }
                Assert.AreEqual(post.Email, opPost.Email, $"{messagePrefix}: opPost.Email");
                Assert.AreEqual(post.Hash, opPost.Hash, $"{messagePrefix}: opPost.Email");
                if (post.Poster != null)
                {
                    Assert.IsNotNull(opPost.Poster, $"{messagePrefix}: opPost.Poster != null");
                    Assert.AreEqual(post.Poster.Name, opPost.Poster.Name, $"{messagePrefix}: opPost.Poster.Name");
                    Assert.AreEqual(post.Poster.Tripcode, opPost.Poster.Tripcode, $"{messagePrefix}: opPost.Poster.Tripcode");
                    Assert.AreEqual(post.Poster.NameColor, opPost.Poster.NameColor, $"{messagePrefix}: opPost.Poster.NameColor");
                    Assert.AreEqual(post.Poster.NameColorStr, opPost.Poster.NameColorStr, $"{messagePrefix}: opPost.Poster.NameColorStr");
                }
                else
                {
                    Assert.IsNull(opPost.Poster, $"{messagePrefix}: opPost.Poster == null");
                }
                Assert.IsNotNull(opPost.Quotes, $"{messagePrefix}: opPost.Quotes != null");
                CollectionAssert.AreEquivalent(post.Quotes.Distinct(BoardLinkEqualityComparer.Instance).Select(l => l.GetLinkHash()).ToList(), opPost.Quotes.Distinct(BoardLinkEqualityComparer.Instance).Select(l => l.GetLinkHash()).ToList(), $"{messagePrefix}: opPost.Quotes");
                Assert.IsNotNull(opPost.MediaFiles, $"{messagePrefix}: opPost.MediaFiles != null");

                Assert.AreEqual(post.MediaFiles.Count, opPost.MediaFiles.Count, $"{messagePrefix}: opPost.MediaFiles.Count");
                for (var i = 0; i < post.MediaFiles.Count; i++)
                {
                    var pMedia = post.MediaFiles[i];
                    var opMedia = opPost.MediaFiles[i];
                    Assert.AreEqual(pMedia is IPostMediaWithSize, opMedia is IPostMediaWithSize, $"{messagePrefix}: opPost.MediaFiles[{i}] is IPostMediaWithSize");
                    Assert.AreEqual(pMedia is IPostMediaWithThumbnail, opMedia is IPostMediaWithThumbnail, $"{messagePrefix}: opPost.MediaFiles[{i}] is IPostMediaWithThumbnail");
                    Assert.AreEqual(pMedia is IPostMediaWithInfo, opMedia is IPostMediaWithInfo, $"{messagePrefix}: opPost.MediaFiles[{i}] is IPostMediaWithInfo");
                    Assert.AreEqual(pMedia.MediaLink?.GetLinkHash(), opMedia.MediaLink?.GetLinkHash(), $"{messagePrefix}: opPost.MediaFiles[{i}].MediaLink");
                    Assert.AreEqual(pMedia.FileSize, opMedia.FileSize, $"{messagePrefix}: opPost.MediaFiles[{i}].FileSize");
                    Assert.AreEqual(pMedia.MediaType, opMedia.MediaType, $"{messagePrefix}: opPost.MediaFiles[{i}].MediaType");
                    if (pMedia is IPostMediaWithSize pMedia2 && opMedia is IPostMediaWithSize opMedia2)
                    {
                        Assert.AreEqual(pMedia2.Size, opMedia2.Size, $"{messagePrefix}: opPost.MediaFiles[{i}].Size");
                    }
                    if (pMedia is IPostMediaWithThumbnail pMedia3 && opMedia is IPostMediaWithThumbnail opMedia3)
                    {
                        Assert.AreEqual(pMedia3.Thumbnail?.MediaLink.GetLinkHash(), opMedia3.Thumbnail?.MediaLink.GetLinkHash(), $"{messagePrefix}: opPost.MediaFiles[{i}].Thumbnail.MediaLink");
                        Assert.AreEqual(pMedia3.Thumbnail?.FileSize, opMedia3.Thumbnail?.FileSize, $"{messagePrefix}: opPost.MediaFiles[{i}].Thumbnail.FileSize");
                        Assert.AreEqual(pMedia3.Thumbnail?.MediaType, opMedia3.Thumbnail?.MediaType, $"{messagePrefix}: opPost.MediaFiles[{i}].Thumbnail.MediaType");
                        Assert.AreEqual(pMedia3.Thumbnail?.Size, opMedia3.Thumbnail?.Size, $"{messagePrefix}: opPost.MediaFiles[{i}].Thumbnail.Size");
                    }
                    if (pMedia is IPostMediaWithInfo pMedia4 && opMedia is IPostMediaWithInfo opMedia4)
                    {
                        Assert.AreEqual(pMedia4.Nsfw, opMedia4.Nsfw, $"{messagePrefix}: opPost.MediaFiles[{i}].Thumbnail.Nsfw");
                        Assert.AreEqual(pMedia4.DisplayName, opMedia4.DisplayName, $"{messagePrefix}: opPost.MediaFiles[{i}].Thumbnail.DisplayName");
                        Assert.AreEqual(pMedia4.Duration, opMedia4.Duration, $"{messagePrefix}: opPost.MediaFiles[{i}].Thumbnail.Duration");
                        Assert.AreEqual(pMedia4.FullName, opMedia4.FullName, $"{messagePrefix}: opPost.MediaFiles[{i}].Thumbnail.FullName");
                        Assert.AreEqual(pMedia4.Hash, opMedia4.Hash, $"{messagePrefix}: opPost.MediaFiles[{i}].Thumbnail.Hash");
                        Assert.AreEqual(pMedia4.Name, opMedia4.Name, $"{messagePrefix}: opPost.MediaFiles[{i}].Thumbnail.Name");
                    }
                }

                var dateDiff = Math.Abs((post.LoadedTime - opPost.LoadedTime).TotalSeconds);
                Assert.IsTrue(dateDiff < 1.5, $"{messagePrefix}: opPost.LoadedTime");
            }
            else
            {
                Assert.Fail($"{messagePrefix}: не типа IBoardPost");
            }
        }

        protected async Task AssertChildrenPositions(IBoardPostCollection collection, PostStoreEntityId collectionId, int windowCount, string collectionName)
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

        protected async Task AssertMediaPositions(IBoardPostCollection collection, PostStoreEntityId collectionId, int windowCount, string collectionName)
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
    }
}