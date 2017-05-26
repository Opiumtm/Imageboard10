using System;
using Imageboard10.Core.Modules;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Сервис сериализации ссылок.
    /// </summary>
    public class LinkSerializationService : ModuleBase<ILinkSerializationService>, ILinkSerializationService
    {
        /// <summary>
        /// Сериализовать ссылку.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Ссылка в виде строки.</returns>
        public string Serialize(BoardLinkBase link)
        {
            if (link == null) throw new ArgumentNullException(nameof(link));
            var serializer = ModuleProvider?.QueryModule<ILinkSerializer, Type>(link.GetType());
            if (serializer == null)
            {
                throw new InvalidOperationException($"Не найдена логика сериализации для ссылки типа {link.GetType().FullName}");
            }
            return JsonConvert.SerializeObject(new Jo()
            {
                TypeId = serializer.LinkTypeId,
                Link = serializer.Serialize(link)
            });
        }

        /// <summary>
        /// Десериализовать ссылку.
        /// </summary>
        /// <param name="linkStr">Ссылка в виде строки.</param>
        /// <returns>Ссыока.</returns>
        public BoardLinkBase Deserialize(string linkStr)
        {
            if (linkStr == null) throw new ArgumentNullException(nameof(linkStr));
            var jo = JsonConvert.DeserializeObject<Jo>(linkStr);
            if (jo.TypeId == null || jo.Link == null)
            {
                throw new InvalidOperationException("Неправильный формат сериализованной ссылки");
            }
            var serializer = ModuleProvider?.QueryModule<ILinkSerializer, string>(jo.TypeId);
            if (serializer == null)
            {
                throw new InvalidOperationException($"Не найдена логика сериализации для ссылки идентификатора типа {jo.TypeId}");
            }
            return serializer.Deserialize(jo.Link);
        }

        public class Jo
        {
            [JsonProperty("t")]
            public string TypeId { get; set; }
            [JsonProperty("l")]
            public string Link { get; set; }
        }
    }
}