using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Кэш сериализаторов.
    /// </summary>
    public static class DataContractSerializerCache
    {
        private static readonly Dictionary<Type, DataContractSerializer> Serializers = new Dictionary<Type, DataContractSerializer>();

        /// <summary>
        /// Получить сериализатор для типа.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <returns>Сериализатор.</returns>
        public static DataContractSerializer GetSerializer<T>()
        {
            lock (Serializers)
            {
                if (!Serializers.ContainsKey(typeof(T)))
                {
                    Serializers[typeof(T)] = new DataContractSerializer(typeof(T));
                }
                return Serializers[typeof(T)];
            }
        }
    }
}