using System.Collections.Generic;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Models.Posts.PostNodes;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Стандартные сериализаторы узлов постов.
    /// </summary>
    public sealed class StandardPostNodeSerializers : ObjectSerializersProviderBase
    {
        /// <summary>
        /// Создать сериализаторы.
        /// </summary>
        /// <returns>Сериализаторы.</returns>
        protected override IEnumerable<IObjectSerializer> CreateSerializers()
        {
            yield return new StandardObjectSerializer<TextPostNode, PostNodeBase>("postnode:std:text");
            yield return new StandardObjectSerializer<CompositePostNode, PostNodeBase>(new CompositePostNodeSerializerCustomization(), "postnode:std:composite");
            yield return new StandardObjectSerializer<LineBreakPostNode, PostNodeBase>("postnode:std:br");
            yield return new StandardObjectSerializer<BoardLinkPostNode, PostNodeBase>(new BoardLinkPostNodeSerializerCustomization(), "postnode:std:b/link");
            yield return new ExternContractObjectSerializer<PostNodeExternalContract, PostNodeBase>("postnode:std:extern");
        }
    }
}