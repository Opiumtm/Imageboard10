using System;
using System.Threading.Tasks;

namespace Imageboard10.Core.Tasks
{
    /// <summary>
    /// Защита от использования с другого потока.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    public struct ThreadAccessGuard<T>
    {
        private readonly SingleThreadDispatcher _dispatcher;

        private readonly T _value;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="dispatcher">Диспетчер.</param>
        /// <param name="value">Значение.</param>
        public ThreadAccessGuard(SingleThreadDispatcher dispatcher, T value)
        {
            _dispatcher = dispatcher;
            _value = value;
        }

        /// <summary>
        /// Значение.
        /// </summary>
        public T Value
        {
            get
            {
                _dispatcher?.CheckAccess();
                return _value;
            }
        }

        /// <summary>
        /// Есть ли доступ из треда.
        /// </summary>
        /// <returns>true, если есть доступ.</returns>
        public bool HaveAccess()
        {
            return _dispatcher?.HaveAccess() ?? true;
        }

        /// <summary>
        /// Выполнить функцию с гарантированным доступом к переменной.
        /// </summary>
        /// <typeparam name="T2">Тип результата.</typeparam>
        /// <param name="func">Функция.</param>
        /// <returns>Результат функции.</returns>
        public ValueTask<T2> Access<T2>(Func<T, T2> func)
        {
            if (func == null)
            {
                return new ValueTask<T2>(default(T2));
            }
            if (_dispatcher?.HaveAccess() ?? true)
            {
                return new ValueTask<T2>(func(_value));
            }

            async ValueTask<T2> Do(SingleThreadDispatcher d, T v)
            {
                return await d.QueueAction(() => func(v));
            }

            return Do(_dispatcher, _value);
        }
    }
}