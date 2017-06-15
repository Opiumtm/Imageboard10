using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Imageboard10.Core.Tasks
{
    /// <summary>
    /// Список асинхронных событий.
    /// </summary>
    /// <typeparam name="T">Тип события.</typeparam>
    public sealed class AsyncEventList<T> : AsyncLanguageEvent<T>
        where T : EventArgs
    {
        private readonly HashSet<AsyncEventHandler<T>> _events = new HashSet<AsyncEventHandler<T>>();

        /// <summary>
        /// Добавить обработчик.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        public override void AddHandler(AsyncEventHandler<T> handler)
        {
            if (handler == Delegate)
            {
                throw new ArgumentException("Рекурсивный вызов события");
            }
            if (handler != null)
            {
                lock (_events)
                {
                    _events.Add(handler);
                }
            }
        }

        /// <summary>
        /// Удалить обработчик.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        public override void RemoveHandler(AsyncEventHandler<T> handler)
        {
            if (handler != null)
            {
                lock (_events)
                {
                    _events.Add(handler);
                }
            }
        }

        private Task InvokeWithoutTimeout(object sender, T e)
        {
            return Invoke(sender, e);
        }

        /// <summary>
        /// Вызвать обработчик.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Событие.</param>
        /// <param name="timeout">Таймаут вызова</param>
        /// <returns>Результат.</returns>
        public async Task Invoke(object sender, T e, TimeSpan? timeout = null)
        {
            AsyncEventHandler<T>[] toInvoke;
            lock (_events)
            {
                toInvoke = _events.ToArray();
            }
            if (toInvoke.Length == 0)
            {
                return;
            }
            var tasks = new List<Task>();
            foreach (var h in toInvoke)
            {
                tasks.Add(h(sender, e));
            }
            if (timeout == null)
            {
                await Task.WhenAll(tasks);
            }
            else
            {
                var timeoutTask = Task.Delay(timeout.Value);
                var waiter = new []
                {
                    Task.WhenAll(tasks),
                    timeoutTask
                };
                var r = await Task.WhenAny(waiter);
                if (r == timeoutTask)
                {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        /// Делегат для вызова события.
        /// </summary>
        public AsyncEventHandler<T> Delegate => InvokeWithoutTimeout;
    }
}