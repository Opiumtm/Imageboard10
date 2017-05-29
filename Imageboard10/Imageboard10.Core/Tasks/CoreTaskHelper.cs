using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Imageboard10.Core.Tasks
{
    /// <summary>
    /// Класс-помощник с тасками.
    /// </summary>
    public static class CoreTaskHelper
    {
        /// <summary>
        /// Запустить таск без ожидания завершения.
        /// </summary>
        /// <param name="task">Таск.</param>
        public static void RunUnawaitedTask(Action task)
        {
            if (task == null)
            {
                return;
            }
            var unawaitedTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    task();
                }
                catch
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            });
        }

        /// <summary>
        /// Запустить таск без ожидания завершения.
        /// </summary>
        /// <param name="task">Таск.</param>
        public static void RunUnawaitedTaskAsync(Func<Task> task)
        {
            if (task == null)
            {
                return;
            }

            async void Do()
            {
                try
                {
                    await task();
                }
                catch
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }

            var unawaitedTask = Task.Factory.StartNew(Do);
        }

        /// <summary>
        /// Запустить таск без ожидания завершения.
        /// </summary>
        /// <param name="task">Таск.</param>
        public static void RunUnawaitedTaskAsync2(Func<ValueTask<Nothing>> task)
        {
            if (task == null)
            {
                return;
            }

            async void Do()
            {
                try
                {
                    await task();
                }
                catch
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }

            var unawaitedTask = Task.Factory.StartNew(Do);
        }

        /// <summary>
        /// Выполнить асинхронную функцию на новом потоке.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="asyncFunc">Асинхронная функция.</param>
        /// <returns>Таск с результатом.</returns>
        public static Task<T> RunAsyncFuncOnNewThread<T>(Func<Task<T>> asyncFunc)
        {
            if (asyncFunc == null) throw new ArgumentNullException(nameof(asyncFunc));
            var tcs = new TaskCompletionSource<T>();

            async void Do()
            {
                try
                {
                    tcs.TrySetResult(await asyncFunc());
                }
                catch (Exception ex)
                {
                    try
                    {
                        tcs.TrySetException(ex);
                    }
                    catch
                    {
                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
                    }
                }
            }

            Task.Factory.StartNew(Do);
            return tcs.Task;
        }
    }
}