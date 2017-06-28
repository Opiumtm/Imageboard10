using System;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Текстовые утилиты.
    /// </summary>
    public static class TextUtility
    {
        /// <summary>
        /// Проверка на равенство строк без учёта регистра.
        /// </summary>
        /// <param name="a">Первая строка.</param>
        /// <param name="b">Вторая строка.</param>
        /// <returns>Результат проверки.</returns>
        public static bool EqualsNc(this string a, string b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            return a.Equals(b, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Попробовать отпарсить цифровое значение.
        /// </summary>
        /// <param name="src">Строка.</param>
        /// <param name="def">Значение по умолчанию.</param>
        /// <returns>Целое число.</returns>
        public static int TryParseWithDefault(this string src, int def = 0)
        {
            if (src == null)
            {
                return def;
            }
            int result;
            if (int.TryParse(src, out result))
            {
                return result;
            }
            return def;
        }

        /// <summary>
        /// Попробовать отпарсить цифровое значение.
        /// </summary>
        /// <param name="src">Строка.</param>
        /// <param name="def">Значение по умолчанию.</param>
        /// <returns>Целое число.</returns>
        public static int? TryParseWithNull(this string src)
        {
            if (src == null)
            {
                return null;
            }
            int result;
            if (int.TryParse(src, out result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Удалить слэш в начале.
        /// </summary>
        /// <param name="src">Исходная строка.</param>
        /// <returns>Строка.</returns>
        public static string RemoveStartingSlash(this string src)
        {
            return src.StartsWith("/") ? src.Remove(0, 1) : src;
        }
    }
}