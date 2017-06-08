using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Storage.Pickers;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.Models.Posts.PostMedia;
using Imageboard10.Core.Models.Posts.PostNodes;
using Imageboard10.Core.Models.Serialization;
using Imageboard10.Core.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            _collection.RegisterModule<ObjectSerializationService, IObjectSerializationService>();
            PostModelsRegistration.RegisterModules(_collection);
            LinkModelsRegistration.RegisterModules(_collection);
            _collection.RegisterModule<FakeExternalPostMediaSerializer, IObjectSerializer>();
            _collection.RegisterModule<FakePostAttributeSerializer, IObjectSerializer>();
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

        [TestMethod]
        public void BasicAttributeSerialization()
        {
            TestAttributeModelSerialization(() => new PostBasicAttribute()
            {
                Attribute = "attribute"
            }, (original, deserialized) =>
            {
                Assert.AreEqual(original.Attribute, deserialized.Attribute, "Значение базового атрибута не совпадает с исходным");
            });
        }

        [TestMethod]
        public void LinkAttributeSerialization()
        {
            TestAttributeModelSerialization(() => new PostLinkAttribute()
            {
                Link = new BoardMediaLink()
                {
                    Board = "b",
                    Engine = "makaba",
                    Uri = "*** uri ***"
                }
            }, (original, deserialized) =>
            {
                Assert.IsNotNull(deserialized.Link, "Десериализованная ссылка = null");
                Assert.AreEqual(original.Link.GetLinkHash(), deserialized.Link.GetLinkHash(), "Значение ссылки не совпадает с исходным");
            });
        }

        [TestMethod]
        public void ExternalAttributeSerialization()
        {
            TestAttributeModelSerialization(() => new FakePostAttribute()
            {
                Attribute = "attribute"
            }, (original, deserialized) =>
            {
                Assert.AreEqual(original.Attribute, deserialized.Attribute, "Значение базового атрибута не совпадает с исходным");
            });
        }

        private void TestMediaModelSerialization<T>(Func<T> createModel, Action<T, T> asserts)
            where T : IPostMedia
        {
            var m = createModel();
            var provider = _modules.QueryModule<IObjectSerializationService>();
            Assert.IsNotNull(provider, "Средство сериализации не найдено");

            var str = provider.SerializeToString(m);
            var m2 = provider.Deserialize(str) as IPostMedia;
            CheckMediaModelsSerialization(m, m2, asserts);

            var bytes = provider.SerializeToBytes(m);
            var m3 = provider.Deserialize(bytes) as IPostMedia;
            CheckMediaModelsSerialization(m, m3, asserts);
        }

        private void TestAttributeModelSerialization<T>(Func<T> createModel, Action<T, T> asserts)
            where T : IPostAttribute
        {
            var m = createModel();
            var provider = _modules.QueryModule<IObjectSerializationService>();
            Assert.IsNotNull(provider, "Средство сериализации не найдено");

            var str = provider.SerializeToString(m);
            var m2 = provider.Deserialize(str) as IPostAttribute;
            CheckAttributeModelsSerialization(m, m2, asserts);

            var bytes = provider.SerializeToBytes(m);
            var m3 = provider.Deserialize(bytes) as IPostAttribute;
            CheckAttributeModelsSerialization(m, m3, asserts);
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

        private void CheckAttributeModelsSerialization<T>(T original, IPostAttribute deserialized, Action<T, T> asserts)
            where T : IPostAttribute
        {
            Assert.IsNotNull(deserialized, "Десериализованный объект == null");
            Assert.AreNotSame(original, deserialized, "Десериализация вернула тот же объект");
            Assert.IsInstanceOfType(deserialized, typeof(T), $"Тип объекта не {typeof(T).FullName}");
            asserts?.Invoke(original, (T)deserialized);
        }

    }
}