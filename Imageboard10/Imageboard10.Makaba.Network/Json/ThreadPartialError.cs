using Newtonsoft.Json;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Ошибка получения части треда.
    /// </summary>
    public class ThreadPartialError
    {
        /// <summary>
        /// Код.
        /// </summary>
        [JsonProperty("Code")]
        public int Code { get; set; }

        /// <summary>
        /// Ошибка.
        /// </summary>
        [JsonProperty("Error")]
        public string Error { get; set; }
    }
}