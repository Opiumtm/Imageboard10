using System;
using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.PostNodes
{
    /// <summary>
    /// Базовая нода поста.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    [KnownType(typeof(CompositePostNode))]
    [KnownType(typeof(TextPostNode))]
    [KnownType(typeof(LineBreakPostNode))]
    [KnownType(typeof(BoardLinkPostNode))]
    [KnownType(typeof(PostNodeExternalContract))]
    public abstract class PostNodeBase : IPostNode
    {
        /// <summary>
        /// Получить тип для сериализации.
        /// </summary>
        /// <returns>Тип для сериализации.</returns>
        public Type GetTypeForSerializer() => GetType();
    }
}