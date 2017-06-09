using System.Linq;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Posts.PostNodes;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Кастомизация сериализации <see cref="CompositePostNode"/>
    /// </summary>
    public class CompositePostNodeSerializerCustomization : ObjectSerializerCustomization<CompositePostNode>
    {
        /// <summary>
        /// Проверить контракт перед сериализацией.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override CompositePostNode ValidateContract(CompositePostNode obj)
        {
            PostNodeBase ValidateNode(IPostNode node)
            {
                return ModuleProvider.ValidateBeforeSerialize<IPostNode, PostNodeBase, PostNodeExternalContract>(node);
            }

            obj = base.ValidateContract(obj);
            if (obj != null)
            {
                obj.ChildrenContracts = obj.Children?.Select(ValidateNode)?.ToList();
                obj.AttributeContract = ModuleProvider.ValidateBeforeSerialize<IPostAttribute, PostAttributeBase, PostAttributeExternalContract>(obj.Attribute);
            }
            return obj;
        }

        /// <summary>
        /// Проверить контракт после сериализации.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override CompositePostNode ValidateAfterDeserialize(CompositePostNode obj)
        {
            IPostNode ValidateNode(PostNodeBase node)
            {
                return ModuleProvider.ValidateAfterDeserialize<PostNodeBase, IPostNode, PostNodeExternalContract>(node);
            }

            obj = base.ValidateAfterDeserialize(obj);
            if (obj != null)
            {
                obj.Attribute = ModuleProvider.ValidateAfterDeserialize<PostAttributeBase, IPostAttribute, PostAttributeExternalContract>(obj.AttributeContract);
                obj.AttributeContract = null;
                obj.Children = obj.ChildrenContracts?.Select(ValidateNode)?.ToList();
                obj.ChildrenContracts = null;
            }
            return obj;
        }
    }
}