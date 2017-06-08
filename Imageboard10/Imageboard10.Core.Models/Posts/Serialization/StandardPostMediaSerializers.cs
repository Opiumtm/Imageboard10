using System.Collections.Generic;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Models.Posts.PostMedia;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Стандартные сериализаторы медиа в постах.
    /// </summary>
    public sealed class StandardPostMediaSerializers : ObjectSerializersProviderBase
    {
        /// <summary>
        /// Создать сериализаторы.
        /// </summary>
        /// <returns>Сериализаторы.</returns>
        protected override IEnumerable<IObjectSerializer> CreateSerializers()
        {
            yield return new StandardObjectSerializer<PostMedia.PostMedia, PostMediaBase, PostMediaExternalContract>(new PostMediaSerializerCustomization<PostMedia.PostMedia>(), "postmedia:std");
            yield return new StandardObjectSerializer<PostMediaWithSize, PostMediaBase, PostMediaExternalContract>(new PostMediaSerializerCustomization<PostMediaWithSize>(), "postmedia:std:w/size");
            yield return new StandardObjectSerializer<PostMediaWithThumbnail, PostMediaBase, PostMediaExternalContract>(new PostMediaWithThumbnailSerializerCustomization<PostMediaWithThumbnail>(), "postmedia:std:w/thumbnail");
            yield return new ExternContractObjectSerializer<PostMediaExternalContract, PostMediaBase>("postmedia:std:extern");
        }
    }
}