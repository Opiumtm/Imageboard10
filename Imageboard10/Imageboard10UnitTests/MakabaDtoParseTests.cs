using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posting;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Boards;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.Models.Posts.PostMedia;
using Imageboard10.Core.Models.Posts.PostNodes;
using Imageboard10.Core.Models.Serialization;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Core.Network.Html;
using Imageboard10.Core.NetworkInterface;
using Imageboard10.Core.NetworkInterface.Html;
using Imageboard10.Makaba;
using Imageboard10.Makaba.Network.Html;
using Imageboard10.Makaba.Network.Json;
using Imageboard10.Makaba.Network.JsonParsers;
using Imageboard10.Makaba.Network.Uri;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Imageboard10UnitTests
{
    [TestClass]
    [TestCategory("DTO")]
    public class MakabaDtoParseTests
    {
        private IModuleProvider _provider;
        private ModuleCollection _collection;

        [TestInitialize]
        public async Task InitializeTests()
        {
            _collection = new ModuleCollection();
            _collection.RegisterModule<ObjectSerializationService, IObjectSerializationService>();
            PostModelsRegistration.RegisterModules(_collection);
            LinkModelsRegistration.RegisterModules(_collection);
            _collection.RegisterModule<MakabaBoardReferenceDtoParsers, INetworkDtoParsers>();
            _collection.RegisterModule<YoutubeIdService, IYoutubeIdService>();
            _collection.RegisterModule<MakabaLinkParser, IEngineLinkParser>();
            _collection.RegisterModule<AgilityHtmlDocumentFactory, IHtmlDocumentFactory>();
            _collection.RegisterModule<MakabaHtmlParser, IHtmlParser>();
            _collection.RegisterModule<MakabaPostDtoParsers, INetworkDtoParsers>();
            await _collection.Seal();
            _provider = _collection.GetModuleProvider();
        }

        [TestCleanup]
        public async Task CleanupTests()
        {
            await _collection.Dispose();
            _collection = null;
            _provider = null;
        }

        private static readonly string[] RequiredCategories = new[]
        {
            "Взрослым",
            "Игры",
            "Политика",
            "Пользовательские",
            "Разное",
            "Творчество",
            "Тематика",
            "Техника и софт",
            "Японская культура"
        };

        [TestMethod]
        public async Task TestParseBoardInfoDto()
        {
            var boards = await TestResources.LoadBoardReferencesFromResource();
            var parser = _provider.FindNetworkDtoParser<MobileBoardInfoCollection, IList<IBoardReference>>();
            var result = parser.Parse(boards);

            Assert.IsNotNull(result, "Парсер вернул null");
            Assert.IsTrue(result.Count > 0, "Парсер вернул пустой список");

            CheckTestBoardReferencesData(result);
        }

        public static void CheckTestBoardReferencesData(IList<IBoardReference> result)
        {
            var categories = new HashSet<string>(result.Select(r => r.Category), StringComparer.OrdinalIgnoreCase);
            var boardIds = new HashSet<string>(result.Select(r => r.ShortName));
            var boardsById = result.ToDictionary(r => r.ShortName, StringComparer.OrdinalIgnoreCase);

            CheckRequiredCategories(categories);

            CheckSomeBoardsExists(boardIds);

            var mlp = boardsById["mlp"];

            CheckMlpBoardData(mlp);

            var b = boardsById["b"];

            CheckBBoardData(b);
        }

        public static void CheckSomeBoardsExists(ISet<string> boardIds)
        {
            Assert.IsTrue(boardIds.Contains("b"), "Нет доски /b/");
            Assert.IsTrue(boardIds.Contains("mobi"), "Нет доски /mobi/");
            Assert.IsTrue(boardIds.Contains("po"), "Нет доски /po/");
            Assert.IsTrue(boardIds.Contains("vg"), "Нет доски /vg/");
            Assert.IsTrue(boardIds.Contains("t"), "Нет доски /t/");
            Assert.IsTrue(boardIds.Contains("mlp"), "Нет доски /mlp/");
        }

        public static void CheckRequiredCategories(ISet<string> categories)
        {
            Assert.AreEqual(RequiredCategories.Length, categories.Count, $"Количество категорий не совпадает. Ожидание: {RequiredCategories.Length}, Реальность: {categories.Count}");
            foreach (var c in RequiredCategories)
            {
                Assert.IsTrue(categories.Contains(c), $"Нет категории \"{c}\"");
            }
        }

        public static void CheckBBoardData(IBoardReference b)
        {
            Assert.IsNotNull(b.PostingCapabilities, "b.PostingCapabilities != null");
            Assert.IsTrue(b.PostingCapabilities.Count > 0, "b.PostingCapabilities.Count > 0");
            Assert.IsFalse(b.PostingCapabilities.Any(c => c.Role == PostingFieldSemanticRole.Title),
                "В /b/ есть поле PostingFieldSemanticRole.Title");
            Assert.IsTrue(b.PostingCapabilities.Any(c => c.Role == PostingFieldSemanticRole.Comment),
                "В /b/ нет поля PostingFieldSemanticRole.Comment");
            Assert.IsTrue(b.PostingCapabilities.Any(c => c.Role == PostingFieldSemanticRole.Captcha),
                "В /b/ нет поля PostingFieldSemanticRole.Captcha");
            Assert.IsTrue(b.PostingCapabilities.Any(c => c.Role == PostingFieldSemanticRole.MediaFile),
                "В /b/ нет поля PostingFieldSemanticRole.MediaFile");

            var bmedia =
                b.PostingCapabilities.FirstOrDefault(c => c.Role == PostingFieldSemanticRole.MediaFile) as
                    IPostingMediaFileCapability;
            Assert.IsNotNull(bmedia,
                "Поле PostingFieldSemanticRole.MediaFile не поддерживает интерфейс IPostingMediaFileCapability");
            Assert.IsTrue(bmedia.MaxFileCount > 0, "bmedia.MaxFileCount > 0");
        }

        public static void CheckMlpBoardData(IBoardReference mlp)
        {
            Assert.IsNotNull(mlp.Icons, "Нет иконок на доске /mlp/");
            Assert.AreEqual(46, mlp.Icons.Count, "Должно быть ровно 46 иконки в /mlp/");
            for (var i = 0; i < mlp.Icons.Count; i++)
            {
                var icon = mlp.Icons[i];
                Assert.IsNotNull(icon, $"Иконка №{i + 1} в /mlp/ = null");
                Assert.IsNotNull(icon.Name, $"Иконка №{i + 1} в /mlp/, Name = null");
                Assert.IsNotNull(icon.Id, $"Иконка №{i + 1} в /mlp/, Id = null");
                Assert.AreEqual((i + 1).ToString(CultureInfo.InvariantCulture.NumberFormat), icon.Id,
                    $"Иконка №{i + 1} в /mlp/, Id = \"{icon.Id}\"");
                Assert.IsNotNull(icon.MediaLink, $"Иконка №{i + 1} в /mlp/, MediaLink = null");
                Assert.IsTrue(icon.MediaLink is EngineMediaLink, $"Иконка №{i + 1} в /mlp/, тип объекта не EngineMediaLink");
            }

            var applejack = mlp.Icons[1];
            Assert.AreEqual("Applejack", applejack.Name, $"У иконки AppleJack, Name = \"{applejack.Name}\"");
            var applejackLink = applejack.MediaLink as EngineMediaLink;
            Assert.IsNotNull(applejackLink, "applejack.MediaLink не EngineMediaLink");
            Assert.AreEqual(applejackLink.Uri, "/icons/logos/applejack.png", $"applejackLink.Uri = {applejackLink.Uri}");
        }

        [TestMethod]
        public void MakabaPostUrlParse()
        {
            var parser = _provider.QueryEngineCapability<IEngineLinkParser>(MakabaConstants.MakabaEngineId);

            var toCheck = new[]
            {
                "http://2ch.hk/b/res/1234.html#4321",
                "https://2ch.hk/b/res/1234.html#4321",
                "http://2ch.so/b/res/1234.html#4321",
                "https://2ch.so/b/res/1234.html#4321",
                "http://2-ch.so/b/res/1234.html#4321",
                "https://2-ch.so/b/res/1234.html#4321"
            };

            var postLink = new PostLink()
            {
                Board = "b",
                Engine = MakabaConstants.MakabaEngineId,
                OpPostNum = 1234,
                PostNum = 4321
            };

            ILink parsedLink;

            foreach (var uri in toCheck)
            {
                Assert.IsTrue(parser.IsLinkForEngine(uri, false), $"{uri} - ссылка не распознана (IsLinkForEngine)");
                parsedLink = parser.TryParseLink(uri, false);
                Assert.IsNotNull(parsedLink, $"{uri} - ссылка не распознана");
                Assert.AreEqual(postLink.GetLinkHash(), parsedLink.GetLinkHash(), $"{uri} - ссылка распознана неправильно");
            }
        }

        [TestMethod]
        public void MakabaUrlParseFail()
        {
            var parser = _provider.QueryEngineCapability<IEngineLinkParser>(MakabaConstants.MakabaEngineId);

            var toCheck = new[]
            {
                "http://2chc.hk/b/res/1234.html#4321",
                "http://2chc.hk/b/res/1234.html",
                "http://2chc.hk/b/res/1234.html#",
                "httpd://2ch.hk/b/res/1234.html#4321",
                "http:/2ch.hk/b/res/1234.html#4321",
                "http://2ch.so/b/res/123d4.html#4321",
                "https://2ch.so/b/res/1234.htmll#4321",
                "https://2ch.so/b/res/1234.htmll#43d21",
                "http://2-ch.hk/b/res/1234.html#4321",
                "https://2-ch.hk/b/res/1234.html#4321",
                "https://4chan.org/b/res/1234.html#4321",
                "https://4chan.org/b/res/1234.html#4321",
                "https://4chan.org/b/res/1234.html",
                "https://4chan.org/b/res/1234.html",
            };

            ILink parsedLink;

            foreach (var uri in toCheck)
            {
                Assert.IsFalse(parser.IsLinkForEngine(uri, false), $"{uri} - ссылка ошибочно распознана (IsLinkForEngine)");
                parsedLink = parser.TryParseLink(uri, false);
                Assert.IsNull(parsedLink, $"{uri} - ссылка ошибочно распознана");
            }
        }

        [TestMethod]
        public void MakabaThreadUrlParse()
        {
            var parser = _provider.QueryEngineCapability<IEngineLinkParser>(MakabaConstants.MakabaEngineId);

            var toCheck = new[]
            {
                "http://2ch.hk/b/res/1234.html",
                "https://2ch.hk/b/res/1234.html",
                "http://2ch.so/b/res/1234.html",
                "https://2ch.so/b/res/1234.html",
                "http://2-ch.so/b/res/1234.html",
                "https://2-ch.so/b/res/1234.html"
            };

            var postLink = new ThreadLink()
            {
                Board = "b",
                Engine = MakabaConstants.MakabaEngineId,
                OpPostNum = 1234,
            };

            ILink parsedLink;

            foreach (var uri in toCheck)
            {
                Assert.IsTrue(parser.IsLinkForEngine(uri, false), $"{uri} - ссылка не распознана (IsLinkForEngine)");
                parsedLink = parser.TryParseLink(uri, false);
                Assert.IsNotNull(parsedLink, $"{uri} - ссылка не распознана");
                Assert.AreEqual(postLink.GetLinkHash(), parsedLink.GetLinkHash(), $"{uri} - ссылка распознана неправильно");
            }
        }

        [TestMethod]
        public void YoutubeUrlParse()
        {
            var parser = _provider.QueryModule<IYoutubeIdService>();

            var toCheck = new[]
            {
                "https://www.youtube.com/watch?v=FBlEGuU_CqU",
                "https://youtu.be/FBlEGuU_CqU",
            };

            var id = "FBlEGuU_CqU";
            foreach (var uri in toCheck)
            {
                Assert.AreEqual(id, parser.GetYoutubeIdFromUri(uri), $"{uri} - ссылка распознана неправильно");
            }
        }

        [TestMethod]
        public void TestParseHtmlPost()
        {
            var parser = _provider.QueryEngineCapability<IHtmlParser>(MakabaConstants.MakabaEngineId);

            var html = "test<strong>test</strong><em><strong>test 2</strong></em><br><a href=\"http://2ch.hk/b/res/1234.html\">link text</a>";
            var expected = new PostDocument()
            {
                Nodes = new List<IPostNode>()
                {
                    new TextPostNode() { Text = "test"},
                    new CompositePostNode()
                    {
                        Attribute = new PostBasicAttribute() { Attribute = PostBasicAttributes.Bold },
                        Children = new List<IPostNode>()
                        {
                            new TextPostNode() { Text = "test" }
                        }
                    },
                    new CompositePostNode()
                    {
                        Attribute = new PostBasicAttribute() { Attribute = PostBasicAttributes.Italic },
                        Children = new List<IPostNode>()
                        {
                            new CompositePostNode()
                            {
                                Attribute = new PostBasicAttribute() { Attribute = PostBasicAttributes.Bold },
                                Children = new List<IPostNode>()
                                {
                                    new TextPostNode() { Text = "test 2" }
                                }
                            }
                        }
                    },
                    new LineBreakPostNode(),
                    new CompositePostNode()
                    {
                        Attribute = new PostLinkAttribute()
                        {
                            Link = new ThreadLink() { Engine = MakabaConstants.MakabaEngineId, Board = "b", OpPostNum = 1234 }
                        },
                        Children = new List<IPostNode>()
                        {
                            new TextPostNode() { Text = "link text" }
                        }
                    }
                },
            };

            var parsed = parser.ParseHtml(html, null);
            PostModelsTests.AssertDocuments(_provider, expected, parsed);
        }

        [TestMethod]
        public async Task MakabaPostDtoParse()
        {
            var jsonStr = await TestResources.ReadTestTextFile("po_post.json");
            var dto = JsonConvert.DeserializeObject<BoardPost2>(jsonStr);
            Assert.IsNotNull(dto, "dto != null");
            var parser = _provider.FindNetworkDtoParser<BoardPost2WithParentLink, IBoardPost>();
            var param = new BoardPost2WithParentLink()
            {
                Counter = 1,
                IsPreview = true,
                Post = dto,
                ParentLink = new ThreadLink() { Engine  = MakabaConstants.MakabaEngineId, Board = "po", OpPostNum = 22855542 },                
            };
            var result = parser.Parse(param);

            AssertPostFlag(result, BoardPostFlags.Banned, false, "Banned = 0");
            AssertPostFlag(result, BoardPostFlags.Closed, false, "Closed = 0");
            Assert.AreEqual("30/05/17 Втр 20:24:08", result.BoardSpecificDate, "BoardSpecificDate");
            Assert.IsNotNull(result.Likes, "Likes != null");
            Assert.AreEqual(9, result.Likes.Dislikes, "Likes.Dislikes");
            Assert.AreEqual(19, result.Likes.Likes, "Likes.Likes");
            Assert.AreEqual("", result.Email, "Email");
            AssertPostFlag(result, BoardPostFlags.Endless, false, "Endless = 0");
            Assert.IsNotNull(result.MediaFiles, "MediaFiles != null");
            Assert.AreEqual(3, result.MediaFiles.Count, "MediaFiles.Count = 3");
            var pm = result.MediaFiles[0] as PostMediaWithThumbnail;
            Assert.IsNotNull(pm, "MediaFiles[0] is PostMediaWithThumbnail");
            Assert.AreEqual(new SizeInt32() { Width = 640, Height = 480}, pm.Size, "MediaFiles[0].Size = 640x480");
            Assert.AreEqual("sad.jpg", pm.DisplayName, "MediaFiles[0].DisplayName");
            Assert.AreEqual("sad.jpg", pm.FullName, "MediaFiles[0].FullName");
            Assert.AreEqual("63a7d4ad258c81dbd2f22ef5de1907c3", pm.Hash, "MediaFiles[0].Hash");
            Assert.AreEqual("14961650483170.jpg", pm.Name, "MediaFiles[0].Name");
            Assert.AreEqual(false, pm.Nsfw, "MediaFiles[0].Nsfw");
            Assert.AreEqual(pm.MediaType, PostMediaTypes.Image, "MediaFiles[0].MediaType");
            var pmuri = pm.MediaLink as BoardMediaLink;
            Assert.IsNotNull(pmuri, "MediaFiles[0].MediaLink is BoardMediaLink");
            Assert.AreEqual(MakabaConstants.MakabaEngineId, pmuri.Engine, "MediaFiles[0].MediaLink.Engine");
            Assert.AreEqual("po", pmuri.Board, "MediaFiles[0].MediaLink.Board");
            Assert.AreEqual("/src/22855542/14961650483170.jpg", pmuri.Uri, "MediaFiles[0].MediaLink.Board");
            var tm = pm.Thumbnail as PostMediaWithSize;
            Assert.IsNotNull(tm, "MediaFiles[0].Thumbnail is PostMediaWithSize");
            Assert.AreEqual(new SizeInt32() { Width = 250, Height = 187 }, tm.Size, "MediaFiles[0].Thumbnail.Size");
            var tmuri = tm.MediaLink as BoardMediaLink;
            Assert.IsNotNull(tmuri, "MediaFiles[0].Thumbnail.MediaLink is BoardMediaLink");
            Assert.AreEqual(MakabaConstants.MakabaEngineId, tmuri.Engine, "MediaFiles[0].Thumbnail.MediaLink.Engine");
            Assert.AreEqual("po", tmuri.Board, "MediaFiles[0].Thumbnail.MediaLink.Board");
            Assert.AreEqual("/thumb/22855542/14961650483170s.jpg", tmuri.Uri, "MediaFiles[0].Thumbnail.MediaLink.Board");
            Assert.IsNotNull(result.Poster, "Poster != null");
            Assert.AreEqual("Аноним", result.Poster.Name, "Poster.Name");
            Assert.AreEqual("", result.Poster.Tripcode, "Poster.Tripcode");
            Assert.AreEqual(result.Link.GetLinkHash(), (new PostLink()
            {
                Engine = MakabaConstants.MakabaEngineId,
                Board = "po",
                OpPostNum = 22855542,
                PostNum = 22855542
            }).GetLinkHash(), "result.Link");
            AssertPostFlag(result, BoardPostFlags.Op, true, "Op = 1");
            AssertPostFlag(result, BoardPostFlags.ThreadOpPost, true, "ThreadOpPost = 1");
            AssertPostFlag(result, BoardPostFlags.Sticky, false, "Sticky = 0");
        }

        private void AssertPostFlag(IBoardPost post, Guid flag, bool value, string msg)
        {
            var found = post.Flags.Any(f => f == flag);
            if (value)
            {
                Assert.IsTrue(found, msg);
            }
            else
            {
                Assert.IsFalse(found, msg);
            }
        }
    }
}