using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.UI;
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
                LoadedTime = DateTimeOffset.Now
            };
            var result = parser.Parse(param);
            Assert.IsNotNull(result);
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
            Assert.IsNull(pm.Duration, "MediaFiles[0].Duration");
            Assert.AreEqual(false, pm.Nsfw, "MediaFiles[0].Nsfw");
            Assert.AreEqual(PostMediaTypes.Image, pm.MediaType, "MediaFiles[0].MediaType");
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
            Assert.AreEqual(param.LoadedTime, result.LoadedTime, "LoadedTime");
            Assert.IsNotNull(result.Comment, "Comment != null");
            Assert.AreEqual("Понимаете, что это навсегда?", (result.Comment.Nodes?.FirstOrDefault() as ITextPostNode)?.Text, "Comment.Nodes.First()");
        }

        [TestMethod]
        public async Task MakabaPostDtoModnames()
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
                ParentLink = new ThreadLink() { Engine = MakabaConstants.MakabaEngineId, Board = "po", OpPostNum = 22855542 },
            };

            IBoardPost result;

            dto.Name = null;

            dto.Tripcode = "!!%mod%!!";
            result = parser.Parse(param);
            Assert.AreEqual("## Mod ##", result?.Poster?.Name);

            dto.Tripcode = "!!%adm%!!";
            result = parser.Parse(param);
            Assert.AreEqual("## Abu ##", result?.Poster?.Name);

            dto.Tripcode = "!!%Inquisitor%!!";
            result = parser.Parse(param);
            Assert.AreEqual("## Applejack ##", result?.Poster?.Name);

            dto.Tripcode = "!!%coder%!!";
            result = parser.Parse(param);
            Assert.AreEqual("## Кодер ##", result?.Poster?.Name);
        }

        [TestMethod]
        public async Task MakabaPostDtoFlags()
        {
            var jsonStr = await TestResources.ReadTestTextFile("po_post2.json");
            var dto = JsonConvert.DeserializeObject<BoardPost2>(jsonStr);
            Assert.IsNotNull(dto, "dto != null");
            var parser = _provider.FindNetworkDtoParser<BoardPost2WithParentLink, IBoardPost>();
            var param = new BoardPost2WithParentLink()
            {
                Counter = 1,
                IsPreview = true,
                Post = dto,
                ParentLink = new ThreadLink() { Engine = MakabaConstants.MakabaEngineId, Board = "po", OpPostNum = 22855542 },
            };
            var result = parser.Parse(param);
            AssertPostFlag(result, BoardPostFlags.Banned, true, "Banned = 1");
            AssertPostFlag(result, BoardPostFlags.Closed, true, "Closed = 1");
            AssertPostFlag(result, BoardPostFlags.Endless, true, "Endless = 1");
            AssertPostFlag(result, BoardPostFlags.Endless, true, "Endless = 1");
            AssertPostFlag(result, BoardPostFlags.Op, false, "Op = 0");
            AssertPostFlag(result, BoardPostFlags.Sticky, true, "Sticky = 1");
        }

        [TestMethod]
        public async Task MakabaPostDtoCountry()
        {
            var jsonStr = await TestResources.ReadTestTextFile("int_post.json");
            var dto = JsonConvert.DeserializeObject<BoardPost2>(jsonStr);
            Assert.IsNotNull(dto, "dto != null");
            var parser = _provider.FindNetworkDtoParser<BoardPost2WithParentLink, IBoardPost>();
            var param = new BoardPost2WithParentLink()
            {
                Counter = 1,
                IsPreview = true,
                Post = dto,
                ParentLink = new ThreadLink() { Engine = MakabaConstants.MakabaEngineId, Board = "int", OpPostNum = 20441 },
            };
            var result = parser.Parse(param);
            Assert.IsNotNull(result.Country, "Country != null");
            Assert.IsNotNull(result.Country.ImageLink, "Country.ImageLink != null");
            Assert.IsInstanceOfType(result.Country.ImageLink, typeof(EngineMediaLink), "Country.ImageLink is EngineMediaLink");
            var ml = (EngineMediaLink)result.Country.ImageLink;
            Assert.AreEqual("/flags/RU.png", ml.Uri, "Country.ImageLink.Uri");
        }

        [TestMethod]
        public async Task MakabaPostDtoWebmFile()
        {
            var jsonStr = await TestResources.ReadTestTextFile("int_post.json");
            var dto = JsonConvert.DeserializeObject<BoardPost2>(jsonStr);
            Assert.IsNotNull(dto, "dto != null");
            var parser = _provider.FindNetworkDtoParser<BoardPost2WithParentLink, IBoardPost>();
            var param = new BoardPost2WithParentLink()
            {
                Counter = 1,
                IsPreview = true,
                Post = dto,
                ParentLink = new ThreadLink() { Engine = MakabaConstants.MakabaEngineId, Board = "int", OpPostNum = 20441 },
            };
            var result = parser.Parse(param);
            Assert.IsNotNull(result.MediaFiles, "MediaFiles != null");
            Assert.AreEqual(1, result.MediaFiles.Count, "MediaFiles.Count");
            var mf = result.MediaFiles[0] as PostMediaWithThumbnail;
            Assert.IsNotNull(mf, "result.MediaFiles[0] is PostMediaWithThumbnail");
            Assert.AreEqual("00:00:20", mf.Duration, "result.MediaFiles[0].Duration");
            Assert.AreEqual(PostMediaTypes.WebmVideo, mf.MediaType, "result.MediaFiles[0].MediaType");
        }

        [TestMethod]
        public async Task MakabaPostDtoIcon()
        {
            var jsonStr = await TestResources.ReadTestTextFile("mlp_post.json");
            var dto = JsonConvert.DeserializeObject<BoardPost2>(jsonStr);
            Assert.IsNotNull(dto, "dto != null");
            var parser = _provider.FindNetworkDtoParser<BoardPost2WithParentLink, IBoardPost>();
            var param = new BoardPost2WithParentLink()
            {
                Counter = 1,
                IsPreview = true,
                Post = dto,
                ParentLink = new ThreadLink() { Engine = MakabaConstants.MakabaEngineId, Board = "int", OpPostNum = 20441 },
            };
            var result = parser.Parse(param);
            Assert.IsNotNull(result.Icon, "Icon != null");
            Assert.IsNotNull(result.Icon.ImageLink, "Icon.ImageLink != null");
            Assert.IsInstanceOfType(result.Icon.ImageLink, typeof(EngineMediaLink), "Icon.ImageLink is EngineMediaLink");
            var ml = (EngineMediaLink)result.Icon.ImageLink;
            Assert.AreEqual("/icons/logos/spike.png", ml.Uri, "Icon.ImageLink.Uri");
            Assert.AreEqual("Spike", result.Icon.Description, "Icon.Description");
        }

        [TestMethod]
        public async Task MakabaPostDtoNameColors()
        {
            var jsonStr = await TestResources.ReadTestTextFile("po_post3.json");
            var dto = JsonConvert.DeserializeObject<BoardPost2>(jsonStr);
            Assert.IsNotNull(dto, "dto != null");
            var parser = _provider.FindNetworkDtoParser<BoardPost2WithParentLink, IBoardPost>();
            var param = new BoardPost2WithParentLink()
            {
                Counter = 1,
                IsPreview = true,
                Post = dto,
                ParentLink = new ThreadLink() { Engine = MakabaConstants.MakabaEngineId, Board = "po", OpPostNum = 22823169 },
            };
            var result = parser.Parse(param);
            Assert.IsNotNull(result.Poster, "Poster != null");
            Assert.IsNotNull(result.Poster.NameColor, "Poster.NameColor != null");
            Assert.AreEqual(Color.FromArgb(255, 163, 13, 175), result.Poster.NameColor.Value, "Poster.NameColor");
            Assert.AreEqual("Бенедикт\xA0Оскарович", result.Poster.Name, "Poster.Name");
        }

        [TestMethod]
        public async Task MakabaIndexParse()
        {
            var jsonStr = await TestResources.ReadTestTextFile("mlp_index.json");
            var dto = JsonConvert.DeserializeObject<BoardEntity2>(jsonStr);
            Assert.IsNotNull(dto, "dto != null");
            var parser = _provider.FindNetworkDtoParser<BoardPageData, IBoardPageThreadCollection>();
            Assert.IsNotNull(parser, "parser != null");
            var param = new BoardPageData()
            {
                Link = new BoardPageLink() {Board = "mlp", Engine = MakabaConstants.MakabaEngineId, Page = 0},
                Etag = "##etag##",
                LoadedTime = DateTimeOffset.Now,
                Entity = dto
            };
            var result = parser.Parse(param);
            Assert.IsNotNull(result, "result != null");
            Assert.AreEqual(param.Etag, result.Etag, "Etag");
            Assert.AreEqual(param.Link.GetLinkHash(), result.Link?.GetLinkHash(), "Link");
            Assert.AreEqual(param.Link.GetRootLink().GetLinkHash(), result.ParentLink?.GetLinkHash(), "ParentLink");
            Assert.AreEqual(dto.Threads.Length, result.Threads.Count, "Threads.Count");

            AssertCollectionInfo<IBoardPostCollectionInfoBoard>(result.Info, info =>
            {
                Assert.AreEqual("mlp", info.Board, "info,Board->Board");
                Assert.AreEqual("My Little Pony", info.BoardName, "info, Board->BoardName");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoBoardLimits>(result.Info, info =>
            {
                Assert.AreEqual("Pony", info.DefaultName, "info,Limits->DefaultName");
                Assert.AreEqual((ulong) (40960 * 1024), info.MaxFilesSize, "info,Limits->MaxFilesSize");
                Assert.AreEqual(15000, info.MaxComment, "info,Limits->MaxComment");
                Assert.IsNotNull(info.Pages, "info,Limits->Pages != null");
                CollectionAssert.AreEquivalent(new List<int>()
                {
                    0,
                    1,
                    2,
                    3,
                    4,
                    5,
                    6,
                    7
                } as ICollection, info.Pages as ICollection, "Limits->Pages != null");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoBoardBanner>(result.Info, info =>
            {
                Assert.AreEqual(new SizeInt32() {Width = 300, Height = 100}, info.BannerSize, "info,BoardBanner->BannerSize");
                Assert.AreEqual((new EngineMediaLink() {Engine = MakabaConstants.MakabaEngineId, Uri = "/ololo/fet_1.jpg"}).GetLinkHash(), info.BannerImageLink?.GetLinkHash());
                Assert.AreEqual((new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "fet"}).GetLinkHash(), info.BannerBoardLink?.GetLinkHash());
            });

            AssertCollectionInfo<IBoardPostCollectionInfoBoardDesc>(result.Info, info =>
            {
                var doc = new PostDocument()
                {
                    Nodes = new List<IPostNode>()
                    {
                        new TextPostNode() {Text = "Правило 34 только в соответствующих тредах. Настоящие кони скачут в /ne/, фурри – в /fur/. Гуро и флаффи запрещены."}
                    }
                };
                PostModelsTests.AssertDocuments(_provider, doc, info.BoardInfo);
                Assert.AreEqual("Мои маленькие пони, дружба, магия", info.BoardInfoOuter, "info,BoardDesc->BoardInfoOuter");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoBoardsAdvertisement>(result.Info, info =>
            {
                Assert.IsNotNull(info.AdvertisementItems, "info,BoardsAdvertisement->AdvertisementItems != null");
                Assert.AreEqual(6, info.AdvertisementItems.Count, "info,BoardsAdvertisement->AdvertisementItems.Count");
                var expBoards = new ILink[]
                {
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "2d"},
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "wwe"},
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "ch"},
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "int"},
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "ruvn"},
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "math"},
                };
                CollectionAssert.AreEqual(expBoards, info.AdvertisementItems.Select(i => i.BoardLink).ToList(), BoardLinkComparer.Instance as IComparer);
                var eb = info.AdvertisementItems[0];
                Assert.AreEqual("Щитпостинг, обсуждение вайфу, аватарки и прочее. Анимешный /b/, постинг 3d не приветствуется.", eb.Info);
                Assert.AreEqual("Аниме/Беседка", eb.Name);
            });

            AssertCollectionInfo<IBoardPostCollectionInfoBottomAdvertisement>(result.Info, info =>
            {
                var l1 = new EngineMediaLink() { Engine = MakabaConstants.MakabaEngineId, Uri = "/banners/kPptGmThLL7w9tz1.png" };
                var l2 = new EngineUriLink() { Engine = MakabaConstants.MakabaEngineId, Uri = "/banners/kPptGmThLL7w9tz1/" };
                Assert.AreEqual(l1.GetLinkHash(), info.AdvertisementBannerLink?.GetLinkHash());
                Assert.AreEqual(l2.GetLinkHash(), info.AdvertisementClickLink?.GetLinkHash());
            });

            AssertCollectionInfo<IBoardPostCollectionInfoPostingSpeed>(result.Info, info =>
            {
                Assert.AreEqual(20, info.Speed);
            });

            AssertCollectionInfo<IBoardPostCollectionInfoIcons>(result.Info, info =>
            {
                Assert.IsNotNull(info.Icons, "info.Icons != null");
                Assert.AreEqual(46, info.Icons.Count, "info.Icons.Count");
                var i1 = info.Icons[0];
                Assert.IsNotNull(i1, "info.Icons[0] != null");
                Assert.AreEqual("1", i1.Id, "info.Icons[0].Id");
                Assert.AreEqual("Twilight Sparkle", i1.Name, "info.Icons[0].Name");
                var i2 = info.Icons[1];
                Assert.IsNotNull(i1, "info.Icons[1] != null");
                Assert.AreEqual("2", i2.Id, "info.Icons[1].Id");
                Assert.AreEqual("Applejack", i2.Name, "info.Icons[1].Name");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoNews>(result.Info, info =>
            {
                Assert.IsNotNull(info.NewsItems, "info.NewsItems != null");
                Assert.AreEqual(3, info.NewsItems.Count, "info.NewsItems.Count");
                var n = info.NewsItems[0];
                Assert.IsNotNull(n, "info.NewsItems[0] != null");
                Assert.AreEqual("02/12/16", n.Date, "info.NewsItems[0].Date");
                Assert.AreEqual("Конкурс визуальных новелл доски /ruvn/", n.Title, "info.NewsItems[0].Title");
                Assert.AreEqual((new ThreadLink()
                {
                    Engine = MakabaConstants.MakabaEngineId,
                    Board = "abu",
                    OpPostNum = 54946
                }).GetLinkHash(), n.NewsLink.GetLinkHash(), "info.NewsItems[0].NewsLink");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoLocation>(result.Info, info =>
            {
                Assert.AreEqual("mlp", info.Board, "info,Location->Board");
                Assert.AreEqual(0, info.CurrentPage, "info,Location->CurrentPage");
                Assert.AreEqual(0, info.CurrentThread, "info,Location->CurrentThread");
                Assert.IsNull(info.MaxPostNumber, "info,Location->MaxPostNumber");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoUniquePosters>(result.Info, info =>
            {
                Assert.IsNull(info.UniquePosters, "info,UniquePosters->UniquePosters");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoTitle>(result.Info, info =>
            {
                Assert.IsNull(info.Title, "info,Title->Title");
            });

            AssertPostCollectionFlags(result.Info, new []
            {
                (PostCollectionFlags.EnableDices, true, "EnableDices"),
                (PostCollectionFlags.EnableCountryFlags, false, "EnableCountryFlags"),
                (PostCollectionFlags.EnableIcons, true, "EnableIcons"),
                (PostCollectionFlags.EnableImages, true, "EnableImages"),
                (PostCollectionFlags.EnableLikes, false, "EnableLikes"),
                (PostCollectionFlags.EnableNames, true, "EnableNames"),
                (PostCollectionFlags.EnableOekaki, false, "EnableOekaki"),
                (PostCollectionFlags.EnablePosting, true, "EnablePosting"),
                (PostCollectionFlags.EnableSage, true, "EnableSage"),
                (PostCollectionFlags.EnableShield, false, "EnableShield"),
                (PostCollectionFlags.EnableSubject, true, "EnableSubject"),
                (PostCollectionFlags.EnableThreadTags, false, "EnableThreadTags"),
                (PostCollectionFlags.EnableTripcodes, true, "EnableTripcodes"),
                (PostCollectionFlags.EnableVideo, true, "EnableVideo"),
                (PostCollectionFlags.IsBoard, true, "IsBoard"),
                (PostCollectionFlags.IsIndex, false, "IsIndex"),
            });

            var t1 = result.Threads[1];
            Assert.IsNotNull(t1, "t1 != null");
            Assert.IsNotNull(t1.ImageCount, "t1.ImageCount");
            Assert.IsNotNull(t1.ReplyCount, "t1.ReplyCount");
            Assert.IsNotNull(t1.Omit, "t1.Omit");
            Assert.IsNotNull(t1.OmitImages, "t1.OmitImages");
        }

        [TestMethod]
        public async Task MakabaThreadParse()
        {
            var jsonStr = await TestResources.ReadTestTextFile("mobi_thread.json");
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
            var result = parser.Parse(param);
            Assert.IsNotNull(result, "result != null");
            Assert.AreEqual(param.Etag, result.Etag, "Etag");
            Assert.AreEqual(param.Link.GetLinkHash(), result.Link?.GetLinkHash(), "Link");
            Assert.AreEqual(param.Link.GetBoardLink().GetLinkHash(), result.ParentLink?.GetLinkHash(), "ParentLink");

            Assert.AreEqual(287, result.Posts.Count, "Posts.Count");

            AssertCollectionInfo<IBoardPostCollectionInfoBoard>(result.Info, info =>
            {
                Assert.AreEqual("mobi", info.Board, "info,Board->Board");
                Assert.AreEqual("Мобильные устройства и приложения", info.BoardName, "info, Board->BoardName");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoBoardLimits>(result.Info, info =>
            {
                Assert.AreEqual("Аноним", info.DefaultName, "info,Limits->DefaultName");
                Assert.AreEqual((ulong)(40960 * 1024), info.MaxFilesSize, "info,Limits->MaxFilesSize");
                Assert.AreEqual(15000, info.MaxComment, "info,Limits->MaxComment");
                Assert.IsNull(info.Pages, "info,Limits->Pages != null");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoBoardDesc>(result.Info, info =>
            {
                var doc = new PostDocument()
                {
                    Nodes = new List<IPostNode>()
                    {
                        new TextPostNode() {Text = "Доска для мобильных анонов. О покупке и мелких вопросах спрашивают прикрепленном треде. Читалки - в /bo/, наушники и плееры - в /t/."}
                    }
                };
                PostModelsTests.AssertDocuments(_provider, doc, info.BoardInfo);
                Assert.AreEqual("мобильные телефоны, приложения, iphone, android, winphone, 2ch browser", info.BoardInfoOuter, "info,BoardDesc->BoardInfoOuter");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoBoardBanner>(result.Info, info =>
            {
                Assert.AreEqual((new EngineMediaLink() { Engine = MakabaConstants.MakabaEngineId, Uri = "/ololo/t_1.jpg" }).GetLinkHash(), info.BannerImageLink?.GetLinkHash());
                Assert.AreEqual((new BoardLink() { Engine = MakabaConstants.MakabaEngineId, Board = "t" }).GetLinkHash(), info.BannerBoardLink?.GetLinkHash());
            });

            AssertCollectionInfo<IBoardPostCollectionInfoBoardsAdvertisement>(result.Info, info =>
            {
                Assert.IsNotNull(info.AdvertisementItems, "info,BoardsAdvertisement->AdvertisementItems != null");
                Assert.AreEqual(6, info.AdvertisementItems.Count, "info,BoardsAdvertisement->AdvertisementItems.Count");
                var expBoards = new ILink[]
                {
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "2d"},
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "wwe"},
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "ch"},
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "int"},
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "ruvn"},
                    new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "math"},
                };
                CollectionAssert.AreEqual(expBoards, info.AdvertisementItems.Select(i => i.BoardLink).ToList(), BoardLinkComparer.Instance as IComparer);
                var eb = info.AdvertisementItems[0];
                Assert.AreEqual("Щитпостинг, обсуждение вайфу, аватарки и прочее. Анимешный /b/, постинг 3d не приветствуется.", eb.Info);
                Assert.AreEqual("Аниме/Беседка", eb.Name);
            });

            AssertCollectionInfo<IBoardPostCollectionInfoBottomAdvertisement>(result.Info, info =>
            {
                var l1 = new EngineMediaLink() { Engine = MakabaConstants.MakabaEngineId, Uri = "/banners/kPptGmThLL7w9tz1.png" };
                var l2 = new EngineUriLink() { Engine = MakabaConstants.MakabaEngineId, Uri = "/banners/kPptGmThLL7w9tz1/" };
                Assert.AreEqual(l1.GetLinkHash(), info.AdvertisementBannerLink?.GetLinkHash());
                Assert.AreEqual(l2.GetLinkHash(), info.AdvertisementClickLink?.GetLinkHash());
            });

            AssertCollectionInfo<IBoardPostCollectionInfoPostingSpeed>(result.Info, info =>
            {
                Assert.AreEqual(0, info.Speed, "info.Speed");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoNews>(result.Info, info =>
            {
                Assert.IsNotNull(info.NewsItems, "info.NewsItems != null");
                Assert.AreEqual(3, info.NewsItems.Count, "info.NewsItems.Count");
                var n = info.NewsItems[0];
                Assert.IsNotNull(n, "info.NewsItems[0] != null");
                Assert.AreEqual("02/12/16", n.Date, "info.NewsItems[0].Date");
                Assert.AreEqual("Конкурс визуальных новелл доски /ruvn/", n.Title, "info.NewsItems[0].Title");
                Assert.AreEqual((new ThreadLink()
                {
                    Engine = MakabaConstants.MakabaEngineId,
                    Board = "abu",
                    OpPostNum = 54946
                }).GetLinkHash(), n.NewsLink.GetLinkHash(), "info.NewsItems[0].NewsLink");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoLocation>(result.Info, info =>
            {
                Assert.AreEqual("mobi", info.Board, "info,Location->Board");
                Assert.IsNull(info.CurrentPage, "info,Location->CurrentPage");
                Assert.AreEqual(1153568, info.CurrentThread, "info,Location->CurrentThread");
                Assert.AreEqual(1155354, info.MaxPostNumber, "info,Location->MaxPostNumber");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoUniquePosters>(result.Info, info =>
            {
                Assert.AreEqual(81, info.UniquePosters, "info,UniquePosters->UniquePosters");
            });

            AssertCollectionInfo<IBoardPostCollectionInfoTitle>(result.Info, info =>
            {
                Assert.AreEqual("Windows 10 Mobile #263 каникулярный", info.Title, "info,Title->Title");
            });

            AssertPostCollectionFlags(result.Info, new[]
            {
                (PostCollectionFlags.EnableDices, false, "EnableDices"),
                (PostCollectionFlags.EnableCountryFlags, false, "EnableCountryFlags"),
                (PostCollectionFlags.EnableIcons, false, "EnableIcons"),
                (PostCollectionFlags.EnableImages, true, "EnableImages"),
                (PostCollectionFlags.EnableLikes, false, "EnableLikes"),
                (PostCollectionFlags.EnableNames, true, "EnableNames"),
                (PostCollectionFlags.EnableOekaki, false, "EnableOekaki"),
                (PostCollectionFlags.EnablePosting, true, "EnablePosting"),
                (PostCollectionFlags.EnableSage, true, "EnableSage"),
                (PostCollectionFlags.EnableShield, false, "EnableShield"),
                (PostCollectionFlags.EnableSubject, true, "EnableSubject"),
                (PostCollectionFlags.EnableThreadTags, true, "EnableThreadTags"),
                (PostCollectionFlags.EnableTripcodes, false, "EnableTripcodes"),
                (PostCollectionFlags.EnableVideo, true, "EnableVideo"),
                (PostCollectionFlags.IsBoard, false, "IsBoard"),
                (PostCollectionFlags.IsIndex, false, "IsIndex"),
            });

            var p15 = result.Posts[14];
            Assert.IsNotNull(p15, "p15 != null");
            AssertPostFlag(p15, BoardPostFlags.Sage, true, "BoardPostFlags.Sage");
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

        private void AssertPostCollectionFlags(IBoardPostCollectionInfoSet set, (Guid flag, bool isSet, string flagName)[] flags)
        {
            AssertCollectionInfo<IBoardPostCollectionInfoFlags>(set, info =>
            {
                foreach (var f in flags)
                {
                    AssertPostCollectionFlag(info, f.flag, f.isSet, "Info, Flags:" + f.flagName);
                }
            });
        }

        private void AssertPostCollectionFlag(IBoardPostCollectionInfoFlags flags, Guid flag, bool value, string msg)
        {
            var found = flags.Flags.Any(f => f == flag);
            if (value)
            {
                Assert.IsTrue(found, msg);
            }
            else
            {
                Assert.IsFalse(found, msg);
            }
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