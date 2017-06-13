﻿using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Преобразовать иерархию в список.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="nodes">Ноды.</param>
        /// <param name="getChildren">Получение дочерних элементов.</param>
        /// <returns></returns>
        public static IEnumerable<T> FlatHierarchy<T>(this IEnumerable<T> nodes, Func<T, IEnumerable<T>> getChildren)
        {
            return (nodes ?? new T[0]).SelectMany(c => FlatHierarchy(c, getChildren));
        }

        /// <summary>
        /// Преобразовать иерархию в список.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="node">Нода.</param>
        /// <param name="getChildren">Получение дочерних элементов.</param>
        /// <returns></returns>
        public static IEnumerable<T> FlatHierarchy<T>(this T node, Func<T, IEnumerable<T>> getChildren)
        {
            yield return node;
            foreach (var item in FlatHierarchy(getChildren(node), getChildren))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Сделать случайный порядок перечисления.
        /// </summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <param name="src">Исходное перечисление.</param>
        /// <returns>Результат.</returns>
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> src)
        {
            var random = new Random();
            return src.Select(item => new { element = item, order = random.Next() }).OrderBy(item => item.order).Select(item => item.element);
        }

        /// <summary>
        /// Удалить повторы.
        /// </summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <typeparam name="TKey">Тип ключа.</typeparam>
        /// <param name="src">Исходное перечисление.</param>
        /// <param name="keyFunc">Функция получения ключа.</param>
        /// <param name="comparer">Средство сравнения.</param>
        /// <returns>Результат.</returns>
        public static IEnumerable<T> Deduplicate<T, TKey>(this IEnumerable<T> src, Func<T, TKey> keyFunc, IEqualityComparer<TKey> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;
            return src.GroupBy(keyFunc, comparer).Select(a => a.First());
        }

        /// <summary>
        /// Получить последовательность с ключами.
        /// </summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <typeparam name="TKey">Тип ключа.</typeparam>
        /// <param name="src">Исходное перечисление.</param>
        /// <param name="keyFunc">Функция получения ключа.</param>
        /// <returns>Результат.</returns>
        public static IEnumerable<KeyValuePair<TKey, T>> WithKeys<T, TKey>(this IEnumerable<T> src, Func<T, TKey> keyFunc)
        {
            return src.Select(a => new KeyValuePair<TKey, T>(keyFunc(a), a));
        }

        /// <summary>
        /// Удалить повторы и сделать словарём.
        /// </summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <typeparam name="TKey">Тип ключа.</typeparam>
        /// <param name="src">Исходное перечисление.</param>
        /// <param name="keyFunc">Функция получения ключа.</param>
        /// <param name="comparer">Средство сравнения.</param>
        /// <returns>Результат.</returns>
        public static Dictionary<TKey, T> DeduplicateToDictionary<T, TKey>(this IEnumerable<T> src, Func<T, TKey> keyFunc, IEqualityComparer<TKey> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;
            return src.WithKeys(keyFunc).Deduplicate(a => a.Key, comparer).ToDictionary(a => a.Key, a => a.Value, comparer);
        }

        /// <summary>
        /// Разбить последовательность.
        /// </summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <param name="src">Источник.</param>
        /// <param name="count">На сколько разбивать.</param>
        /// <returns>Разбитая последовательность.</returns>
        public static ILookup<int, T> SplitLookup<T>(this IEnumerable<T> src, int count = 100)
        {
            if (count <= 0)
            {
                count = 1;
            }
            return src.WithCounter().ToLookup(t => t.Key / count, t => t.Value);
        }

        /// <summary>
        /// Получить список вместе со счётчиком.
        /// </summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <param name="src">Источник.</param>
        /// <returns>Последовательность со счётчиком.</returns>
        public static IEnumerable<KeyValuePair<int, T>> WithCounter<T>(this IEnumerable<T> src)
        {
            int i = 0;
            foreach (var t in src)
            {
                yield return new KeyValuePair<int, T>(i, t);
                i++;
            }
        }
    }
}