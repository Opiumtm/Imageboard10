using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Сериализатор ссылок.
    /// </summary>
    public sealed class UriLinkSerializer : LinkSerializerBase<UriLink, UriLinkSerializer.Jo>
    {
        public class Jo
        {
            [JsonProperty("u")]
            public string Uri { get; set; }
        }

        public override string LinkTypeId => "uri";

        /// <summary>
        /// Получить объект JSON.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Объект JSON.</returns>
        protected override Jo GetJsonObject(UriLink link)
        {
            return new Jo()
            {
                Uri = link.Uri
            };
        }

        /// <summary>
        /// Заполнить значения.
        /// </summary>
        /// <param name="result">Результат.</param>
        /// <param name="jsonObject">JSON-объект.</param>
        protected override void FillValues(UriLink result, Jo jsonObject)
        {
            result.Uri = jsonObject.Uri;
        }
    }
}