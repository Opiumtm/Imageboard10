using System.Threading.Tasks;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// Дополнительные настройки сериализации.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    public class ObjectSerializerCustomization<T> : IObjectSerializerCustomization<T>
        where T : class, ISerializableObject, new()
    {
        /// <summary>
        /// Проверить контракт перед сериализацией.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public virtual T ValidateContract(T obj)
        {
            return obj;
        }

        /// <summary>
        /// Проверить контракт после сериализации.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public virtual T ValidateAfterDeserialize(T obj)
        {
            return obj;
        }

        /// <summary>
        /// Сервис сериализации ссылок.
        /// </summary>
        protected ILinkSerializationService LinkSerializationService { get; private set; }

        /// <summary>
        /// Провайдер модулей.
        /// </summary>
        protected IModuleProvider ModuleProvider { get; private set; }

        /// <summary>
        /// Сервис сериализации.
        /// </summary>
        protected IObjectSerializationService ObjectSerializationService { get; private set; }

        /// <summary>
        /// Инициализировать объект.
        /// </summary>
        /// <param name="modules">Модули.</param>
        public virtual async ValueTask<Nothing> Initialize(IModuleProvider modules)
        {
            ModuleProvider = modules;
            LinkSerializationService = await modules.QueryModuleAsync<ILinkSerializationService>() ?? throw new ModuleNotFoundException(typeof(ILinkSerializationService));
            ObjectSerializationService = await modules.QueryModuleAsync<IObjectSerializationService>() ?? throw new ModuleNotFoundException(typeof(IObjectSerializationService));
            return Nothing.Value;
        }
    }
}