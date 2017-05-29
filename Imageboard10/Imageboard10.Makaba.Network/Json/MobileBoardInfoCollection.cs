using System.Collections.Generic;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Коллекция информации о досках.
    /// </summary>
    public struct MobileBoardInfoCollection
    {
        /// <summary>
        /// Доски.
        /// </summary>
        public Dictionary<string, MobileBoardInfo[]> Boards;
    }
}