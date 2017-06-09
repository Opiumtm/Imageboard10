using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.PostNodes
{
    /// <summary>
    /// Ссылка на объект доски.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class BoardLinkPostNode : PostNodeBase, IBoardLinkPostNode
    {
        /// <summary>
        /// Ссылка.
        /// </summary>
        [IgnoreDataMember]
        public ILink BoardLink { get; set; }

        /// <summary>
        /// Контракт ссылки. Только для сериализации.
        /// </summary>
        [DataMember]
        public string BoardLinkContract { get; set; }
    }
}