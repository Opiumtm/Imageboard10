using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links.Serialization
{

    /// <summary>
    /// Стандартные сериализаторы ссылок.
    /// </summary>
    public sealed class StandardLinkSerializers : LinkSerializersProviderBase
    {
        /// <summary>
        /// Создать сериализаторы.
        /// </summary>
        /// <returns>Сериализаторы.</returns>
        protected override IEnumerable<ILinkSerializer> CreateSerializers()
        {
            yield return new BoardLinkSerializer();
            yield return new BoardMediLinkSerializer();
            yield return new BoardPageLinkSerializer();
            yield return new CatalogLinkSerializer();
            yield return new EngineMediaLinkSerializer();
            yield return new EngineUriLinkSerializer();
            yield return new MediaLinkSerializer();
            yield return new PostLinkSerializer();
            yield return new RootLinkSerializer();
            yield return new ThreadLinkSerializer();
            yield return new ThreadPartLinkSerializer();
            yield return new ThreadTagLinkSerializer();
            yield return new UriLinkSerializer();
            yield return new YoutubeLinkSerializer();
        }
    }
}