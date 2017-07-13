using System;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Реализация IDisposable по действию.
    /// </summary>
    public sealed class ActionDisposable : IDisposable
    {
        private readonly Action _dispose;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="dispose">Действие.</param>
        public ActionDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _dispose?.Invoke();
        }

        public static implicit operator ActionDisposable(Action action)
        {
            return new ActionDisposable(action);
        }
    }
}