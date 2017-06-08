using System.Collections.Generic;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Models.Posts.PostNodes;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Стандартные сериализаторы медиа в постах.
    /// </summary>
    public sealed class StandardPostAttributeSerializers : ObjectSerializersProviderBase
    {
        /// <summary>
        /// Создать сериализаторы.
        /// </summary>
        /// <returns>Сериализаторы.</returns>
        protected override IEnumerable<IObjectSerializer> CreateSerializers()
        {
            yield return new StandardObjectSerializer<PostBasicAttribute, PostAttributeBase>("postattr:std:basic");
            yield return new StandardObjectSerializer<PostLinkAttribute, PostAttributeBase>(new PostLinkAttributeCustomization<PostLinkAttribute>(), "postattr:std:link");
            yield return new ExternContractObjectSerializer<PostAttributeExternalContract, PostAttributeBase>("postattr:std:extern");
        }
    }
}