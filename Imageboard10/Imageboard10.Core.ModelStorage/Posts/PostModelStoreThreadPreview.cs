using System;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Превью треда.
    /// </summary>
    internal class PostModelStoreThreadPreview : PostModelStoreCollection, IThreadPreviewPostCollection
    {
        public int? ImageCount { get; set; }

        public int? OmitImages { get; set; }

        public int? Omit { get; set; }

        public int? ReplyCount { get; set; }

        public int OnPageSequence { get; set; }
    }
}