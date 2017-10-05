using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;
using Imageboard10.Core.ModelStorage.UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Imageboard10UnitTests
{
    [TestClass]
    [TestCategory("ModelStore")]
    public class PostModelStoreBenchmark : PostModelStoreTestsBase
    {
        [TestMethod]
        public async Task SaveThreadToStoreBenhcmark()
        {
            const int iterations = 10;
            var collection = await ReadThread("mobi_thread_2.json");
            (collection.Info.Items.FirstOrDefault(f => f.GetInfoInterfaceTypes().Any(i => i == typeof(IBoardPostCollectionInfoFlags))) as IBoardPostCollectionInfoFlags).Flags.Add(UnitTestStoreFlags.AlwaysInsert);
            foreach (var p in collection.Posts)
            {
                p.Flags.Add(UnitTestStoreFlags.AlwaysInsert);
            }
            var st = new Stopwatch();
            st.Start();
            for (var i = 0; i < iterations; i++)
            {
                await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null);
            }
            st.Stop();
            var count = collection.Posts.Count;
            Logger.LogMessage("Время загрузки треда в базу: {0:F2} сек. всего, {1:F2} мс на итерацию, {2} постов, {3:F2} мс/пост", st.Elapsed.TotalSeconds, st.Elapsed.TotalMilliseconds / iterations, collection.Posts.Count, st.Elapsed.TotalMilliseconds / iterations / collection.Posts.Count);
            var postsSize = await _store.GetTotalSize(PostStoreEntityType.Post);
            var threadsSize = await _store.GetTotalSize(PostStoreEntityType.Thread);
            var totalSize = await _store.GetTotalSize(null);
            Assert.AreEqual(count * iterations, postsSize, "Количество постов");
            Assert.AreEqual(1 * iterations, threadsSize, "Количество тредов");
            Assert.AreEqual((count + 1) * iterations, totalSize, "Общее количество сущностей");
        }

        [TestMethod]
        public async Task SaveThreadToStoreMergeBenhcmark()
        {
            const int iterations = 10;
            var collection = await ReadThread("mobi_thread_2.json");
            await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Replace, null);
            var st = new Stopwatch();
            st.Start();
            for (var i = 0; i < iterations; i++)
            {
                await _store.SaveCollection(collection, BoardPostCollectionUpdateMode.Merge, null);
            }
            st.Stop();
            var count = collection.Posts.Count;
            Logger.LogMessage("Время загрузки треда в базу: {0:F2} сек. всего, {1:F2} мс на итерацию, {2} постов, {3:F2} мс/пост", st.Elapsed.TotalSeconds, st.Elapsed.TotalMilliseconds / iterations, collection.Posts.Count, st.Elapsed.TotalMilliseconds / iterations / collection.Posts.Count);
            var postsSize = await _store.GetTotalSize(PostStoreEntityType.Post);
            var threadsSize = await _store.GetTotalSize(PostStoreEntityType.Thread);
            var totalSize = await _store.GetTotalSize(null);
            Assert.AreEqual(count, postsSize, "Количество постов");
            Assert.AreEqual(1, threadsSize, "Количество тредов");
            Assert.AreEqual(count + 1, totalSize, "Общее количество сущностей");
        }
    }
}