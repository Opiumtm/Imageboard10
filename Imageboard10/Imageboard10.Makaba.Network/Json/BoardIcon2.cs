using Newtonsoft.Json;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Иконка борды.
    /// </summary>
    public class BoardIcon2
    {
        /// <summary>
        /// Имя.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Номер.
        /// </summary>
        [JsonProperty("num")]
        public string Number { get; set; }

        /// <summary>
        /// URL.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public int NumberInt
        {
            get
            {
                int r;
                if (int.TryParse(Number, out r))
                {
                    return r;
                }
                return 0;
            }
        }
    }
}