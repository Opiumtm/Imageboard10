using System.Runtime.Serialization;
using Windows.Graphics;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.PostMedia
{
    /// <summary>
    /// Медиа с размером.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostMediaWithSize : PostMedia, IPostMediaWithSize
    {
        /// <summary>
        /// Размер.
        /// </summary>
        [IgnoreDataMember]
        public SizeInt32 Size
        {
            get => new SizeInt32() {Height = Height, Width = Width};
            set
            {
                Height = value.Height;
                Width = value.Width;
            }
        }

        /// <summary>
        /// Высота.
        /// </summary>
        [DataMember]
        public int Height { get; set; }

        /// <summary>
        /// Ширина.
        /// </summary>
        [DataMember]
        public int Width { get; set; }
    }
}