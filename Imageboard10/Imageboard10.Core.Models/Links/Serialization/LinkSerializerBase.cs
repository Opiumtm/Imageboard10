using System;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Modules;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Базовый класс сериализации ссылок.
    /// </summary>
    /// <typeparam name="T">Тип ссылки.</typeparam>
    /// <typeparam name="TJson">Тип JSON-объекта.</typeparam>
    public abstract class LinkSerializerBase<T, TJson> : ModuleBase<ILinkSerializer>, ILinkSerializer
        where T : class, ILink, new()
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
            if (link == null) throw new ArgumentNullException(nameof(link));
            if (link is T l)
            {
                return JsonConvert.SerializeObject(GetJsonObject(l));
            }
            throw new ArgumentException($"Неправильный тип ссылки для сериализации {link.GetType().FullName}");
        }

        /// <summary>
        /// Десериализовать ссылку.
        /// </summary>
        /// <param name="linkStr">Ссылка в виде строки.</param>
        /// <returns>Ссыока.</returns>
        public ILink Deserialize(string linkStr)
        {
            var json = JsonConvert.DeserializeObject<TJson>(linkStr);
            if (json == null)
            {
                throw new InvalidOperationException("Ошибка десериализации ссылки");
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