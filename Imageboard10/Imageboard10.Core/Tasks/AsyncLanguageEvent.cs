using System;

namespace Imageboard10.Core.Tasks
{
    /// <summary>
    /// Список асинхронных событий.
    /// </summary>
    /// <typeparam name="T">Тип события.</typeparam>
    public abstract class AsyncLanguageEvent<T>
        where T : EventArgs
    {
        /// <summary>
        /// Добавить обработчик.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        public abstract void AddHandler(AsyncEventHandler<T> handler);

        /// <summary>
        /// Удалить обработчик.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        public abstract void RemoveHandler(AsyncEventHandler<T> handler);

        public static AsyncLanguageEvent<T> operator +(AsyncLanguageEvent<T> src, AsyncEventHandler<T> handler)
        {
            src?.AddHandler(handler);
            return src;
        }

        public static AsyncLanguageEvent<T> operator -(AsyncLanguageEvent<T> src, AsyncEventHandler<T> handler)
        {
            src?.RemoveHandler(handler);
            return src;
        }
    }
}