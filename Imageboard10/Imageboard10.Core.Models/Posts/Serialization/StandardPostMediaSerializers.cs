using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Стандартные сериализаторы медиа в постах.
    /// </summary>
    public sealed class StandardPostMediaSerializers : PostMediaSerializersProviderBase
    {
        /// <summary>
        /// Создать сериализаторы.
        /// </summary>
        /// <returns>Сериализаторы.</returns>
        protected override IEnumerable<IPostMediaSerializer> CreateSerializers()
        {
            yield return new PostMediaSerializer();
            yield return new PostMediaWithSizeSerializer();
            yield return new PostMediaWithThumbnailSerializer();
            yield return new PostMediaExternalSerializer();
        }
    }
}