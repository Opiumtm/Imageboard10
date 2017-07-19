using System;
using System.Collections.Generic;
using Windows.UI;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Полный пост.
    /// </summary>
    internal class PostModelStorePost : PostModelStorePostLight, IBoardPost
    {
        public IPostDocument Comment { get; set; }

        public IList<ILink> Quotes { get; set; }

        public IList<IPostMedia> MediaFiles { get; set; }

        public string Hash { get; set; }

        public string Email { get; set; }

        public IPosterInfo Poster { get; set; }

        public DateTimeOffset LoadedTime { get; set; }

        public IBoardPostIcon Icon { get; set; }

        public IBoardPostCountryFlag Country { get; set; }

        public string UniqueId { get; set; }

        internal class PostIcon : IBoardPostIcon
        {
            public ILink ImageLink { get; set; }

            public string Description { get; set; }
        }

        internal class PosterInfo : IPosterInfo
        {
            public string Name { get; set; }

            public string Tripcode { get; set; }

            public string NameColorStr { get; set; }

            public Color? NameColor { get; set; }
        }

        internal class CountryFlag : IBoardPostCountryFlag
        {
            public ILink ImageLink { get; set; }
        }
    }
}