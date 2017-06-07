using System.Threading.Tasks;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// Дополнительные настройки сериализации.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    public interface IObjectSerializerCustomization<T>
        where T : class, ISerializableObject, new()
    {
        /// <summary>
        /// Проверить контракт перед сериализацией.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        T ValidateContract(T obj);

        /// <summary>
        /// Проверить контракт после сериализации.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        T ValidateAfterDeserialize(T obj);

        /// <summary>
        /// Инициализировать объект.
        /// </summary>
        /// <param name="modules">Модули.</param>
        ValueTask<Nothing> Initialize(IModuleProvider modules);
    }
}