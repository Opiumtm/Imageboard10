using System.Collections.Generic;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Makaba.Models.Posts.Serialization
{
    /// <summary>
    /// Сериализаторы моделей makaba.
    /// </summary>
    public class MakabaModelsSerializers : ObjectSerializersProviderBase
    {
        /// <summary>
        /// Создать сериализаторы.
        /// </summary>
        /// <returns>Сериализаторы.</returns>
        protected override IEnumerable<IObjectSerializer> CreateSerializers()
        {
            yield return new StandardObjectSerializer<MakabaEntityInfoModel, MakabaEntityInfoModel>(new MakabaEntityInfoModelCustomization(), "makaba:entity");
        }
    }
}