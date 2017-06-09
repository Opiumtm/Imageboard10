using System.Collections.Generic;
using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.PostNodes
{
    /// <summary>
    /// Композитный узел.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class CompositePostNode : PostNodeBase, ICompositePostNode
    {
        /// <summary>
        /// Атрибут.
        /// </summary>
        [IgnoreDataMember]
        public IPostAttribute Attribute { get; set; }

        /// <summary>
        /// Дочерние узлы.
        /// </summary>
        [IgnoreDataMember]
        public IList<IPostNode> Children { get; set; }

        /// <summary>
        /// Дочерние узлы. Только для сериализации.
        /// </summary>
        [DataMember]
        public List<PostNodeBase> ChildrenContracts { get; set; }

        /// <summary>
        /// Атрибут. Только для сериализации.
        /// </summary>
        [DataMember]
        public PostAttributeBase AttributeContract { get; set; }
    }
}