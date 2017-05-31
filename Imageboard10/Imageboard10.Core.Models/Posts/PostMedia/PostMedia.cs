using System;
using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.PostMedia
{
    /// <summary>
    /// Медиа в посте.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostMedia : PostMediaBase, IPostMedia
    {
        /// <summary>
        /// Ссылка на медиа.
        /// </summary>
        [IgnoreDataMember]
        public ILink MediaLink { get; set; }

        /// <summary>
        /// Строка со ссылкой на медиа. Только для сериализации.
        /// </summary>
        [DataMember]
        public string MediaLinkContract { get; set; }

        /// <summary>
        /// Тип медиафайла.
        /// </summary>
        [DataMember]
        public Guid MediaType { get; set; }

        /// <summary>
        /// Размер файла.
        /// </summary>
        [DataMember]
        public ulong? FileSize { get; set; }
    }
}