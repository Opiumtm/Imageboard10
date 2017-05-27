using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Сериализатор ссылок.
    /// </summary>
    public sealed class YoutubeLinkSerializer : LinkSerializerBase<YoutubeLink, YoutubeLinkSerializer.Jo>
    {
        public class Jo
        {
            [JsonProperty("id")]
            public string YoutubeId { get; set; }
        }

        public override string LinkTypeId => "youtube";

        /// <summary>
        /// Получить объект JSON.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Объект JSON.</returns>
        protected override Jo GetJsonObject(YoutubeLink link)
        {
            return new Jo()
            {
                YoutubeId = link.YoutubeId
            };
        }

        /// <summary>
        /// Заполнить значения.
        /// </summary>
        /// <param name="result">Результат.</param>
        /// <param name="jsonObject">JSON-объект.</param>
        protected override void FillValues(YoutubeLink result, Jo jsonObject)
        {
            result.YoutubeId = jsonObject.YoutubeId;
        }
    }
}