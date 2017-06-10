using System.Linq;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Posts.PostNodes;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Кастомизация сериализации для <see cref="PostDocument"/>.
    /// </summary>
    public class PostDocumentSerializerCustomization : ObjectSerializerCustomization<PostDocument>
    {
        /// <summary>
        /// Проверить контракт перед сериализацией.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override PostDocument ValidateContract(PostDocument obj)
        {
            PostNodeBase Validate(IPostNode n)
            {
                return ModuleProvider.ValidateBeforeSerialize<IPostNode, PostNodeBase, PostNodeExternalContract>(n);
            }

            obj = base.ValidateContract(obj);
            if (obj != null)
            {
                obj.NodesContract = obj.Nodes?.Select(Validate)?.ToList();
            }
            return obj;
        }

        /// <summary>
        /// Проверить контракт после сериализации.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override PostDocument ValidateAfterDeserialize(PostDocument obj)
        {
            IPostNode Validate(PostNodeBase n)
            {
                return ModuleProvider.ValidateAfterDeserialize<PostNodeBase, IPostNode, PostNodeExternalContract>(n);
            }

            obj = base.ValidateAfterDeserialize(obj);
            if (obj != null)
            {
                obj.Nodes = obj.NodesContract?.Select(Validate)?.ToList();
                obj.NodesContract = null;
            }
            return obj;
        }
    }
}