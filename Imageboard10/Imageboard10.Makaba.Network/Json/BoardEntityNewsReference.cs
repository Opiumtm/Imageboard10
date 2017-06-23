using Newtonsoft.Json;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Новости.
    /// </summary>
    public class BoardEntityNewsReference
    {
        /// <summary>
        /// Дата.
        /// </summary>
        [JsonProperty("date")]
        public string Date { get; set; }

        /// <summary>
        /// Номер.
        /// </summary>
        [JsonProperty("num")]
        public int Number { get; set; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        [JsonProperty("subject")]
        public string Subject { get; set; }
    }
}