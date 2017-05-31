using System;
using System.Runtime.Serialization;

namespace Imageboard10.Core.Models.Posts.PostMedia
{
    /// <summary>
    /// Базовый класс медиа в посте.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    [KnownType(typeof(PostMedia))]
    [KnownType(typeof(PostMediaWithSize))]
    [KnownType(typeof(PostMediaWithThumbnail))]
    [KnownType(typeof(PostMediaExternalContract))]
    public abstract class PostMediaBase
    {
        /// <summary>
        /// Получить тип для сериализатора.
        /// </summary>
        /// <returns>Тип для сериализатора.</returns>
        public Type GetTypeForSerializer() => GetType();
    }
}