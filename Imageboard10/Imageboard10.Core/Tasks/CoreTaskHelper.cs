using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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

        /// <summary>
        /// Выполнить асинхронную функцию на новом потоке.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="asyncFunc">Асинхронная функция.</param>
        /// <returns>Таск с результатом.</returns>
        public static Task<T> RunAsyncFuncOnNewThreadValueTask<T>(Func<ValueTask<T>> asyncFunc)
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

        /// <summary>
        /// Привести Task к виду ValueTask.
        /// </summary>
        /// <typeparam name="T">Вид результата.</typeparam>
        /// <param name="task">Таск.</param>
        /// <returns>Вещественный таск.</returns>
        public static async ValueTask<T> AsValueTask<T>(this Task<T> task)
        {
            return await task;
        }

        /// <summary>
        /// Привести ValueTask к виду Task.
        /// </summary>
        /// <typeparam name="T">Вид результата.</typeparam>
        /// <param name="task">Вещественный таск.</param>
        /// <returns>Таск.</returns>
        public static async Task<T> AsTask<T>(this ValueTask<T> task)
        {
            return await task;
        }

        /// <summary>
        /// Привести Task к виду ValueTask.
        /// </summary>
        /// <param name="task">Таск.</param>
        /// <returns>Вещественный таск.</returns>
        public static async ValueTask<Nothing> AsValueTask(this Task task)
        {
            await task;
            return Nothing.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static async ValueTask<T[]> WhenAllValueTasks<T>(IEnumerable<ValueTask<T>> tasks)
        {
            var results = new List<T>();
            var errors = new List<Exception>();
            foreach (var t in tasks)
            {
                try
                {
                    results.Add(await t);
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
            return results.ToArray();
        }
    }
}
