﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Imageboard10.Core.Database;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.ModelStorage.Blobs;
using Imageboard10.Core.ModelStorage.UnitTests;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Makaba.Network.JsonParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Imageboard10UnitTests
{
    [TestClass]
    [TestCategory("ModelStore")]
    public class BlobStoreTests
    {
        private ModuleCollection _collection;
        private IModuleProvider _provider;
        private IBlobsModelStore _store;

        [TestInitialize]
        public async Task Initialize()
        {
            _collection = new ModuleCollection();

            LinkModelsRegistration.RegisterModules(_collection);
            _collection.RegisterModule<EsentInstanceProvider, IEsentInstanceProvider>(new EsentInstanceProvider(true));
            _collection.RegisterModule<BlobsModelStore, IBlobsModelStore>();
            _collection.RegisterModule<MakabaBoardReferenceDtoParsers, INetworkDtoParsers>();

            TableVersionStatusForTests.ClearInstance();
            await _collection.Seal();
            _provider = _collection.GetModuleProvider();
            var module = _provider.QueryModule<object>(typeof(IBlobsModelStore), null) ?? throw new ModuleNotFoundException();
            _store = module.QueryView<IBlobsModelStore>() ?? throw new ModuleNotFoundException();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await _collection.Dispose();
            _collection = null;
            _provider = null;
            _store = null;
        }

        [TestMethod]
        public async Task BlobStoreUploadSmall()
        {
            var tmpId = Guid.NewGuid();
            var tmpFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(tmpId.ToString());
            long size;
            byte[] originalHash;
            using (var str = await tmpFile.OpenStreamForWriteAsync())
            {
                using (var wr = new BinaryWriter(str))
                {
                    for (int i = 0; i < 1024; i++)
                    {
                        wr.Write(i);
                    }
                    wr.Flush();
                    size = str.Length;
                    str.Position = 0;
                    originalHash = await GetStreamHash(str);
                }
            }
            BlobId id;
            using (var b = await tmpFile.OpenStreamForReadAsync())
            {
                id = await _store.SaveBlob(new InputBlob()
                {
                    BlobStream = b,
                    Category = "test",
                    UniqueName = tmpFile.Name
                }, CancellationToken.None);
            }
            using (var str = await _store.LoadBlob(id))
            {
                Assert.AreEqual(BlobStreamKind.Inlined, BlobStreamInfo.GetBlobStreamKind(str), "Тип потока не совпадает");
                Assert.AreEqual(size, str.Length, "Размер блоба не совпадает с исходным");
                var newHash = await GetStreamHash(str);
                Assert.AreEqual(Convert.ToBase64String(originalHash), Convert.ToBase64String(newHash), "Хэш-код данных не совпадает с исходным");
            }
        }

        [TestMethod]
        public async Task BlobStoreUploadBig()
        {
            var tmpId = Guid.NewGuid();
            var tmpFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(tmpId.ToString());
            long size;
            byte[] originalHash;
            using (var str = await tmpFile.OpenStreamForWriteAsync())
            {
                using (var wr = new BinaryWriter(str))
                {
                    for (int i = 0; i < 1024 * 1024 * 2; i++)
                    {
                        wr.Write(i);
                    }
                    wr.Flush();
                    size = str.Length;
                    str.Position = 0;
                    originalHash = await GetStreamHash(str);
                }
            }
            BlobId id;
            using (var b = await tmpFile.OpenStreamForReadAsync())
            {
                id = await _store.SaveBlob(new InputBlob()
                {
                    BlobStream = b,
                    Category = "test",
                    UniqueName = tmpFile.Name
                }, CancellationToken.None);
            }
            using (var str = await _store.LoadBlob(id))
            {
                Assert.AreEqual(BlobStreamKind.Normal, BlobStreamInfo.GetBlobStreamKind(str), "Тип потока не совпадает");
                Assert.AreEqual(size, str.Length, "Размер блоба не совпадает с исходным");
                var newHash = await GetStreamHash(str);
                Assert.AreEqual(Convert.ToBase64String(originalHash), Convert.ToBase64String(newHash), "Хэш-код данных не совпадает с исходным");
            }
        }

        [TestMethod]
        public async Task BlobStoreFindByName()
        {
            (var tmpFile, var id, var size) = await UploadSmallTempFile();
            var id2 = await _store.FindBlob(tmpFile.Name);
            Assert.AreEqual(id, id2, "ID найденного файла не совпадает");
            var id3 = await _store.FindBlob(Guid.NewGuid().ToString());
            Assert.IsNull(id3, "Найден несуществующий файл");
            var i = await _store.GetBlobInfo(id);
            Assert.IsNotNull(i, $"Не получилось взять информацию о файле ID={id.Id}");
            Assert.IsNull(i.Value.Category, $"Категория != null, ID={id.Id}");
            Assert.IsNull(i.Value.ReferenceId, $"ReferenceId != null, ID={id.Id}");
            Assert.AreEqual(size, i.Value.Size, $"Размер файла не совпадает, ID={id.Id}");
            Assert.AreEqual(tmpFile.Name, i.Value.UniqueName, $"Имя файла не совпадает, ID={id.Id}");
        }

        [TestMethod]
        public async Task BlobStoreCountAndDelete()
        {
            var files = new List<(StorageFile tempFile, BlobId id, long size)>();
            const int filecount = 10;
            for (var i = 0; i < filecount; i++)
            {
                files.Add(await UploadSmallTempFile());
            }
            var count = await _store.GetBlobsCount();
            Assert.AreEqual(files.Count, count, "Количество файлов не совпадает");
            var desiredSize = files.Sum(f => f.size);
            var size = await _store.GetTotalSize();
            Assert.AreEqual(desiredSize, size, "Общий размер файлов не совпадает");

            foreach (var f in files)
            {
                var i = await _store.GetBlobInfo(f.id);
                Assert.IsNotNull(i, $"Не получилось взять информацию о файле ID={f.id.Id}");
                Assert.IsNull(i.Value.Category, $"Категория != null, ID={f.id.Id}");
                Assert.IsNull(i.Value.ReferenceId, $"ReferenceId != null, ID={f.id.Id}");
                Assert.AreEqual(f.size, i.Value.Size, $"Размер файла не совпадает, ID={f.id.Id}");
                Assert.AreEqual(f.tempFile.Name, i.Value.UniqueName, $"Имя файла не совпадает, ID={f.id.Id}");
            }

            Assert.IsTrue(await _store.DeleteBlob(files[0].id), "Не получилось удалить файл");
            files.RemoveAt(0);
            count = await _store.GetBlobsCount();
            Assert.AreEqual(files.Count, count, "Количество файлов не совпадает (после удаления файла)");
            desiredSize = files.Sum(f => f.size);
            size = await _store.GetTotalSize();
            Assert.AreEqual(desiredSize, size, "Общий размер файлов не совпадает (после удаления файла)");

            var ids = files.Take(3).Select(f => f.id).ToArray();
            files.RemoveAt(0);
            files.RemoveAt(0);
            files.RemoveAt(0);
            var delIds = await _store.DeleteBlobs(ids);
            CollectionAssert.AreEquivalent(ids, delIds, "Список удалённых файлов не совпадает с исходным");

            count = await _store.GetBlobsCount();
            Assert.AreEqual(files.Count, count, "Количество файлов не совпадает (после удаления 3 файлов)");
            desiredSize = files.Sum(f => f.size);
            size = await _store.GetTotalSize();
            Assert.AreEqual(desiredSize, size, "Общий размер файлов не совпадает (после удаления 3 файлов)");

            await _store.DeleteAllBlobs();
            files.Clear();
            count = await _store.GetBlobsCount();
            Assert.AreEqual(0, count, "Количество файлов не совпадает (после удаления всех файлов)");
            size = await _store.GetTotalSize();
            Assert.AreEqual(0, size, "Общий размер файлов не совпадает (после удаления всех файлов)");
        }

        [TestMethod]
        public async Task BlobStoreCategories()
        {
            const string category0 = null;
            const string category1 = "Категория 1";
            const string category2 = "Категория 2";

            const int filecount = 10;
            var files0 = new List<(StorageFile tempFile, BlobId id, long size)>();
            var files1 = new List<(StorageFile tempFile, BlobId id, long size)>();
            var files2 = new List<(StorageFile tempFile, BlobId id, long size)>();
            for (var i = 0; i < filecount; i++)
            {
                files0.Add(await UploadSmallTempFile(category0));
            }
            for (var i = 0; i < filecount + 1; i++)
            {
                files1.Add(await UploadSmallTempFile(category1));
            }
            for (var i = 0; i < filecount + 2; i++)
            {
                files2.Add(await UploadSmallTempFile(category2));
            }

            async Task AssertCategory(List<(StorageFile tempFile, BlobId id, long size)> files, string category, string suffix)
            {
                var desiredCount = files.Count;
                var desiredSize = files.Select(f => f.size).DefaultIfEmpty(0).Sum();
                var count = await _store.GetCategoryBlobsCount(category);
                var size = await _store.GetCategorySize(category);
                var infos = (await _store.ReadCategory(category)).ToDictionary(i => i.Id);
                var filesById = files.ToDictionary(f => f.id);

                Assert.AreEqual(desiredCount, count, $"Категория {category}, количество файлов не совпадает{suffix}");
                Assert.AreEqual(desiredCount, infos.Count, $"Категория {category}, количество записей с информацией не совпадает{suffix}");
                Assert.AreEqual(desiredSize, size, $"Категория {category}, размер не совпадает{suffix}");

                foreach (var id in filesById.Keys)
                {
                    Assert.IsTrue(infos.ContainsKey(id), $"Категория {category}, информация о файле {id.Id} не найдена{suffix}");
                    var desired = filesById[id];
                    var info = infos[id];
                    Assert.AreEqual(desired.size, info.Size, $"Категория {category}, размер файла {id.Id} не совпадает{suffix}");
                    Assert.AreEqual(desired.tempFile.Name, info.UniqueName, $"Категория {category}, имя файла {id.Id} не совпадает{suffix}");
                    Assert.AreEqual(category, info.Category, $"Категория {category}, категория файла {id.Id} не совпадает{suffix}");
                }
            }

            await AssertCategory(files0, category0, "");
            await AssertCategory(files1, category1, "");
            await AssertCategory(files2, category2, "");

            Assert.IsTrue(await _store.DeleteBlob(files0[0].id), $"Не удалось удалить файл {files0[0].id}");
            files0.RemoveAt(0);
            Assert.IsTrue(await _store.DeleteBlob(files1[0].id), $"Не удалось удалить файл {files1[0].id}");
            files1.RemoveAt(0);
            Assert.IsTrue(await _store.DeleteBlob(files2[0].id), $"Не удалось удалить файл {files2[0].id}");
            files2.RemoveAt(0);
            await AssertCategory(files0, category0, ", после удаления 1 файла");
            await AssertCategory(files1, category1, ", после удаления 1 файла");
            await AssertCategory(files2, category2, ", после удаления 1 файла");

            await _store.DeleteBlobs(files0.Select(f => f.id).ToArray());
            files0.Clear();
            await _store.DeleteBlobs(files1.Select(f => f.id).ToArray());
            files1.Clear();
            await _store.DeleteBlobs(files2.Select(f => f.id).ToArray());
            files2.Clear();
            await AssertCategory(files0, category0, ", после удаления всех файлов");
            await AssertCategory(files1, category1, ", после удаления всех файлов");
            await AssertCategory(files2, category2, ", после удаления всех файлов");
        }

        [TestMethod]
        public async Task BlobStoreReferenced()
        {
            var ref1 = Guid.NewGuid();
            var ref2 = Guid.NewGuid();

            const int filecount = 10;
            var files1 = new List<(StorageFile tempFile, BlobId id, long size)>();
            var files2 = new List<(StorageFile tempFile, BlobId id, long size)>();
            for (var i = 0; i < filecount; i++)
            {
                files1.Add(await UploadSmallTempFile(null, ref1));
            }
            for (var i = 0; i < filecount + 1; i++)
            {
                files2.Add(await UploadSmallTempFile(null, ref2));
            }

            async Task AssertReferenced(List<(StorageFile tempFile, BlobId id, long size)> files, Guid referenceId, string suffix)
            {
                var desiredCount = files.Count;
                var desiredSize = files.Select(f => f.size).DefaultIfEmpty(0).Sum();
                var count = await _store.GetReferencedBlobsCount(referenceId);
                var size = await _store.GetReferencedSize(referenceId);
                var infos = (await _store.ReadReferencedBlobs(referenceId)).ToDictionary(i => i.Id);
                var filesById = files.ToDictionary(f => f.id);

                Assert.AreEqual(desiredCount, count, $"Ссылка {referenceId}, количество файлов не совпадает{suffix}");
                Assert.AreEqual(desiredCount, infos.Count, $"Ссылка {referenceId}, количество записей с информацией не совпадает{suffix}");
                Assert.AreEqual(desiredSize, size, $"Ссылка {referenceId}, размер не совпадает{suffix}");

                foreach (var id in filesById.Keys)
                {
                    Assert.IsTrue(infos.ContainsKey(id), $"Ссылка {referenceId}, информация о файле {id.Id} не найдена{suffix}");
                    var desired = filesById[id];
                    var info = infos[id];
                    Assert.AreEqual(desired.size, info.Size, $"Ссылка {referenceId}, размер файла {id.Id} не совпадает{suffix}");
                    Assert.AreEqual(desired.tempFile.Name, info.UniqueName, $"Ссылка {referenceId}, имя файла {id.Id} не совпадает{suffix}");
                    Assert.IsNotNull(info.ReferenceId, $"Ссылка {referenceId}, ссылка на файл {id.Id} равна null{suffix}");
                    Assert.AreEqual(referenceId, info.ReferenceId, $"Ссылка {referenceId}, ссылка на файл {id.Id} не совпадает{suffix}");
                }
            }

            await AssertReferenced(files1, ref1, "");
            await AssertReferenced(files2, ref2, "");

            Assert.IsTrue(await _store.DeleteBlob(files1[0].id), $"Не удалось удалить файл {files1[0].id}");
            files1.RemoveAt(0);
            Assert.IsTrue(await _store.DeleteBlob(files2[0].id), $"Не удалось удалить файл {files2[0].id}");
            files2.RemoveAt(0);
            await AssertReferenced(files1, ref1, ", после удаления 1 файла");
            await AssertReferenced(files2, ref2, ", после удаления 1 файла");

            await _store.DeleteBlobs(files1.Select(f => f.id).ToArray());
            files1.Clear();
            await _store.DeleteBlobs(files2.Select(f => f.id).ToArray());
            files2.Clear();
            await AssertReferenced(files1, ref1, ", после удаления всех файлов");
            await AssertReferenced(files2, ref2, ", после удаления всех файлов");
        }

        [TestMethod]
        public async Task BlobStoreReferences()
        {
            var refs = new Guid[]
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
            };
            var refsList = new List<Guid>();
            foreach (var r in refs)
            {
                await _store.AddPermanentReference(r);
                refsList.Add(r);
            }
            var crefs = await _store.CheckIfReferencesPermanent(refs);

            CollectionAssert.AreEquivalent(refs, crefs, "Список постоянных ссылок не совпадает");
            await _store.RemovePermanentReference(refsList[0]);
            refsList.RemoveAt(0);
            await _store.RemovePermanentReference(refsList[0]);
            refsList.RemoveAt(0);
            crefs = await _store.CheckIfReferencesPermanent(refs);
            CollectionAssert.AreEquivalent(refsList.ToArray(), crefs, "Список постоянных ссылок не совпадает (после удаления 2 ссылок)");

            await _store.DeleteAllReferences();
            crefs = await _store.CheckIfReferencesPermanent(refs);
            Assert.AreEqual(0, crefs.Length, "Остались не удалённые ссылки после удаления всех ссылок");
        }

        [TestMethod]
        public async Task BlobStoreUploadError()
        {
            using (var fakeStr = new FakeErrorStream())
            {
                await Assert.ThrowsExceptionAsync<NotSupportedException>(async () =>
                {
                    await _store.SaveBlob(new InputBlob()
                    {
                        UniqueName = Guid.NewGuid().ToString(),
                        // ReSharper disable once AccessToDisposedClosure
                        BlobStream = fakeStr
                    }, CancellationToken.None);
                }, "Должен бросить исключение NotSupportedException");
            }

            Assert.AreEqual(0, await _store.GetUncompletedBlobsCount(), "Должен был удалить созданный файл при ошибке загрузки");
            Assert.AreEqual(0, await _store.GetUncompletedTotalSize(), "Должен был удалить созданный файл при ошибке загрузки");
        }

        [TestMethod]
        public async Task BlobStoreUncompleted()
        {
            var rid = Guid.NewGuid();
            var f = new[]
            {
                await UploadSmallTempFile("test", rid),
                await UploadSmallTempFile("test", rid),
            };
            var ids = f.Select(a => a.id).ToArray();

            var totalSize = f.Select(a => a.size).Sum();

            Assert.AreEqual(2, await _store.GetBlobsCount(), "Не загружено 2 файла");
            Assert.AreEqual(totalSize, await _store.GetTotalSize(), "Не загружено 2 файла");
            Assert.AreEqual(0, await _store.GetUncompletedBlobsCount(), "Найдены незавершённые файлы в списке обычных");
            Assert.AreEqual(0, await _store.GetUncompletedTotalSize(), "Найдены незавершённые файлы в списке обычных");
            CollectionAssert.AreEquivalent(new Guid[0], await _store.FindUncompletedBlobs(), "Найдены незавершённые файлы в списке обычных");

            Assert.IsTrue(await _store.MarkUncompleted(f[0].id), "Не удалось пометить как незавершённый файл");
            Assert.IsTrue(await _store.MarkUncompleted(f[1].id), "Не удалось пометить как незавершённый файл");

            Assert.AreEqual(0, await _store.GetBlobsCount(), "Найдены незавершённые файлы в списке обычных");
            Assert.AreEqual(0, await _store.GetTotalSize(), "Найдены незавершённые файлы в списке обычных");
            Assert.AreEqual(2, await _store.GetUncompletedBlobsCount(), "Не найдены незавершённые файлы");
            Assert.AreEqual(totalSize, await _store.GetUncompletedTotalSize(), "Не найдены незавершённые файлы");
            CollectionAssert.AreEquivalent(ids, await _store.FindUncompletedBlobs(), "Не найдены незавершённые файлы");

            Assert.AreEqual(0, await _store.GetCategoryBlobsCount("test"), "Найдены незавершённые файлы в категории");
            Assert.AreEqual(0, await _store.GetCategorySize("test"), "Найдены незавершённые файлы в категории");
            Assert.AreEqual(0, (await _store.ReadCategory("test")).Length, "Найдены незавершённые файлы в категории");

            Assert.AreEqual(0, await _store.GetReferencedBlobsCount(rid), "Найдены незавершённые файлы по ссылке");
            Assert.AreEqual(0, await _store.GetReferencedSize(rid), "Найдены незавершённые файлы по ссылке");
            Assert.AreEqual(0, (await _store.ReadReferencedBlobs(rid)).Length, "Найдены незавершённые файлы по ссылке");

            Assert.IsNull(await _store.FindBlob(f[0].tempFile.Name), "Незавершённый файл найден по имени");
            Assert.IsNull(await _store.FindBlob(f[1].tempFile.Name), "Незавершённый файл найден по имени");

            Assert.IsNull(await _store.GetBlobInfo(f[0].id), "Получена информация по незавершённому файлу при обычном поиске");
            Assert.IsNull(await _store.GetBlobInfo(f[1].id), "Получена информация по незавершённому файлу при обычном поиске");

            await _store.DeleteAllUncompletedBlobs();

            Assert.AreEqual(0, await _store.GetUncompletedBlobsCount(), "Незавершённые файлы не удалены");
            Assert.AreEqual(0, await _store.GetUncompletedTotalSize(), "Незавершённые файлы не удалены");
            CollectionAssert.AreEquivalent(new Guid[0], await _store.FindUncompletedBlobs(), "Незавершённые файлы не удалены");
        }

        private async Task<(StorageFile tempFile, BlobId id, long size)> UploadSmallTempFile(string category = null, Guid? referenceId = null)
        {
            var tmpId = Guid.NewGuid();
            var tmpFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(tmpId.ToString());
            long size;
            using (var str = await tmpFile.OpenStreamForWriteAsync())
            {
                using (var wr = new BinaryWriter(str))
                {
                    for (int i = 0; i < 1024; i++)
                    {
                        wr.Write(i);
                    }
                    wr.Flush();
                    size = str.Length;
                    str.Position = 0;
                }
            }
            BlobId id;
            using (var b = await tmpFile.OpenStreamForReadAsync())
            {
                id = await _store.SaveBlob(new InputBlob()
                {
                    BlobStream = b,
                    Category = category,
                    UniqueName = tmpFile.Name,
                    ReferenceId = referenceId
                }, CancellationToken.None);
            }
            return (tmpFile, id, size);
        }

        [TestMethod]
        public async Task BlobStoreUploadBenchmark()
        {
            long size;
            Stopwatch sw;
            List<BlobId> id;
            using (var str = new MemoryStream())
            {
                var wr = new BinaryWriter(str);
                for (int i = 0; i < 1024 * 1024 * 2; i++)
                {
                    wr.Write(i);
                }
                wr.Flush();
                size = str.Length;
                str.Position = 0;
                id = new List<BlobId>();
                sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < 5; i++)
                {
                    str.Position = 0;
                    id.Add(await _store.SaveBlob(new InputBlob()
                    {
                        BlobStream = str,
                        Category = "test",
                        UniqueName = Guid.NewGuid().ToString()
                    }, CancellationToken.None));
                }
                sw.Stop();
                var sizeMb = (float) size / (1024 * 1024);
                var sec = sw.Elapsed.TotalSeconds / id.Count;
                sw = new Stopwatch();
                sw.Start();
                Logger.LogMessage($"Время сохранения файла размером {id.Count}x{sizeMb:F2} Мб: {sec} сек, {sizeMb / sec} Мб/Сек");
                foreach (var bid in id)
                {
                    byte[] buf = new byte[64 * 1024];
                    using (var str2 = await _store.LoadBlob(bid))
                    {
                        while (await (str2.ReadAsync(buf, 0, buf.Length)) == buf.Length)
                        {
                            // do nothing
                        }
                    }
                }
                sw.Stop();
                sec = sw.Elapsed.TotalSeconds / id.Count;
                Logger.LogMessage($"Время чтения файла размером {id.Count}x{sizeMb:F2} Мб: {sec} сек, {sizeMb / sec} Мб/Сек");
                sw = new Stopwatch();
                sw.Start();
                for (var i = 0; i < 5; i++)
                {
                    str.Position = 0;
                    using (var str2 =
                        await (await ApplicationData.Current.TemporaryFolder.CreateFileAsync(Guid.NewGuid().ToString()))
                            .OpenStreamForWriteAsync())
                    {
                        byte[] buf = new byte[64 * 1024];
                        int sz;
                        while ((sz = await (str.ReadAsync(buf, 0, buf.Length))) == buf.Length)
                        {
                            await str2.WriteAsync(buf, 0, sz);
                        }
                    }
                }
                sw.Stop();
                sec = sw.Elapsed.TotalSeconds / id.Count;
                Logger.LogMessage($"Время записи файла (файловая система) размером {id.Count}x{sizeMb:F2} Мб: {sec} сек, {sizeMb / sec} Мб/Сек");
            }
        }

        [TestMethod]
        public async Task BlobStoreUploadBenchmarkParallel()
        {
            long size;
            Stopwatch sw;
            List<BlobId> id;
            using (var str = new MemoryStream())
            {
                var wr = new BinaryWriter(str);
                for (int i = 0; i < 1024 * 1024 * 2; i++)
                {
                    wr.Write(i);
                }
                wr.Flush();
                byte[] dataBuf = str.ToArray();
                size = str.Length;
                str.Position = 0;
                id = new List<BlobId>();
                sw = new Stopwatch();
                sw.Start();
                var l = new object();
                await Task.Factory.StartNew(() =>
                {
                    Parallel.ForEach(Enumerable.Range(0, 5), new ParallelOptions() { MaxDegreeOfParallelism = 5 }, i =>
                    {
                        using (var str2 = new MemoryStream(dataBuf))
                        {
                            var id1 = _store.SaveBlob(new InputBlob()
                            {
                                BlobStream = str2,
                                Category = "test",
                                UniqueName = Guid.NewGuid().ToString()
                            }, CancellationToken.None).Result;
                            lock (l)
                            {
                                id.Add(id1);
                            }
                        }
                    });
                });
                sw.Stop();
                var sizeMb = (float)size / (1024 * 1024);
                var sec = sw.Elapsed.TotalSeconds / id.Count;
                sw = new Stopwatch();
                sw.Start();
                Logger.LogMessage($"Время сохранения файла размером {id.Count}x{sizeMb:F2} Мб: {sec} сек, {sizeMb / sec} Мб/Сек");
            }
        }

        private async Task<byte[]> GetStreamHash(Stream str)
        {
            var prov = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            var hash = prov.CreateHash();
            var buffer = new byte[65536];
            do
            {
                var sz = await str.ReadAsync(buffer, 0, buffer.Length);
                if (sz == 0)
                {
                    break;
                }
                if (sz != 65536)
                {
                    var b = new byte[sz];
                    Array.Copy(buffer, b, sz);
                    hash.Append(CryptographicBuffer.CreateFromByteArray(b));
                }
                else
                {
                    hash.Append(CryptographicBuffer.CreateFromByteArray(buffer));
                }
            } while (true);
            return hash.GetValueAndReset().ToArray();
        }
    }

    /// <summary>
    /// Поток, который кидает ошибку при обращении к методам.
    /// </summary>
    public class FakeErrorStream : Stream
    {
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => 0;

        public override long Position
        {
            get => 0;
            set => throw new NotSupportedException();
        }
    }
}