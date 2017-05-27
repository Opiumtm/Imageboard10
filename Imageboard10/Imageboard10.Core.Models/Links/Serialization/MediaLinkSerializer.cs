using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Сериализатор ссылок.
    /// </summary>
    public sealed class MediaLinkSerializer : LinkSerializerBase<MediaLink, MediaLinkSerializer.Jo>
    {
        public class Jo
        {
            [JsonProperty("u")]
            public string Uri { get; set; }
        }

        public override string LinkTypeId => "media";

        /// <summary>
        /// Получить объект JSON.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Объект JSON.</returns>
        protected override Jo GetJsonObject(MediaLink link)
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
        protected override void FillValues(MediaLink result, Jo jsonObject)
        {
            result.Uri = jsonObject.Uri;
        }
    }

}