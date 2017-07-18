using System;
using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Облегчённая версия поста.
    /// </summary>
    internal class PostModelStorePostLight : PostModelStoreBareEntity, IBoardPostLight
    {
        public DateTimeOffset Date { get; set; }

        public int Counter { get; set; }

        public string BoardSpecificDate { get; set; }

        public IList<Guid> Flags { get; set; }

        public IBoardPostTags Tags { get; set; }

        public IBoardPostLikes Likes { get; set; }
    }
}