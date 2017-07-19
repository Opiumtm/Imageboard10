using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Коллекция тредов.
    /// </summary>
    internal class PostModelStoreThreadCollection : PostModelStoreBareEntity, IBoardPageThreadCollection, IPostModelStoreChildrenLoadStageInfo
    {
        public IBoardPostCollectionInfoSet Info { get; set; }

        public string Etag { get; set; }

        public IList<IThreadPreviewPostCollection> Threads { get; } = new List<IThreadPreviewPostCollection>();

        public byte Stage { get; set; }
    }
}