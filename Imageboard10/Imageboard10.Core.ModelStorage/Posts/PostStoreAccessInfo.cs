using System;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.ModelInterface.Posts.Store;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Информация о доступе.
    /// </summary>
    internal class PostStoreAccessLogItem : IBoardPostStoreAccessLogItem
    {
        public Guid? LogEntryId { get; set; }

        public IBoardPostEntity Entity { get; set; }

        public DateTimeOffset? AccessTime { get; set; }
    }

    /// <summary>
    /// Информация о доступе.
    /// </summary>
    internal class PostStoreAccessInfo : PostStoreAccessLogItem, IBoardPostStoreAccessInfo
    {
        public DateTimeOffset? LastUpdate { get; set; }

        public DateTimeOffset? LastDownload { get; set; }

        public int? NumberOfPosts { get; set; }

        public int NumberOfLoadedPosts { get; set; }

        public int? NumberOfReadPosts { get; set; }

        public ILink LastPost { get; set; }

        public ILink LastLoadedPost { get; set; }

        public string Etag { get; set; }

        public bool IsArchived { get; set; }

        public bool IsFavorite { get; set; }
    }
}