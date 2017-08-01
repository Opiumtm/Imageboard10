using System;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Информация о лайках.
    /// </summary>
    public interface IBoardPostLikesStoreInfo
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        PostStoreEntityId Id { get; }

        /// <summary>
        /// Лайки.
        /// </summary>
        IBoardPostLikes Likes { get; }
    }
}