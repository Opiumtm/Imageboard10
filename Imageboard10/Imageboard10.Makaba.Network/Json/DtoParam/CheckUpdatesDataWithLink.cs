using Imageboard10.Core.Models.Links.LinkTypes;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Данные об обновлении.
    /// </summary>
    public struct CheckUpdatesDataWithLink
    {
        /// <summary>
        /// Ссылка.
        /// </summary>
        public ThreadLink Link;

        /// <summary>
        /// Данные.
        /// </summary>
        public CheckUpdatesData Data;
    }
}