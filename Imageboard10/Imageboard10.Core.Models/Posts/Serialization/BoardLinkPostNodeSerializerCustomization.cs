using Imageboard10.Core.Models.Posts.PostNodes;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Кастомизация сериализации <see cref="BoardLinkPostNode"/>
    /// </summary>
    public class BoardLinkPostNodeSerializerCustomization : ObjectSerializerCustomization<BoardLinkPostNode>
    {
        /// <summary>
        /// Проверить контракт перед сериализацией.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override BoardLinkPostNode ValidateContract(BoardLinkPostNode obj)
        {
            obj = base.ValidateContract(obj);
            if (obj != null)
            {
                obj.BoardLinkContract = LinkSerializationService.Serialize(obj.BoardLink);
            }
            return obj;
        }

        /// <summary>
        /// Проверить контракт после сериализации.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override BoardLinkPostNode ValidateAfterDeserialize(BoardLinkPostNode obj)
        {
            obj = base.ValidateAfterDeserialize(obj);
            if (obj != null)
            {
                obj.BoardLink = LinkSerializationService.Deserialize(obj.BoardLinkContract);
            }
            return obj;
        }
    }
}