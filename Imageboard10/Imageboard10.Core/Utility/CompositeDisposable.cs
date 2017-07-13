using System;
using System.Collections.Generic;
using System.Linq;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Составное завершение работы.
    /// </summary>
    public sealed class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="disposables">Список для завершения.</param>
        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            if (disposables == null)
            {
                _disposables = new List<IDisposable>();
            }
            else
            {
                _disposables = disposables.ToList();
            }
        }

        /// <summary>
        /// Добавить завершение.
        /// </summary>
        /// <param name="disposable">Средство завершения.</param>
        public void AddDisposable(IDisposable disposable)
        {
            lock (_disposables)
            {
                _disposables.Add(disposable);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            var errors = new List<Exception>();
            IDisposable[] toDispose;
            lock (_disposables)
            {
                toDispose = _disposables.ToArray();
                _disposables.Clear();
            }
            foreach (var d in toDispose)
            {
                try
                {
                    d?.Dispose();
                }
                catch (Exception e)
                {
                    errors.Add(e);
                }
            }
            if (errors.Count > 0)
            {
                throw new AggregateException(errors);
            }
        }
    }
}