using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Posts.PostNodes
{
    /// <summary>
    /// Базовая нода поста.
    /// </summary>
    public abstract class PostNodeBase : IPostNode, IDeepCloneable<PostNodeBase>
    {
        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <param name="modules">Модули.</param>
        /// <returns>Клон.</returns>
        public abstract PostNodeBase DeepClone(IModuleProvider modules);
    }
}