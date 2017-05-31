using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Storage.Pickers;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.Models.Posts.PostMedia;
using Imageboard10.Core.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Imageboard10UnitTests
{
    [TestClass]
    [TestCategory("PostModels")]
    public class PostModelsTests
    {
        private ModuleCollection _collection;
        private IModuleProvider _modules;

        [TestInitialize]
        public async Task Initialize()
        {
            _collection = new ModuleCollection();
            PostModelsRegistration.RegisterModules(_collection);
            LinkModelsRegistration.RegisterModules(_collection);
            _collection.RegisterModule<FakeExternalPostMediaSerializer, IPostMediaSerializer>();
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

        [TestMethod]
        public void PostMeidaSerialization()
        {
            TestMediaModelSerialization(() => new PostMedia()
            {
                MediaLink = new BoardMediaLink()
                {
                    Engine = "makaba",
                    Board = "b",
                    Uri = "*** uri ***"
                },
                FileSize = 1024,
                MediaType = PostMediaTypes.WebmVideo,
            }, (original, deserialized) =>
            {
                Assert.IsNotNull(deserialized.FileSize, "Размер медиа == null");
                Assert.AreEqual(original.FileSize, deserialized.FileSize, "Не совпадает размер медиа");
                Assert.AreEqual(original.MediaType, deserialized.MediaType, "Не совпадает тип медиа");
            });
            TestMediaModelSerialization(() => new PostMedia()
            {
                MediaLink = new BoardMediaLink()
                {
                    Engine = "makaba",
                    Board = "po",
                    Uri = "*** uri2 ***"
                },
                FileSize = null,
                MediaType = PostMediaTypes.YoutubeVideo,
            }, (original, deserialized) =>
            {
                Assert.IsNull(deserialized.FileSize, "Размер медиа не null");
                Assert.AreEqual(original.MediaType, deserialized.MediaType, "Не совпадает тип медиа");
            });
        }

        [TestMethod]
        public void PostMediaWithSizeSerialization()
        {
            TestMediaModelSerialization(() => new PostMediaWithSize()
            {
                MediaLink = new BoardMediaLink()
                {
                    Engine = "makaba",
                    Board = "b",
                    Uri = "*** uri ***"
                },
                FileSize = 1024,
                MediaType = PostMediaTypes.WebmVideo,
                Size = new SizeInt32() { Height = 150, Width = 250 }
            }, (original, deserialized) =>
            {
                Assert.IsNotNull(deserialized.FileSize, "Размер медиа == null");
                Assert.AreEqual(original.FileSize, deserialized.FileSize, "Не совпадает размер медиа");
                Assert.AreEqual(original.MediaType, deserialized.MediaType, "Не совпадает тип медиа");
                Assert.AreEqual(original.Size, deserialized.Size, "Размеры изображения не совпадают");
            });
        }

        [TestMethod]
        public void PostMediaWithThumbnailSerialization()
        {
            void AssertMediaWithSize(PostMediaWithSize original, PostMediaWithSize deserialized)
            {
                Assert.IsNotNull(deserialized.FileSize, "Размер медиа == null");
                Assert.AreEqual(original.FileSize, deserialized.FileSize, "Не совпадает размер медиа");
                Assert.AreEqual(original.MediaType, deserialized.MediaType, "Не совпадает тип медиа");
                Assert.AreEqual(original.Size, deserialized.Size, "Размеры изображения не совпадают");
            }

            TestMediaModelSerialization(() => new PostMediaWithThumbnail()
            {
                MediaLink = new BoardMediaLink()
                {
                    Engine = "makaba",
                    Board = "b",
                    Uri = "*** uri ***"
                },
                FileSize = 1024,
                MediaType = PostMediaTypes.WebmVideo,
                Size = new SizeInt32() { Height = 150, Width = 250 },
                Thumbnail = new PostMediaWithSize()
                {
                    MediaLink = new BoardMediaLink()
                    {
                        Engine = "makaba",
                        Board = "po",
                        Uri = "*** uri 2 ***"
                    },
                    FileSize = 1024,
                    MediaType = PostMediaTypes.Image,
                    Size = new SizeInt32() { Height = 350, Width = 450 },
                },
                DisplayName = "display name",
                FullName = "full name",
                Nsfw = true                
            }, (original, deserialized) =>
            {
                AssertMediaWithSize(original, deserialized);
                CheckMediaModelsSerialization((PostMediaWithSize)original.Thumbnail, deserialized.Thumbnail, AssertMediaWithSize);
                Assert.AreEqual(original.DisplayName, deserialized.DisplayName, "DisplayName не совпадает");
                Assert.AreEqual(original.FullName, deserialized.FullName, "Full name не совпадает");
                Assert.AreEqual(original.Nsfw, deserialized.Nsfw, "Флаг NSFW не совпадает");
            });

            TestMediaModelSerialization(() => new PostMediaWithThumbnail()
            {
                MediaLink = new BoardMediaLink()
                {
                    Engine = "wakaba",
                    Board = "po",
                    Uri = "*** uri 2 ***"
                },
                FileSize = 1024,
                MediaType = PostMediaTypes.WebmVideo,
                Size = new SizeInt32() { Height = 150, Width = 250 },
                Thumbnail = null,
                DisplayName = "display name 2",
                FullName = "full name 2",
                Nsfw = true
            }, (original, deserialized) =>
            {
                AssertMediaWithSize(original, deserialized);
                Assert.IsNull(deserialized.Thumbnail, "Thumbnail не равен null");
                Assert.AreEqual(original.DisplayName, deserialized.DisplayName, "DisplayName не совпадает");
                Assert.AreEqual(original.FullName, deserialized.FullName, "Full name не совпадает");
                Assert.AreEqual(original.Nsfw, deserialized.Nsfw, "Флаг NSFW не совпадает");
            });
        }

        [TestMethod]
        public void ExternalPostMediaSerialization()
        {
            void AssertMediaWithSize(FakeExternalPostMedia original, FakeExternalPostMedia deserialized)
            {
                Assert.IsNotNull(deserialized.FileSize, "Размер медиа == null");
                Assert.AreEqual(original.FileSize, deserialized.FileSize, "Не совпадает размер медиа");
                Assert.AreEqual(original.MediaType, deserialized.MediaType, "Не совпадает тип медиа");
                Assert.AreEqual(original.Size, deserialized.Size, "Размеры изображения не совпадают");
            }

            TestMediaModelSerialization(() => new FakeExternalPostMedia()
            {
                MediaLink = new BoardMediaLink()
                {
                    Engine = "makaba",
                    Board = "b",
                    Uri = "*** uri ***"
                },
                FileSize = 1024,
                MediaType = PostMediaTypes.WebmVideo,
                Size = new SizeInt32() { Height = 150, Width = 250 }
            }, (original, deserialized) =>
            {
                AssertMediaWithSize(original, deserialized);
            });

            TestMediaModelSerialization(() => new PostMediaWithThumbnail()
            {
                MediaLink = new BoardMediaLink()
                {
                    Engine = "makaba",
                    Board = "b",
                    Uri = "*** uri ***"
                },
                FileSize = 1024,
                MediaType = PostMediaTypes.WebmVideo,
                Size = new SizeInt32() { Height = 150, Width = 250 },
                Thumbnail = new FakeExternalPostMedia()
                {
                    MediaLink = new BoardMediaLink()
                    {
                        Engine = "makaba",
                        Board = "po",
                        Uri = "*** uri 2 ***"
                    },
                    FileSize = 1024,
                    MediaType = PostMediaTypes.Image,
                    Size = new SizeInt32() { Height = 350, Width = 450 },
                },
                DisplayName = "display name",
                FullName = "full name",
                Nsfw = true
            }, (original, deserialized) =>
            {
                CheckMediaModelsSerialization((FakeExternalPostMedia)original.Thumbnail, deserialized.Thumbnail, AssertMediaWithSize);
            });
        }

        private void TestMediaModelSerialization<T>(Func<T> createModel, Action<T, T> asserts)
            where T : IPostMedia
        {
            var m = createModel();
            var provider = _modules.QueryModule<IPostMediaSerializationService>();
            Assert.IsNotNull(provider, "Средство сериализации не найдено");

            var str = provider.SerializeToString(m);
            var m2 = provider.Deserialize(str);
            CheckMediaModelsSerialization(m, m2, asserts);

            var bytes = provider.SerializeToBytes(m);
            var m3 = provider.Deserialize(bytes);
            CheckMediaModelsSerialization(m, m3, asserts);
        }

        private void CheckMediaModelsSerialization<T>(T original, IPostMedia deserialized, Action<T, T> asserts)
            where T : IPostMedia
        {
            Assert.IsNotNull(deserialized, "Десериализованный объект == null");
            Assert.AreNotSame(original, deserialized, "Десериализация вернула тот же объект");
            Assert.IsTrue(BoardLinkEqualityComparer.Instance.Equals(original.MediaLink, deserialized.MediaLink), "Не совпадают ссылки  на медиа");
            Assert.IsInstanceOfType(deserialized, typeof(T), $"Тип объекта не {typeof(T).FullName}");
            asserts?.Invoke(original, (T)deserialized);
        }
    }

    public class FakeExternalPostMedia : IPostMediaWithSize
    {
        [JsonIgnore]
        public ILink MediaLink { get; set; }

        [JsonProperty("MediaLink")]
        public string MediaLinkJson { get; set; }

        [JsonProperty("MediaType")]
        public Guid MediaType { get; set; }

        [JsonProperty("FileSize")]
        public ulong? FileSize { get; set; }

        public Type GetTypeForSerializer() => typeof(FakeExternalPostMedia);

        [JsonIgnore]
        public SizeInt32 Size { get; set; }

        [JsonProperty("Width")]
        public int Width { get; set; }

        [JsonProperty("Height")]
        public int Height { get; set; }

        public void FillValuesBeforeSerialize(IModuleProvider modules)
        {
            Height = Size.Height;
            Width = Size.Width;
            MediaLinkJson = MediaLink.Serialize(modules);
        }

        public FakeExternalPostMedia FillValuesAfterDeserialize(IModuleProvider modules)
        {
            Size = new SizeInt32()
            {
                Height = Height,
                Width = Width
            };
            MediaLink = modules.DeserializeLink(MediaLinkJson);
            MediaLinkJson = null;
            return this;
        }
    }

    public class FakeExternalPostMediaSerializer : ModuleBase<IPostMediaSerializer>, IPostMediaSerializer, IStaticModuleQueryFilter
    {
        public string TypeId => "tests.fakemedia";

        public Type Type => typeof(FakeExternalPostMedia);

        public string SerializeToString(IPostMedia media)
        {
            ((FakeExternalPostMedia)media).FillValuesBeforeSerialize(ModuleProvider);
            return JsonConvert.SerializeObject((FakeExternalPostMedia) media);
        }

        public byte[] SerializeToBytes(IPostMedia media)
        {
            ((FakeExternalPostMedia)media).FillValuesBeforeSerialize(ModuleProvider);
            using (var str = new MemoryStream())
            {
                using (var wr = new BsonDataWriter(str))
                {
                    var s = new JsonSerializer();
                    s.Serialize(wr, (FakeExternalPostMedia)media);
                    wr.Flush();
                }
                return str.ToArray();
            }
        }

        public IPostMedia Deserialize(string data)
        {
            return JsonConvert.DeserializeObject<FakeExternalPostMedia>(data).FillValuesAfterDeserialize(ModuleProvider);
        }

        public IPostMedia Deserialize(byte[] data)
        {
            using (var str = new MemoryStream(data))
            {
                using (var rd = new BsonDataReader(str))
                {
                    var s = new JsonSerializer();
                    return s.Deserialize<FakeExternalPostMedia>(rd).FillValuesAfterDeserialize(ModuleProvider);
                }
            }
        }

        public bool CheckQuery<T>(T query)
        {
            if (query is Type)
            {
                return (query as Type) == Type;
            }
            if (query is string)
            {
                return (query as string) == TypeId;
            }
            return false;
        }
    }
}