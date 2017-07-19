using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Коллекция постов.
    /// </summary>
    internal class PostModelStoreCollection : PostModelStoreBareEntity, IBoardPostCollection, IBoardPostCollectionEtagEnabled, IPostModelStoreChildrenLoadStageInfo
    {
        public IBoardPostCollectionInfoSet Info { get; set; }

        public IList<IBoardPost> Posts { get; } = new List<IBoardPost>();

        public string Etag { get; set; }

        public byte Stage { get; set; }
    }
}