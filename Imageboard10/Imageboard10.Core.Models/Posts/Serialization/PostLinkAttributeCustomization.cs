using Imageboard10.Core.Models.Posts.PostNodes;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Кастомизация сериализации объекта <see cref="PostLinkAttribute"/>.
    /// </summary>
    public class PostLinkAttributeCustomization<T> : ObjectSerializerCustomization<T>
        where T : PostLinkAttribute, new()
    {
        /// <summary>
        /// Проверить контракт перед сериализацией.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override T ValidateContract(T obj)
        {
            obj = base.ValidateContract(obj);
            if (obj != null)
            {
                obj.LinkContract = obj.Link != null ? LinkSerializationService.Serialize(obj.Link) : null;
            }
            return obj;
        }

        /// <summary>
        /// Проверить контракт после сериализации.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override T ValidateAfterDeserialize(T obj)
        {
            obj = base.ValidateAfterDeserialize(obj);
            if (obj != null)
            {
                obj.Link = obj.LinkContract != null ? LinkSerializationService.Deserialize(obj.LinkContract) : null;
                obj.LinkContract = null;
            }
            return obj;
        }
    }
}