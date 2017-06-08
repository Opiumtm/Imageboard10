using System;
using System.Threading.Tasks;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// Стандартный сериализатор.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    /// <typeparam name="TBase">Базовый класс контракта.</typeparam>
    public sealed class StandardObjectSerializer<T, TBase> : ObjectSerializerBase<T, TBase>
        where TBase : class, ISerializableObject
        where T : class, TBase, new()
    {
        private readonly IObjectSerializerCustomization<T> _customization;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="customization">Кастомизация.</param>
        /// <param name="typeId">Идентификатор типа.</param>
        public StandardObjectSerializer(IObjectSerializerCustomization<T> customization, string typeId)
        {
            _customization = customization;
            TypeId = typeId ?? throw new ArgumentNullException(nameof(typeId));
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="typeId">Идентификатор типа.</param>
        public StandardObjectSerializer(string typeId)
        {
            _customization = null;
            TypeId = typeId ?? throw new ArgumentNullException(nameof(typeId));
        }

        /// <summary>
        /// Идентификатор типа.
        /// </summary>
        public override string TypeId { get; }

        /// <summary>
        /// Проверить контракт перед сериализацией.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        protected override TBase ValidateContract(T obj)
        {
            if (_customization == null)
            {
                return obj;
            }
            return _customization.ValidateContract(obj);
        }

        /// <summary>
        /// Проверить контракт после сериализации.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        protected override TBase ValidateAfterDeserialize(T obj)
        {
            if (_customization == null)
            {
                return obj;
            }
            return _customization.ValidateAfterDeserialize(obj);
        }

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            if (_customization != null)
            {
                await _customization.Initialize(moduleProvider);
            }
            return Nothing.Value;
        }
    }
}