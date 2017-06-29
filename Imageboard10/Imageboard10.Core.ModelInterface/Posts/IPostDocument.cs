using System;
using System.Collections.Generic;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Документ с текстом поста.
    /// </summary>
    public interface IPostDocument : ISerializableObject
    {
        /// <summary>
        /// Уникальный идентификатор документа.
        /// </summary>
        Guid UniqueId { get; }

        /// <summary>
        /// Узлы.
        /// </summary>
        IList<IPostNode> Nodes { get; }
    }
}