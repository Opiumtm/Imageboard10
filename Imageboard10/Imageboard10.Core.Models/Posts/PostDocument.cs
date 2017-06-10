using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Posts.PostNodes;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Документ поста.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostDocument : IPostDocument
    {
        /// <summary>
        /// Получить тип для сериализации.
        /// </summary>
        /// <returns>Тип для сериализации.</returns>
        public Type GetTypeForSerializer() => GetType();

        /// <summary>
        /// Узлы.
        /// </summary>
        [IgnoreDataMember]
        public IList<IPostNode> Nodes { get; set; }

        /// <summary>
        /// Узлы. Только для сериализации.
        /// </summary>
        [DataMember]
        public List<PostNodeBase> NodesContract { get; set; }
    }
}