using System;
using System.Collections.Generic;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Класс-помощник для Linq.
    /// </summary>
    public static class LinqHelper
    {
        /// <summary>
        /// Разделить перечисление на части с ограничением по размеру.
        /// </summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <param name="src">Исходное перечисление.</param>
        /// <param name="maxCount">Максимальное количество элементов.</param>
        /// <returns>Результат.</returns>
        public static IEnumerable<IEnumerable<T>> SplitSet<T>(this IEnumerable<T> src, int maxCount)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            var buffer = new List<T>(maxCount);
            foreach (var el in src)
            {
                buffer.Add(el);
                if (buffer.Count >= maxCount)
                {
                    yield return buffer;
                    buffer = new List<T>(maxCount);
                }
            }
            if (buffer.Count > 0)
            {
                yield return buffer;
            }
        }
    }
}