using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Metadata;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Сериализатор медиа постов.
    /// </summary>
    public interface IPostMediaSerializer
    {
        /// <summary>
        /// Идентификатор типа.
        /// </summary>
        string TypeId { get; }

        /// <summary>
        /// Тип.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="media">Медиа.</param>
        /// <returns>Сериализованное медиа.</returns>
        string SerializeToString(IPostMedia media);

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="media">Медиа.</param>
        /// <returns>Сериализованное медиа.</returns>
        byte[] SerializeToBytes(IPostMedia media);

        /// <summary>
        /// Десериализовать.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Медиа поста.</returns>
        [DefaultOverload]
        IPostMedia Deserialize(string data);

        /// <summary>
        /// Десериализовать.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Медиа поста.</returns>
        IPostMedia Deserialize([ReadOnlyArray] byte[] data);
    }
}