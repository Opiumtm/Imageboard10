using System;

namespace Imageboard10.Core.Modules.Wrappers
{
    /// <summary>
    /// Базовый класс для обёрток.
    /// </summary>
    /// <typeparam name="T">Тип исходного объекта.</typeparam>
    public abstract class WrapperBase<T>
    {
        /// <summary>
        /// Исходный объект.
        /// </summary>
        protected readonly T Wrapped;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Обёртка.</param>
        protected WrapperBase(T wrapped)
        {
            if (wrapped == null) throw new ArgumentNullException(nameof(wrapped));
            Wrapped = wrapped;
        }
    }
}