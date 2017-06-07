using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Metadata;

namespace Imageboard10.Core.ModelInterface
{
    /// <summary>
    /// Сериализатор объектов.
    /// </summary>
    public interface IObjectSerializer
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
        /// Действия до сериализации. Для выполнения сериализации внешними средствами.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Исправленный объект (если нужно).</returns>
        ISerializableObject BeforeSerialize(ISerializableObject obj);


        /// <summary>
        /// Действия после десериализации. Для выполнения сериализации внешними средствами.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Исправленный объект (если нужно).</returns>
        ISerializableObject AfterDeserialize(ISerializableObject obj);
    }
}