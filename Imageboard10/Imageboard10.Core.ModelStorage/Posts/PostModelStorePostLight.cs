using System;
using System.Collections.Generic;
using System.Linq;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Облегчённая версия поста.
    /// </summary>
    internal class PostModelStorePostLight : PostModelStoreBareEntity, IBoardPostLight, IBoardPostLikes, IBoardPostTags
    {
        public DateTimeOffset Date { get; set; }

        public int Counter { get; set; }

        public string BoardSpecificDate { get; set; }

        public IList<Guid> Flags { get; set; }

        public string[] TagsSet { get; set; }

        public IBoardPostTags Tags => TagsSet?.Length > 0 ? this : null;

        public IBoardPostLikes Likes => LLikes != null || LDislikes != null ? this : null;

        public int? LDislikes { get; set; }

        public int? LLikes { get; set; }

        int IBoardPostLikes.Dislikes => LDislikes ?? 0;

        int IBoardPostLikes.Likes => LLikes ?? 0;

        string IBoardPostTags.TagStr => TagsSet?.FirstOrDefault();

        IList<string> IBoardPostTags.Tags => TagsSet;
    }

    internal class PostModelStorePostLightWithSequence : PostModelStorePostLight, IBoardPostEntityWithSequence2
    {
        public int OnPageSequence { get; private set; }

        public void SetOnPageSequence(int seq)
        {
            OnPageSequence = seq;
        }
    }
}