using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            var tmpId = Guid.NewGuid();
            var tmpFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(tmpId.ToString());
            long size;
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
            var id2 = await _store.FindBlob(tmpFile.Name);
            Assert.AreEqual(id, id2, "ID найденного файла не совпадает");
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
}