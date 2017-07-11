using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Utility;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Базовый класс сериализации ссылок.
    /// </summary>
    /// <typeparam name="T">Тип ссылки.</typeparam>
    /// <typeparam name="TJson">Тип JSON-объекта.</typeparam>
    public abstract class LinkSerializerBase<T, TJson> : ModuleBase<ILinkSerializer>, ILinkSerializer
        where T : class, ILink, new()
        where TJson : class , new()
    {
        /// <summary>
        /// Идентификатор типа ссылки.
        /// </summary>
        public abstract string LinkTypeId { get; }

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public Type LinkType => typeof(T);

        /// <summary>
        /// Сериализовать ссылку.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Ссылка в виде строки.</returns>
        public string Serialize(ILink link)
        {
            if (link == null)
            {
                return null;
            }
            if (link is T l)
            {
                return SerializeToString(GetJsonObject(l));
            }
            throw new ArgumentException($"Неправильный тип ссылки для сериализации {link.GetType().FullName}");
        }

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns>Сериализованный объект.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string SerializeToString(TJson obj)
        {
            if (obj == null)
            {
                return null;
            }
            var serializer = DataContractSerializerCache.GetNoTypeDataJsonSerializer<T>();
            using (var str = new MemoryStream())
            {
                serializer.WriteObject(str, obj);
                return Encoding.UTF8.GetString(str.ToArray());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TJson DesereaizeFromString(string str)
        {
            if (str == null)
            {
                return null;
            }
            var serializer = DataContractSerializerCache.GetNoTypeDataJsonSerializer<TJson>();
            var bt = Encoding.UTF8.GetBytes(str);
            using (var instr = new MemoryStream(bt))
            {
                return serializer.ReadObject(instr) as TJson;
            }
        }


        /// <summary>
        /// Десериализовать ссылку.
        /// </summary>
        /// <param name="linkStr">Ссылка в виде строки.</param>
        /// <returns>Ссыока.</returns>
        public ILink Deserialize(string linkStr)
        {
            var json = DesereaizeFromString(linkStr);
            if (json == null)
            {
                return null;
            }
            var result = new T();
            FillValues(result, json);
            return result;
        }

        /// <summary>
        /// Получить объект JSON.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Объект JSON.</returns>
        protected abstract TJson GetJsonObject(T link);

        /// <summary>
        /// Заполнить значения.
        /// </summary>
        /// <param name="result">Результат.</param>
        /// <param name="jsonObject">JSON-объект.</param>
        protected abstract void FillValues(T result, TJson jsonObject);
    }
}