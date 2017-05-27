using System.Collections.Generic;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Композитный узел.
    /// </summary>
    public interface ICompositePostNode : IPostNode
    {
        /// <summary>
        /// Атрибут.
        /// </summary>
        IPostAttribute Attribute { get; }

        /// <summary>
        /// Дочерние узлы.
        /// </summary>
        IList<IPostNode> Children { get; }
    }
}