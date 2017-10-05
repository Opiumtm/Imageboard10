using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Базовая информация о сущности.
    /// </summary>
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

    internal class PostModelStoreBareEntityWithSequence : PostModelStoreBareEntity, IBoardPostEntityWithSequence2
    {
        public int OnPageSequence { get; private set; }

        public void SetOnPageSequence(int seq)
        {
            OnPageSequence = seq;
        }
    }

    internal interface IBoardPostEntityWithSequence2 : IBoardPostEntityWithSequence
    {
        void SetOnPageSequence(int seq);
    }
}