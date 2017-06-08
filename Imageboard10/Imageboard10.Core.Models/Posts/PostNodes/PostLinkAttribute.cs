using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.PostNodes
{
    /// <summary>
    /// Атрибут ссылки.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostLinkAttribute : PostAttributeBase, IPostLinkAttribute
    {
        /// <summary>
        /// Ссылка.
        /// </summary>
        [IgnoreDataMember]
        public ILink Link { get; set; }

        /// <summary>
        /// Контракт ссылки. Только для сериализации.
        /// </summary>
        [DataMember]
        public string LinkContract { get; set; }
    }
}