using System.Runtime.Serialization;
using Windows.Graphics;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.PostMedia
{
    /// <summary>
    /// ����� � ��������.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostMediaWithSize : PostMedia, IPostMediaWithSize
    {
        /// <summary>
        /// ������.
        /// </summary>
        [IgnoreDataMember]
        public SizeOfInt32 Size
        {
            get => new SizeOfInt32() {Height = Height, Width = Width};
            set
            {
                Height = value.Height;
                Width = value.Width;
            }
        }

        /// <summary>
        /// ������.
        /// </summary>
        [DataMember]
        public int Height { get; set; }

        /// <summary>
        /// ������.
        /// </summary>
        [DataMember]
        public int Width { get; set; }
    }
}