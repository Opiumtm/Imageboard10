using System;
using Imageboard10.Core.ModelInterface;

namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// Стандартный сериализатор.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    /// <typeparam name="TBase">Базовый класс контракта.</typeparam>
    public sealed class ExternContractObjectSerializer<T, TBase> : ObjectSerializerBase<T, TBase>
        where TBase : class, ISerializableObject
        where T : class, TBase, IExternalContractHost, new()
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="typeId">Идентификатор типа.</param>
        public ExternContractObjectSerializer(string typeId)
        {
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
            return obj;
        }

        /// <summary>
        /// Проверить контракт после сериализации.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        protected override TBase ValidateAfterDeserialize(T obj)
        {
            return obj;
        }
    }
}