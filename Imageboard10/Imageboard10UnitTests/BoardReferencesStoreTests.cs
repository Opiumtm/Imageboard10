using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Imageboard10.Core;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.ModelStorage.Boards;
using Imageboard10.Core.ModelStorage.UnitTests;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Core.Tasks;
using Imageboard10.Makaba;
using Imageboard10.Makaba.Network.Json;
using Imageboard10.Makaba.Network.JsonParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Imageboard10UnitTests
{
    [TestClass]
    [TestCategory("ModelStore")]
    public class BoardReferencesStoreTests
    {
        private ModuleCollection _collection;
        private IModuleProvider _provider;
        private IBoardReferenceStore _store;
        private IBoardReferenceStoreForTests _testData;

        [TestInitialize]
        public async Task Initialize()
        {
            _collection = new ModuleCollection();

            LinkModelsRegistration.RegisterModules(_collection);
            _collection.RegisterModule<EsentInstanceProvider, IEsentInstanceProvider>(new EsentInstanceProvider(true));
            _collection.RegisterModule<BoardReferenceStore, IBoardReferenceStore>(new BoardReferenceStore("makaba"));
            _collection.RegisterModule<MakabaBoardReferenceDtoParsers, INetworkDtoParsers>();

            TableVersionStatusForTests.ClearInstance();
            await _collection.Seal();
            _provider = _collection.GetModuleProvider();
            var module = _provider.QueryModule(typeof(IBoardReferenceStore), "makaba") ?? throw new ModuleNotFoundException();
            _store = module.QueryView<IBoardReferenceStore>() ?? throw new ModuleNotFoundException();
            _testData = module.QueryView<IBoardReferenceStoreForTests>() ?? throw new ModuleNotFoundException();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await _collection.Dispose();
            _collection = null;
            _provider = null;
            _store = null;
            _testData = null;
        }

        [TestMethod]
        public async Task BoardReferenceTablesCreated()
        {
            await _testData.WaitForInitialization();
            Assert.IsTrue(await _testData.IsTablePresent(_testData.TableversionTableName), "Не найдена таблица с версиями");
            Assert.IsTrue(await _testData.IsTablePresent(_testData.BoardsTableName), "Не найдена таблица с досками");
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
        public async Task BoardReferenceUploadData()
        {
            var sourceData = await UploadTestData();
            var counter = await _store.GetCount(new BoardReferenceStoreQuery());
            Assert.IsTrue(counter > 0, "Не загружено ни одной доски");
            Assert.AreEqual(sourceData.Count, counter, "Количество досок в базе не совпадает с исходным количеством");

            var categories = new HashSet<string>(await _store.GetAllCategories());
            MakabaDtoParseTests.CheckRequiredCategories(categories);

            var boardIds = new HashSet<string>((await _store.LoadShortReferences(0, int.MaxValue, new BoardReferenceStoreQuery())).Select(si => si.ShortName), StringComparer.OrdinalIgnoreCase);
            MakabaDtoParseTests.CheckSomeBoardsExists(boardIds);

            var mlp = await _store.LoadReference(new BoardLink() {Engine = MakabaConstants.MakabaEngineId, Board = "mlp"});
            MakabaDtoParseTests.CheckMlpBoardData(mlp);

            var b = await _store.LoadReference(new BoardLink() { Engine = MakabaConstants.MakabaEngineId, Board = "b" });
            MakabaDtoParseTests.CheckBBoardData(b);

            int srcCategoriesCount, categoriesCount;

            srcCategoriesCount = sourceData.Select(s => s.Category).Distinct().Count();
            categoriesCount = await _store.GetCategoryCount();
            Assert.AreEqual(srcCategoriesCount, categoriesCount, $"Количество категорий (IsAdult = null) не совпадает с исходным. Ожидание: {srcCategoriesCount}, Реальность: {categoriesCount}");

            srcCategoriesCount = sourceData.Where(s => s.IsAdult == false).Select(s => s.Category).Distinct().Count();
            categoriesCount = await _store.GetCategoryCount(false);
            Assert.AreEqual(srcCategoriesCount, categoriesCount, $"Количество категорий (IsAdult = false) не совпадает с исходным. Ожидание: {srcCategoriesCount}, Реальность: {categoriesCount}");

            srcCategoriesCount = sourceData.Where(s => s.IsAdult == true).Select(s => s.Category).Distinct().Count();
            categoriesCount = await _store.GetCategoryCount(true);
            Assert.AreEqual(srcCategoriesCount, categoriesCount, $"Количество категорий (IsAdult = true) не совпадает с исходным. Ожидание: {srcCategoriesCount}, Реальность: {categoriesCount}");

            List<string> srcCategories;
            List<string> realCategories;

            srcCategories = sourceData.Where(s => s.IsAdult == false).Select(s => s.Category).Distinct().ToList();
            realCategories = (await _store.GetAllCategories(false)).ToList();
            CollectionAssert.AreEquivalent(srcCategories, realCategories, $"Не совпадаеют категории при IsAdult = false");

            srcCategories = sourceData.Where(s => s.IsAdult == true).Select(s => s.Category).Distinct().ToList();
            realCategories = (await _store.GetAllCategories(true)).ToList();
            CollectionAssert.AreEquivalent(srcCategories, realCategories, $"Не совпадаеют категории при IsAdult = false");

            int srcBoardsCount, boardsCount;

            srcBoardsCount = sourceData.Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            boardsCount = await _store.GetCount(new BoardReferenceStoreQuery());
            Assert.AreEqual(srcBoardsCount, boardsCount, $"Количество досок (IsAdult = null) не совпадает с исходным. Ожидание: {srcBoardsCount}, Реальность: {boardsCount}");

            srcBoardsCount = sourceData.Where(s => s.IsAdult == false).Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            boardsCount = await _store.GetCount(new BoardReferenceStoreQuery() { IsAdult = false });
            Assert.AreEqual(srcBoardsCount, boardsCount, $"Количество досок (IsAdult = false) не совпадает с исходным. Ожидание: {srcBoardsCount}, Реальность: {boardsCount}");

            srcBoardsCount = sourceData.Where(s => s.IsAdult == true).Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            boardsCount = await _store.GetCount(new BoardReferenceStoreQuery() { IsAdult = true });
            Assert.AreEqual(srcBoardsCount, boardsCount, $"Количество досок (IsAdult = true) не совпадает с исходным. Ожидание: {srcBoardsCount}, Реальность: {boardsCount}");

            bool?[] adultFlags = new bool?[] { null, false, true };
            var allCategories = sourceData.Select(s => s.Category).Distinct().ToArray();

            foreach (var adultFlag in adultFlags)
            {
                string afn;
                if (adultFlag == true)
                {
                    afn = "true";
                } else if (adultFlag == false)
                {
                    afn = "false";
                }
                else
                {
                    afn = "null";
                }
                foreach (var c in allCategories)
                {
                    srcBoardsCount = sourceData.Where(s => (s.IsAdult == adultFlag || adultFlag == null) && s.Category == c).Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).Count();
                    boardsCount = await _store.GetCount(new BoardReferenceStoreQuery() { IsAdult = adultFlag, Category = c });
                    Assert.AreEqual(srcBoardsCount, boardsCount, $"Количество досок (IsAdult = {afn}, Category = \"{c}\") не совпадает с исходным. Ожидание: {srcBoardsCount}, Реальность: {boardsCount}");
                }
            }

            List<string> srcBoards;
            List<string> dstBoards;

            srcBoards = sourceData.Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            dstBoards = (await _store.LoadShortReferences(0, int.MaxValue, new BoardReferenceStoreQuery())).Select(s => s.ShortName).ToList();
            CollectionAssert.AreEquivalent(srcBoards, dstBoards, $"Полученные доски (IsAdult = null) не совпадают с исходными.");

            srcBoards = sourceData.Where(s => s.IsAdult == false).Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            dstBoards = (await _store.LoadShortReferences(0, int.MaxValue, new BoardReferenceStoreQuery() { IsAdult = false })).Select(s => s.ShortName).ToList();
            CollectionAssert.AreEquivalent(srcBoards, dstBoards, $"Полученные доски (IsAdult = false) не совпадают с исходными.");

            srcBoards = sourceData.Where(s => s.IsAdult == true).Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            dstBoards = (await _store.LoadShortReferences(0, int.MaxValue, new BoardReferenceStoreQuery() { IsAdult = true })).Select(s => s.ShortName).ToList();
            CollectionAssert.AreEquivalent(srcBoards, dstBoards, $"Полученные доски (IsAdult = true) не совпадают с исходными.");

            foreach (var adultFlag in adultFlags)
            {
                string afn;
                if (adultFlag == true)
                {
                    afn = "true";
                }
                else if (adultFlag == false)
                {
                    afn = "false";
                }
                else
                {
                    afn = "null";
                }
                foreach (var c in allCategories)
                {
                    srcBoards = sourceData.Where(s => (s.IsAdult == adultFlag || adultFlag == null) && s.Category == c).Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    dstBoards = (await _store.LoadShortReferences(0, int.MaxValue, new BoardReferenceStoreQuery() { IsAdult = adultFlag, Category = c})).Select(s => s.ShortName).ToList();
                    CollectionAssert.AreEquivalent(srcBoards, dstBoards, $"Полученные доски (IsAdult = {afn}, Category = \"{c}\") не совпадают с исходными.");
                }
            }

            async Task<IList<IBoardShortInfo>> LoadPaged(BoardReferenceStoreQuery query)
            {
                var cnt = await _store.GetCount(query);
                const int pageSize = 5;
                List<IBoardShortInfo> result = new List<IBoardShortInfo>();
                int cnt2 = 100;
                while (result.Count < cnt && cnt2 > 0)
                {
                    cnt2--;
                    result.AddRange(await _store.LoadShortReferences(result.Count, pageSize, query));
                }
                return result;
            }

            srcBoards = sourceData.Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            dstBoards = (await LoadPaged(new BoardReferenceStoreQuery())).Select(s => s.ShortName).ToList();
            CollectionAssert.AreEquivalent(srcBoards, dstBoards, $"Полученные доски (IsAdult = null) не совпадают с исходными.");

            srcBoards = sourceData.Where(s => s.IsAdult == false).Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            dstBoards = (await LoadPaged(new BoardReferenceStoreQuery() { IsAdult = false })).Select(s => s.ShortName).ToList();
            CollectionAssert.AreEquivalent(srcBoards, dstBoards, $"Полученные доски (IsAdult = false) не совпадают с исходными.");

            srcBoards = sourceData.Where(s => s.IsAdult == true).Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            dstBoards = (await LoadPaged(new BoardReferenceStoreQuery() { IsAdult = true })).Select(s => s.ShortName).ToList();
            CollectionAssert.AreEquivalent(srcBoards, dstBoards, $"Полученные доски (IsAdult = true) не совпадают с исходными.");

            foreach (var adultFlag in adultFlags)
            {
                string afn;
                if (adultFlag == true)
                {
                    afn = "true";
                }
                else if (adultFlag == false)
                {
                    afn = "false";
                }
                else
                {
                    afn = "null";
                }
                foreach (var c in allCategories)
                {
                    srcBoards = sourceData.Where(s => (s.IsAdult == adultFlag || adultFlag == null) && s.Category == c).Select(s => s.ShortName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    dstBoards = (await LoadPaged(new BoardReferenceStoreQuery() { IsAdult = adultFlag, Category = c })).Select(s => s.ShortName).ToList();
                    CollectionAssert.AreEquivalent(srcBoards, dstBoards, $"Полученные доски (IsAdult = {afn}, Category = \"{c}\") не совпадают с исходными.");
                }
            }

            var toGet = sourceData.Take(50).Select(t => t.BoardLink).ToArray();
            var got = (await _store.LoadShortReferences(toGet)).Select(t => t.BoardLink).ToArray();
            CollectionAssert.AreEquivalent(toGet.Select(l => l.GetLinkHash()).ToArray(), got.Select(l => l.GetLinkHash()).ToArray(), "Взятые ссылки не соответствуют запрошенным");
        }

        [TestMethod]
        public async Task BoardReferenceUploadDataBenchmark()
        {
            const int iterations = 10;
            var boards = await TestResources.LoadBoardReferencesFromResource();
            var parser = _provider.FindNetworkDtoParser<MobileBoardInfoCollection, IList<IBoardReference>>();
            var result = parser.Parse(boards);

            var st = new Stopwatch();
            st.Start();
            for (var i = 0; i < iterations; i++)
            {
                await _store.UpdateReferences(result, true);
            }
            st.Stop();
            Logger.LogMessage("Время загрузки полного списка досок в базу: {0:F2} сек. всего, {1:F2} мс на итерацию", st.Elapsed.TotalSeconds, st.Elapsed.TotalMilliseconds / iterations);
        }

        [TestMethod]
        public async Task BoardReferenceParallelQuery()
        {
            var boards = await TestResources.LoadBoardReferencesFromResource();
            var parser = _provider.FindNetworkDtoParser<MobileBoardInfoCollection, IList<IBoardReference>>();
            var result = parser.Parse(boards);
            await _store.UpdateReferences(result, true);

            async Task<Nothing> Query()
            {
                var links = result.Take(40).Select(r => r.BoardLink);
                foreach (var l in links)
                {
                    var o = await _store.LoadReference(l);
                    Assert.IsNotNull(o, "Не получена информация о доске");
                    Assert.IsTrue(BoardLinkEqualityComparer.Instance.Equals(l, o.BoardLink), "Ссылка не соответствует исходной");
                }
                return Nothing.Value;
            }

            for (var i = 0; i < 4; i++)
            {
                await _collection.Suspend();
                await _collection.Resume();
                var toWait = new Task[]
                {
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                    CoreTaskHelper.RunAsyncFuncOnNewThread(Query),
                };

                await Task.WhenAll(toWait);
            }
        }

        private async Task<IList<IBoardReference>> UploadTestData()
        {
            var boards = await TestResources.LoadBoardReferencesFromResource();
            var parser = _provider.FindNetworkDtoParser<MobileBoardInfoCollection, IList<IBoardReference>>();
            var result = parser.Parse(boards);
            var st = new Stopwatch();
            st.Start();
            await _store.UpdateReferences(result, true);
            st.Stop();
            Logger.LogMessage("Время загрузки полного списка досок в базу = {0:F2} мс", st.Elapsed.TotalMilliseconds);
            return result;
        }
    }
}