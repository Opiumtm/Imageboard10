using System;
using System.Collections.Generic;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// Сервис сериализации (общая реализация).
    /// </summary>
    public sealed class ObjectSerializationService : ModuleBase<IObjectSerializationService>, IObjectSerializationService
    {
        private readonly Dictionary<Type, IObjectSerializer> _typeCache = new Dictionary<Type, IObjectSerializer>();
        private readonly Dictionary<string, IObjectSerializer> _idCache = new Dictionary<string, IObjectSerializer>();

        private IObjectSerializer GetSerializer(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            lock (_typeCache)
            {
                if (!_typeCache.ContainsKey(type))
                {
                    _typeCache[type] = ModuleProvider.QueryModule<IObjectSerializer, Type>(type)
                                       ?? throw new ModuleNotFoundException($"Не найдена логика сериализации типа {type.FullName}");
                }
                return _typeCache[type];
            }
        }

        private IObjectSerializer GetSerializer(string typeId)
        {
            if (typeId == null) throw new ArgumentNullException(nameof(typeId));
            lock (_idCache)
            {
                if (!_idCache.ContainsKey(typeId))
                {
                    _idCache[typeId] = ModuleProvider.QueryModule<IObjectSerializer, string>(typeId)
                                     ?? throw new ModuleNotFoundException($"Не найдена логика сериализации типа TypeId=\"{typeId}\"");
                }
                return _idCache[typeId];
            }
        }

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Сериализованный объект.</returns>
        public string SerializeToString(ISerializableObject obj)
        {
            if (obj == null)
            {
                return null;
            }
            var serializer = GetSerializer(obj.GetTypeForSerializer());
            return SerializationImplHelper.WithTypeId(serializer.SerializeToString(obj), serializer.TypeId);
        }

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Сериализованное медиа.</returns>
        public byte[] SerializeToBytes(ISerializableObject obj)
        {
            if (obj == null)
            {
                return null;
            }
            var serializer = GetSerializer(obj.GetTypeForSerializer());
            return SerializationImplHelper.WithTypeId(serializer.SerializeToBytes(obj), serializer.TypeId);
        }

        /// <summary>
        /// Десериализовать.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Объект.</returns>
        public ISerializableObject Deserialize(string data)
        {
            if (data == null)
            {
                return null;
            }
            (var sdata, var typeId) = SerializationImplHelper.ExtractTypeId(data);
            var serializer = GetSerializer(typeId);
            return serializer.Deserialize(sdata);
        }

        /// <summary>
        /// Десериализовать.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Объект.</returns>
        public ISerializableObject Deserialize(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            (var sdata, var typeId) = SerializationImplHelper.ExtractTypeId(data);
            var serializer = GetSerializer(typeId);
            return serializer.Deserialize(sdata);
        }

        /// <summary>
        /// Найти сериализатор.
        /// </summary>
        /// <param name="type">Тип.</param>
        /// <returns>Сериализатор.</returns>
        public IObjectSerializer FindSerializer(Type type) => GetSerializer(type);

        /// <summary>
        /// Найти сериализатор.
        /// </summary>
        /// <param name="typeId">Идентификатор типа.</param>
        /// <returns>Сериализатор.</returns>
        public IObjectSerializer FindSerializer(string typeId) => GetSerializer(typeId);
    }
}