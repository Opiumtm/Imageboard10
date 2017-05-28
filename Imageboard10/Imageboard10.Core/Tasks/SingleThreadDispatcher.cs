using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Imageboard10.Core.Tasks
{
    /// <summary>
    /// Однопоточный диспетчер.
    /// </summary>
    public sealed class SingleThreadDispatcher : IDisposable
    {
        private readonly AutoResetEvent _queuedEvent = new AutoResetEvent(false);

        private readonly ManualResetEvent _disposedEvent = new ManualResetEvent(false);

        private readonly ConcurrentQueue<ActionInfo> _queue = new ConcurrentQueue<ActionInfo>();

        private readonly Task _queueTask;

        private int _isDisposed;

        public SingleThreadDispatcher()
        {
            _queueTask = Task.Factory.StartNew(TaskAction, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Выполнить действие на выделенном потоке.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="action">Действие.</param>
        /// <returns>Результат.</returns>
        public async Task<T> QueueAction<T>(Func<T> action)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 0)
            {
                if (Task.CurrentId == _queueTask.Id)
                {
                    throw new InvalidOperationException("Не поддерживается постановка в очередь задачи из того же потока диспетчеризации");
                }
                var tcs = new TaskCompletionSource<object>();
                _queue.Enqueue(new ActionInfo()
                {
                    tcs = tcs,
                    action = () =>
                    {
                        return action();
                    }
                });
                _queuedEvent.Set();
                var obj = await tcs.Task;
                return (T)obj;
            }
            throw new ObjectDisposedException("SingleThreadDispatcher");
        }

        private void TaskAction()
        {
            try
            {
                var waiters = new WaitHandle[]
                {
                    _queuedEvent,
                    _disposedEvent
                };
                do
                {
                    var idx = WaitHandle.WaitAny(waiters, TimeSpan.FromSeconds(7));
                    if (idx == 0 || idx == WaitHandle.WaitTimeout)
                    {
                        ActionInfo info;
                        while (_queue.TryDequeue(out info))
                        {
                            object result = null;
                            Exception error = null;
                            try
                            {
                                result = info.action();
                            }
                            catch (Exception e)
                            {
                                error = e;
                            }
                            var p = new Tuple<object, Exception, TaskCompletionSource<object>>(
                                result,
                                error,
                                info.tcs);
                            Task.Factory.StartNew(pp =>
                            {
                                try
                                {
                                    var r = (Tuple<object, Exception, TaskCompletionSource<object>>)pp;
                                    if (r.Item2 != null)
                                    {
                                        r.Item3.TrySetException(r.Item2);
                                    }
                                    else
                                    {
                                        r.Item3.TrySetResult(r.Item1);
                                    }
                                }
                                catch
                                {
                                }
                            }, p);
                        }
                    }
                    else
                    {
                        break;
                    }
                } while (true);
                _disposedEvent.Dispose();
                _queuedEvent.Dispose();
                ActionInfo info2;
                while (_queue.TryDequeue(out info2))
                {
                    var i = info2;
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            i.tcs.TrySetCanceled();
                        }
                        catch
                        {
                        }
                    });
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                _disposedEvent.Set();
            }
        }

        private struct ActionInfo
        {
            // ReSharper disable once InconsistentNaming
            public TaskCompletionSource<object> tcs;
            // ReSharper disable once InconsistentNaming
            public Func<object> action;
        }
    }
}