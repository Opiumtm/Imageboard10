using System;
using System.Collections.Generic;
using System.Linq;
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
            _collection.RegisterModule<FakePostNodeSerializer, IObjectSerializer>();
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

        [TestMethod]
        public void TextNodeSerialization()
        {
            TestNodeModelSerialization(() => new TextPostNode()
            {
                Text = "sample text"
            }, (original, deserialized) =>
            {
                Assert.AreEqual(original.Text, deserialized.Text, "Текст не совпадает");
            });
        }

        [TestMethod]
        public void LineBreakNodeSerialization()
        {
            TestNodeModelSerialization(() => new LineBreakPostNode()
            {
            }, (original, deserialized) =>
            {
            });
        }

        [TestMethod]
        public void BoardLinkNodeSerialization()
        {
            TestNodeModelSerialization(() => new BoardLinkPostNode()
            {
                BoardLink = new PostLink()
                {
                    Engine = "makaba",
                    Board = "b",
                    OpPostNum = 1234,
                    PostNum = 4321
                }
            }, (original, deserialized) =>
            {
                Assert.IsNotNull(deserialized.BoardLink, "Ссылка равна null");
                Assert.AreEqual(original.BoardLink.GetLinkHash(), deserialized.BoardLink.GetLinkHash(), "Ссылки не совпадают");
            });
        }

        [TestMethod]
        public void CompositeNodeSerialization()
        {
            TestNodeModelSerialization(() => new CompositePostNode()
            {
                Attribute = null,
                Children = new List<IPostNode>()
                {
                    new TextPostNode() { Text = "sample text"},
                    new BoardLinkPostNode()
                    {
                        BoardLink = new BoardLink() { Engine = "makaba", Board = "po" },                        
                    },
                    new LineBreakPostNode(),
                    new LineBreakPostNode(),
                    new TextPostNode() { Text = "sample text 2"},
                    new CompositePostNode()
                    {
                        Attribute = new PostBasicAttribute()
                        {
                            Attribute = "i"
                        },
                        Children = new List<IPostNode>()
                        {
                            new CompositePostNode()
                            {
                                Attribute = new PostLinkAttribute()
                                {
                                    Link = new PostLink() { Board = "b", Engine = "makaba", OpPostNum = 1234, PostNum = 4321 },                                    
                                },
                                ChildrenContracts = new List<PostNodeBase>()
                                {
                                    new TextPostNode() { Text = "sample text 3"},
                                    new LineBreakPostNode(),
                                    new TextPostNode() { Text = "sample text 4"},
                                }
                            },
                            new CompositePostNode()
                            {
                                Attribute = null,
                                Children = new List<IPostNode>()
                            },
                            new CompositePostNode()
                            {
                                Attribute = null,
                                Children = null
                            }
                        }
                    },
                    new LineBreakPostNode()
                }
            }, (original, deserialized) =>
            {
                AssertNodes(_modules, original, deserialized);
            });
        }

        [TestMethod]
        public void ExternalNodeSerialization()
        {
            TestNodeModelSerialization(() => new FakePostNode()
            {
                Text = "fake sample text",
            }, (original, deserialized) =>
            {
                Assert.AreEqual(original.Text, deserialized.Text, "Текст не совпадает");
            });

            TestNodeModelSerialization(() => new CompositePostNode()
            {
                Attribute = null,
                Children = new List<IPostNode>()
                {
                    new FakePostNode()
                    {
                        Text = "fake sample text 2"
                    }
                }
            }, (original, deserialized) =>
            {
                AssertNodes(_modules, original, deserialized, null, (o, d) =>
                {
                    if (o is FakePostNode)
                    {
                        var of = (FakePostNode) o;
                        var df = (FakePostNode) d;
                        Assert.AreEqual(of.Text, df.Text, "Текст не совпадает");
                    }
                });
            });

        }

        [TestMethod]
        public void CompositeDocumentSerialization()
        {
            TestDocumentModelSerialization(() => new PostDocument()
            {
                Nodes = new List<IPostNode>()
                {
                    new CompositePostNode()
                    {
                        Attribute = null,
                        Children = new List<IPostNode>()
                        {
                            new TextPostNode() { Text = "sample text"},
                            new BoardLinkPostNode()
                            {
                                BoardLink = new BoardLink() { Engine = "makaba", Board = "po" },
                            },
                            new LineBreakPostNode(),
                            new LineBreakPostNode(),
                            new TextPostNode() { Text = "sample text 2"},
                            new CompositePostNode()
                            {
                                Attribute = new PostBasicAttribute()
                                {
                                    Attribute = "i"
                                },
                                Children = new List<IPostNode>()
                                {
                                    new CompositePostNode()
                                    {
                                        Attribute = new PostLinkAttribute()
                                        {
                                            Link = new PostLink() { Board = "b", Engine = "makaba", OpPostNum = 1234, PostNum = 4321 },
                                        },
                                        ChildrenContracts = new List<PostNodeBase>()
                                        {
                                            new TextPostNode() { Text = "sample text 3"},
                                            new LineBreakPostNode(),
                                            new TextPostNode() { Text = "sample text 4"},
                                        }
                                    },
                                    new CompositePostNode()
                                    {
                                        Attribute = null,
                                        Children = new List<IPostNode>()
                                    },
                                    new CompositePostNode()
                                    {
                                        Attribute = null,
                                        Children = null
                                    }
                                }
                            },
                            new LineBreakPostNode()
                        }
                    },
                    new TextPostNode() { Text = "sample text 5" }
                }
            } , (original, deserialized) =>
            {
                if (original?.Nodes != null)
                {
                    for (var i = 0; i < original.Nodes.Count; i++)
                    {
                        AssertNodes(_modules, original.Nodes[i], deserialized.Nodes[i], new int[] {i + 1});
                    }
                }
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

        private void TestNodeModelSerialization<T>(Func<T> createModel, Action<T, T> asserts)
            where T : IPostNode
        {
            var m = createModel();
            var provider = _modules.QueryModule<IObjectSerializationService>();
            Assert.IsNotNull(provider, "Средство сериализации не найдено");

            var str = provider.SerializeToString(m);
            var m2 = provider.Deserialize(str) as IPostNode;
            CheckPostModelsSerialization(m, m2, asserts);

            var bytes = provider.SerializeToBytes(m);
            var m3 = provider.Deserialize(bytes) as IPostNode;
            CheckPostModelsSerialization(m, m3, asserts);
        }

        private void TestDocumentModelSerialization<T>(Func<T> createModel, Action<T, T> asserts)
            where T : IPostDocument
        {
            var m = createModel();
            var provider = _modules.QueryModule<IObjectSerializationService>();
            Assert.IsNotNull(provider, "Средство сериализации не найдено");

            var str = provider.SerializeToString(m);
            var m2 = provider.Deserialize(str) as IPostDocument;
            CheckPostDocumentSerialization(m, m2, asserts);

            var bytes = provider.SerializeToBytes(m);
            var m3 = provider.Deserialize(bytes) as IPostDocument;
            CheckPostDocumentSerialization(m, m3, asserts);
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

        private void CheckPostModelsSerialization<T>(T original, IPostNode deserialized, Action<T, T> asserts)
            where T : IPostNode
        {
            Assert.IsNotNull(deserialized, "Десериализованный объект == null");
            Assert.AreNotSame(original, deserialized, "Десериализация вернула тот же объект");
            Assert.IsInstanceOfType(deserialized, typeof(T), $"Тип объекта не {typeof(T).FullName}");
            asserts?.Invoke(original, (T)deserialized);
        }

        private void CheckPostDocumentSerialization<T>(T original, IPostDocument deserialized, Action<T, T> asserts)
            where T : IPostDocument
        {
            Assert.IsNotNull(deserialized, "Десериализованный объект == null");
            Assert.AreNotSame(original, deserialized, "Десериализация вернула тот же объект");
            Assert.IsInstanceOfType(deserialized, typeof(T), $"Тип объекта не {typeof(T).FullName}");
            if (original.Nodes == null)
            {
                Assert.IsNull(deserialized.Nodes, "deserialized.Nodes должно быть не null");
            }
            else
            {
                Assert.IsNotNull(deserialized.Nodes, "deserialized.Nodes не должно быть null");
                Assert.AreNotSame(original.Nodes, deserialized.Nodes, "Должны быть разные объекты");
                Assert.AreEqual(original.Nodes.Count, deserialized.Nodes.Count, "Должно быть равное количество элементов");
            }
            asserts?.Invoke(original, (T)deserialized);
        }

        public static void AssertDocuments(IModuleProvider modules, IPostDocument original, IPostDocument deserialized, int[] path = null, Action<IPostNode, IPostNode> assertCallback = null)
        {
            if (original == null)
            {
                Assert.IsNull(deserialized, "Документ должен быть null");
            }
            else
            {
                Assert.IsNotNull(deserialized, "Документ не должен быть null");
                if (original.Nodes == null)
                {
                    Assert.IsNull(deserialized.Nodes, "Узлы документа должны быть null");
                }
                else
                {
                    Assert.IsNotNull(deserialized.Nodes, "Узлы документа не должны быть null");
                    Assert.AreEqual(original.Nodes.Count, deserialized.Nodes.Count, "Количество узлов документа не совпадает");
                    for (var i = 0; i < original.Nodes.Count; i++)
                    {
                        AssertNodes(modules, original.Nodes[i], deserialized.Nodes[i], new [] { i + 1 }, assertCallback);
                    }
                }
            }
        }

        public static void AssertNodes(IModuleProvider modules, IPostNode original, IPostNode deserialized, int[] path = null, Action<IPostNode, IPostNode> assertCallback = null)
        {
            var p = path != null ? path.Aggregate(new StringBuilder(), (sb, n) => (sb.Length > 0 ? sb.Append("/") : sb).Append(n)).ToString() : "";
            if (original == null)
            {
                Assert.IsNull(deserialized, $"{p} Объект должен быть null");
            }
            else
            {
                Assert.IsNotNull(deserialized, $"{p} Объект не должен быть null");
                Assert.AreEqual(original.GetType(), deserialized.GetType(), $"{p} Тип объекта не совпадает");
                Assert.AreNotSame(original, deserialized, "Должны быть разные объекты");
                switch (original)
                {
                    case TextPostNode ot:
                        var dt = (TextPostNode) deserialized;
                        Assert.AreEqual(ot.Text, dt.Text, $"{p} Текст не совпадает");
                        break;
                    case BoardLinkPostNode obl:
                        var dbl = (BoardLinkPostNode) deserialized;
                        Assert.AreEqual(obl.BoardLink?.GetLinkHash(), dbl.BoardLink?.GetLinkHash(), $"{p} Ссылка не совпадает");
                        break;
                    case CompositePostNode oc:
                        var dc = (CompositePostNode) deserialized;
                        var oat = modules.QueryModule<IObjectSerializationService>().SerializeToString(oc.Attribute);
                        var dat = modules.QueryModule<IObjectSerializationService>().SerializeToString(dc.Attribute);
                        Assert.AreEqual(oat, dat, $"{p} Атрибут узла не совпадает");
                        if (oc.Children == null)
                        {
                            Assert.IsNull(dc.Children, $"{p} Список дочерних атрибутов должен быть null");
                        }
                        else
                        {
                            Assert.IsNotNull(dc.Children, $"{p} Список дочерних узлов не должен быть null");
                            Assert.AreEqual(oc.Children.Count, dc.Children.Count, $"{p} Количество дочерних узлов не совпадает");
                            for (var i = 0; i < dc.Children.Count; i++)
                            {
                                AssertNodes(modules, oc.Children[i], dc.Children[i], (path ?? new int[0]).Concat(new [] { i+1 }).ToArray(), assertCallback);
                            }
                        }
                        break;
                }
                assertCallback?.Invoke(original, deserialized);
            }
        }
    }
}