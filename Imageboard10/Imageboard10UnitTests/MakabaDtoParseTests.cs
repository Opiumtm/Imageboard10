using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Posting;
using Imageboard10.Core.Models.Boards;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Makaba.Network.Json;
using Imageboard10.Makaba.Network.JsonParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            _collection.RegisterModule<MakabaBoardReferenceDtoParsers, INetworkDtoParsers>();
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
    }
}