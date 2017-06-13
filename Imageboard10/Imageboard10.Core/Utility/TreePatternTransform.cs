using System;
using System.Collections.Generic;
using System.Linq;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Сопоставление с образцом для деревьев.
    /// </summary>
    public static class TreePatternTransform
    {
        /// <summary>
        /// Создать контекст рекурсивного обхода дерева.
        /// </summary>
        /// <typeparam name="T">Тип элемента дерева.</typeparam>
        /// <typeparam name="TApp">Тип результата.</typeparam>
        /// <param name="source">Источник.</param>
        /// <param name="result">Результат.</param>
        /// <returns>Контекст.</returns>
        public static TreeWalkContext<T, TApp> TreeWalk<T, TApp>(this IEnumerable<T> source, TApp result)
        {
            return new TreeWalkContext<T, TApp>()
            {
                Source = source,
                Result = result,
                DefaultApply = (i, r) => r,
                DefaultGetChildren = i => null
            };
        }

        /// <summary>
        /// Создать сопоставление для рекурсивного обхода дерева.
        /// </summary>
        /// <typeparam name="T">Тип элемента дерева.</typeparam>
        /// <typeparam name="TApp">Тип результата.</typeparam>
        /// <param name="context">Контекст.</param>
        /// <param name="ifFunc">Функция сопоставления.</param>
        /// <param name="applyFunc">Функция применения результата.</param>
        /// <param name="getChildrenFunc">Функция получения дочерних элементов.</param>
        /// <returns>Контекст.</returns>
        public static TreeWalkContext<T, TApp> If<T, TApp>(this TreeWalkContext<T, TApp> context, Func<T, bool> ifFunc,
            Func<T, TApp, TApp> applyFunc = null,
            Func<T, IEnumerable<T>> getChildrenFunc = null)
        {
            context.Functions.Add(new TreeApplyFunc<T, TApp>()
            {
                Apply = applyFunc,
                GetChildren = getChildrenFunc,
                If = ifFunc ?? (v => true),
                IsElse = ifFunc == null
            });
            return context;
        }

        /// <summary>
        /// Создать сопоставление для рекурсивного обхода дерева для прочих случаев.
        /// </summary>
        /// <typeparam name="T">Тип элемента дерева.</typeparam>
        /// <typeparam name="TApp">Тип результата.</typeparam>
        /// <param name="context">Контекст.</param>
        /// <param name="applyFunc">Функция применения результата.</param>
        /// <param name="getChildrenFunc">Функция получения дочерних элементов.</param>
        /// <returns>Контекст.</returns>
        public static TreeWalkContext<T, TApp> Else<T, TApp>(this TreeWalkContext<T, TApp> context,
            Func<T, TApp, TApp> applyFunc = null,
            Func<T, IEnumerable<T>> getChildrenFunc = null)
        {
            return context.If(null, applyFunc, getChildrenFunc);
        }

        /// <summary>
        /// Установить функцию получения дочерних элементов по умолчанию.
        /// </summary>
        /// <typeparam name="T">Тип элемента дерева.</typeparam>
        /// <typeparam name="TApp">Тип результата.</typeparam>
        /// <param name="context">Контекст.</param>
        /// <param name="getChildrenFunc">Функция получения дочерних элементов.</param>
        /// <returns>Контекст.</returns>
        public static TreeWalkContext<T, TApp> GetChildren<T, TApp>(this TreeWalkContext<T, TApp> context, Func<T, IEnumerable<T>> getChildrenFunc)
        {
            context.DefaultGetChildren = getChildrenFunc;
            return context;
        }

        /// <summary>
        /// Установить функцию применения результата по умолчанию.
        /// </summary>
        /// <typeparam name="T">Тип элемента дерева.</typeparam>
        /// <typeparam name="TApp">Тип результата.</typeparam>
        /// <param name="context">Контекст.</param>
        /// <param name="applyFunc">Функция применения результата.</param>
        /// <returns>Контекст.</returns>
        public static TreeWalkContext<T, TApp> Apply<T, TApp>(this TreeWalkContext<T, TApp> context, Func<T, TApp, TApp> applyFunc)
        {
            context.DefaultApply = applyFunc;
            return context;
        }

        /// <summary>
        /// Выполнить проход по дереву.
        /// </summary>
        /// <typeparam name="T">Тип элемента дерева.</typeparam>
        /// <typeparam name="TApp">Тип результата.</typeparam>
        /// <param name="context">Контекст.</param>
        /// <returns>Результат.</returns>
        public static TApp Run<T, TApp>(this TreeWalkContext<T, TApp> context)
        {
            WalkTree(context, context.Source, context.Result);
            return context.Result;
        }

        private static void WalkTree<T, TApp>(TreeWalkContext<T, TApp> context, IEnumerable<T> elements,
            TApp currentResult)
        {
            if (elements == null || context.IsBreak)
            {
                return;
            }
            foreach (var item in elements)
            {
                if (context.IsBreak)
                {
                    break;
                }
                var applyFunc = context.Functions.Where(f => !f.IsElse).FirstOrDefault(f => f.If(item)) ??
                                context.Functions.Where(f => f.IsElse).FirstOrDefault(f => f.If(item));
                if (applyFunc != null)
                {
                    var newResult = (applyFunc.Apply ?? context.DefaultApply)(item, currentResult);
                    var children = (applyFunc.GetChildren ?? context.DefaultGetChildren)(item);
                    WalkTree(context, children, newResult);
                }
            }
        }
    }
}