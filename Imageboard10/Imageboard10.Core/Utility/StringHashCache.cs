using System.Collections.Generic;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Кэш хэшей для строк.
    /// </summary>
    public static class StringHashCache
    {
        private static readonly Dictionary<string, string> HashIdCache = new Dictionary<string, string>();

        /// <summary>
        /// Получить хэш для строки.
        /// </summary>
        /// <param name="id">Строка-идентификатор.</param>
        /// <returns>Строка с хэшем.</returns>
        public static string GetHashId(string id)
        {
            lock (HashIdCache)
            {
                if (HashIdCache.Count > 512)
                {
                    HashIdCache.Clear();
                }
                var id1 = (id ?? "").ToLowerInvariant();
                if (!HashIdCache.ContainsKey(id1))
                {
                    HashIdCache[id1] = UniqueIdHelper.CreateIdString(id1);
                }
                return HashIdCache[id1];
            }
        }
    }
}