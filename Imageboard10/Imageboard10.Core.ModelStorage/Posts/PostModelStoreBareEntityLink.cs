using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Базовая информация о сущности (только ссыкла).
    /// </summary>
    internal class PostModelStoreBareEntityLink : IBoardPostEntity
    {
        public PostStoreEntityType EntityType { get; set; }

        public PostStoreEntityId? StoreId { get; set; }

        public PostStoreEntityId? StoreParentId { get; set; }

        public ILink Link { get; set; }

        public ILink ParentLink { get; set; }

        public string Subject => null;

        public IPostMediaWithSize Thumbnail => null;
    }
}