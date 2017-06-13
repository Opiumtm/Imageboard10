using System;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Сопоставление с образцом для дерева по шаблону.
    /// </summary>
    public static class TreeTemplatePatternMatch
    {
        /// <summary>
        /// Пройтись по шаблону.
        /// </summary>
        /// <typeparam name="T1">Тип исходного элемента.</typeparam>
        /// <typeparam name="T2">Тип элемента-результата.</typeparam>
        /// <param name="value">Значение.</param>
        /// <param name="check">Проверка</param>
        /// <param name="getNext">Получить следующий элемент.</param>
        /// <returns>Результат.</returns>
        public static T2 WalkTemplate<T1, T2>(this T1 value, Func<T1, bool> check, Func<T1, T2> getNext)
            where T1 : class
            where T2 : class
        {
            if (value == null)
            {
                return null;
            }
            if (!check(value))
            {
                return null;
            }
            return getNext(value);
        }

        /// <summary>
        /// Пройтись по шаблону.
        /// </summary>
        /// <typeparam name="T1">Тип исходного элемента.</typeparam>
        /// <typeparam name="T2">Тип элемента-результата.</typeparam>
        /// <param name="value">Значение.</param>
        /// <param name="getNext">Получить следующий элемент.</param>
        /// <returns>Результат.</returns>
        public static T2 WalkTemplate<T1, T2>(this T1 value, Func<T1, T2> getNext)
            where T1 : class
            where T2 : class
        {
            if (value == null)
            {
                return null;
            }
            return getNext(value);
        }

    }
}