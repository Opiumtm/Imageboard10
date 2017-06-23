using Newtonsoft.Json;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Сущность "борда или тред".
    /// </summary>
    public class BoardEntity2 : BoardEntity2Base
    {
        /// <summary>
        /// Треды.
        /// </summary>
        [JsonProperty("threads")]
        public BoardThread2[] Threads { get; set; }
    }
}