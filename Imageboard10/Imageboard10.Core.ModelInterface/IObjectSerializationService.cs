using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Metadata;

namespace Imageboard10.Core.ModelInterface
{
    /// <summary>
    /// Сервис сериализации (общая реализация).
    /// </summary>
    public interface IObjectSerializationService
    {
        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Сериализованный объект.</returns>
        string SerializeToString(ISerializableObject obj);

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Сериализованное медиа.</returns>
        byte[] SerializeToBytes(ISerializableObject obj);

        /// <summary>
        /// Десериализовать.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Объект.</returns>
        [DefaultOverload]
        ISerializableObject Deserialize(string data);

        /// <summary>
        /// Десериализовать.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Объект.</returns>
        ISerializableObject Deserialize([ReadOnlyArray] byte[] data);

        /// <summary>
        /// Найти сериализатор.
        /// </summary>
        /// <param name="type">Тип.</param>
        /// <returns>Сериализатор.</returns>
        IObjectSerializer FindSerializer(Type type);

        /// <summary>
        /// Найти сериализатор.
        /// </summary>
        /// <param name="typeId">Идентификатор типа.</param>
        /// <returns>Сериализатор.</returns>
        [DefaultOverload]
        IObjectSerializer FindSerializer(string typeId);
    }
}