using Newtonsoft.Json;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Сущность каталога.
    /// </summary>
    public class CatalogEntity : BoardEntity2Base
    {
        /// <summary>
        /// Треды.
        /// </summary>
        [JsonProperty("threads")]
        public BoardPost2[] Threads { get; set; }
    }
}