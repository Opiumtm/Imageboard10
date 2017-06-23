using Newtonsoft.Json;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// "Верхняя" реклама досок.
    /// </summary>
    public class BoardEntity2TopAdvert
    {
        /// <summary>
        /// Доска.
        /// </summary>
        [JsonProperty("board")]
        public string Board { get; set; }

        /// <summary>
        /// Информация.
        /// </summary>
        [JsonProperty("info")]
        public string Info { get; set; }

        /// <summary>
        /// Имя.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}