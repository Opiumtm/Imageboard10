using System;
using System.IO;
using System.Text;
using System.Xml;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Utility;

namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// Сериализатор объекта.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    /// <typeparam name="TBase">Базовый класс контракта.</typeparam>
    /// <typeparam name="TExtern">Внешний контракт.</typeparam>
    public abstract class ObjectSerializerBase<T, TBase, TExtern> : ModuleBase<IObjectSerializer>, IObjectSerializer
        where TBase : class, ISerializableObject, new()
        where T : class, TBase, new()
        where TExtern: class , TBase, IExternalContractHost, new()
    {
        /// <summary>
        /// Идентификатор типа.
        /// </summary>
        public abstract string TypeId { get; }

        /// <summary>
        /// Тип.
        /// </summary>
        public Type Type => typeof(T);

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Сериализованный объект.</returns>
        public string SerializeToString(ISerializableObject obj)
        {
            return SerializeToString(obj as T);
        }

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Сериализованный объект.</returns>
        private string SerializeToString(T obj)
        {
            if (obj == null)
            {
                return null;
            }
            var serializer = DataContractSerializerCache.GetJsonSerializer<TBase>();
            using (var str = new MemoryStream())
            {
                serializer.WriteObject(str, ValidateContract(obj));
                return Encoding.UTF8.GetString(str.ToArray());
            }
        }

        /// <summary>
        /// Проверить контракт перед сериализацией.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        protected virtual T ValidateContract(T obj)
        {
            return obj;
        }

        /// <summary>
        /// Проверить контракт после сериализации.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        protected virtual T ValidateAfterDeserialize(T obj)
        {
            return obj;
        }

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Сериализованное медиа.</returns>
        public byte[] SerializeToBytes(ISerializableObject obj)
        {
            return SerializeToBytes(obj as T);
        }

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Сериализованное медиа.</returns>
        private byte[] SerializeToBytes(T obj)
        {
            if (obj == null)
            {
                return null;
            }
            var serializer = DataContractSerializerCache.GetSerializer<TBase>();
            using (var str = new MemoryStream())
            {
                using (var wr = XmlDictionaryWriter.CreateBinaryWriter(str))
                {
                    serializer.WriteObject(wr, ValidateContract(obj));
                    wr.Flush();
                }
                return str.ToArray();
            }
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
            var serializer = DataContractSerializerCache.GetJsonSerializer<TBase>();
            using (var str = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                var r = serializer.ReadObject(str) as T;
                return r != null ? ValidateAfterDeserialize(r) : null;
            }
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
            var serializer = DataContractSerializerCache.GetSerializer<TBase>();
            using (var str = new MemoryStream(data))
            {
                using (var rd = XmlDictionaryReader.CreateBinaryReader(str, XmlDictionaryReaderQuotas.Max))
                {
                    var r = serializer.ReadObject(rd) as T;
                    return r != null ? ValidateAfterDeserialize(r) : null;
                }
            }
        }

        /// <summary>
        /// Действия до сериализации. Для выполнения сериализации внешними средствами.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Исправленный объект (если нужно).</returns>
        public ISerializableObject BeforeSerialize(ISerializableObject obj)
        {
            var r = obj as T;
            return r != null ? ValidateContract(r) : null;
        }

        /// <summary>
        /// Действия после десериализации. Для выполнения сериализации внешними средствами.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Исправленный объект (если нужно).</returns>
        public ISerializableObject AfterDeserialize(ISerializableObject obj)
        {
            var r = obj as T;
            return r != null ? ValidateAfterDeserialize(r) : null;
        }

        /// <summary>
        /// Сериализовать во внешний контракт.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Внешний контракт.</returns>
        protected TExtern SerializeToExternalContract(ISerializableObject obj)
        {
            return ModuleProvider.SerializeToExternalContract<TExtern>(obj);
        }

        /// <summary>
        /// Десериализовать из внешнего контракта.
        /// </summary>
        /// <param name="contract">Контракт.</param>
        /// <returns>Объект.</returns>
        protected ISerializableObject DeserializeExternalContract(TExtern contract)
        {
            return ModuleProvider.DeserializeExternalContract(contract);
        }
    }
}