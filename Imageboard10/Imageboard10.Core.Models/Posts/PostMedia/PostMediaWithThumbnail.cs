using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.PostMedia
{
    /// <summary>
    /// Медиа с предварительным просмотром.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostMediaWithThumbnail : PostMediaWithSize, IPostMediaWithThumbnail, IPostMediaWithInfo
    {
        /// <summary>
        /// Изображение предварительного просмотра.
        /// </summary>
        [IgnoreDataMember]
        public IPostMediaWithSize Thumbnail { get; set; }

        /// <summary>
        /// Контракт (только для сериализации).
        /// </summary>
        [DataMember]
        public PostMediaBase ThumbnailContract { get; set; }

        /// <summary>
        /// Имя.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Отображаемое имя.
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// Полное имя.
        /// </summary>
        [DataMember]
        public string FullName { get; set; }

        /// <summary>
        /// Not safe for work.
        /// </summary>
        [DataMember]
        public bool Nsfw { get; set; }

        /// <summary>
        /// Хэш.
        /// </summary>
        [DataMember]
        public string Hash { get; set; }
    }
}