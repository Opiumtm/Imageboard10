using Newtonsoft.Json;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Ответ от Makaba на постинг.
    /// </summary>
    public class MakabaPostResponse
    {
        /// <summary>
        /// Статус.
        /// </summary>
        [JsonProperty("Reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Статус.
        /// </summary>
        [JsonProperty("Status")]
        public string Status { get; set; }

        /// <summary>
        /// Номер треда.
        /// </summary>
        [JsonProperty("Num")]
        public string Num { get; set; }

        /// <summary>
        /// Номер треда.
        /// </summary>
        [JsonProperty("Target")]
        public string Target { get; set; }

        /// <summary>
        /// Ошибка.
        /// </summary>
        [JsonProperty("Error")]
        public string Error { get; set; }
    }
}