using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;

namespace Imageboard10.Core.ModelStorage.Posts
{
    internal class PostModelStoreBareEntity : IBoardPostEntity
    {
        public PostStoreEntityType EntityType { get; set; }

        public PostStoreEntityId? StoreId { get; set; }

        public PostStoreEntityId? StoreParentId { get; set; }

        public ILink Link { get; set; }

        public ILink ParentLink { get; set; }

        public string Subject { get; set; }

        public IPostMediaWithSize Thumbnail { get; set; }
    }
}