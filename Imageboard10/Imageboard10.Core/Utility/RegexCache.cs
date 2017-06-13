using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Кэш регулярный выражений.
    /// </summary>
    public static class RegexCache
    {
        private static readonly Dictionary<string, Regex> RegexCacheDic = new Dictionary<string, Regex>();

        /// <summary>
        /// Создать регулярное выражение.
        /// </summary>
        /// <param name="expression">Выражение.</param>
        /// <returns>Объект.</returns>
        public static Regex CreateRegex(string expression)
        {
            lock (RegexCacheDic)
            {
                if (!RegexCacheDic.ContainsKey(expression))
                {
                    RegexCacheDic[expression] = new Regex(expression, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                }
                return RegexCacheDic[expression];
            }
        }
    }
}