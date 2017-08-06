namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Информация о лайках.
    /// </summary>
    public interface IBoardPostLikesInfo
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        PostStoreEntityId StoreId { get; }

        /// <summary>
        /// Лайки.
        /// </summary>
        IBoardPostLikes Likes { get; }
    }
}