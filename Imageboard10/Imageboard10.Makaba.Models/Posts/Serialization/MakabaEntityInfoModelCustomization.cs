using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Makaba.Models.Posts.Serialization
{
    /// <summary>
    /// Кастомизация сериализации для типа <see cref="MakabaEntityInfoModel"/>.
    /// </summary>
    public class MakabaEntityInfoModelCustomization : ObjectSerializerCustomization<MakabaEntityInfoModel>
    {
        /// <summary>
        /// Проверить контракт перед сериализацией.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override MakabaEntityInfoModel ValidateContract(MakabaEntityInfoModel obj)
        {
            var res = base.ValidateContract(obj);
            res?.BeforeSerialize(ModuleProvider);
            return res;
        }

        /// <summary>
        /// Проверить контракт после сериализации.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override MakabaEntityInfoModel ValidateAfterDeserialize(MakabaEntityInfoModel obj)
        {
            var res = base.ValidateAfterDeserialize(obj);
            res?.AfterDeserialize(ModuleProvider);
            return res;
        }
    }
}