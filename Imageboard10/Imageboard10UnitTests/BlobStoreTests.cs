using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.ModelStorage.Blobs;
using Imageboard10.Core.ModelStorage.Boards;
using Imageboard10.Core.ModelStorage.UnitTests;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Makaba.Network.JsonParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public async Task BlobStoreUpload()
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
                Assert.AreEqual(size, str.Length, "Размер блоба не совпадает с исходным");
            }
        }
    }
}