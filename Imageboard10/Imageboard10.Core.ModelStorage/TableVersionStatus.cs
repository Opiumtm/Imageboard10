using System;
using System.Threading;
using System.Threading.Tasks;
using Imageboard10.Core.Tasks;

namespace Imageboard10.Core.ModelStorage
{
    /// <summary>
    /// Статус информации о версиях таблиц.
    /// </summary>
    internal class TableVersionStatus
    {
        private static TableVersionStatus _instance;

        /// <summary>
        /// Экземпляр.
        /// </summary>
        public static TableVersionStatus Instance => Interlocked.CompareExchange(ref _instance, null, null);

        private int _isProcessing;

        private readonly TaskCompletionSource<Nothing> _tcs;

        private readonly Task _task;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public TableVersionStatus()
        {
            _tcs = new TaskCompletionSource<Nothing>();
            _task = _tcs.Task;
        }

        /// <summary>
        /// Один раз вызвать инициализацию таблицы tableversion или ждать, если инициализация уже запущена.
        /// </summary>
        /// <param name="initFunc">Функция инициализации.</param>
        public async Task InitializeTableVersionOnce(Func<ValueTask<Nothing>> initFunc)
        {
            if (initFunc == null) throw new ArgumentNullException(nameof(initFunc));
            if (Interlocked.Exchange(ref _isProcessing, 1) == 0)
            {
                try
                {
                    await CoreTaskHelper.RunAsyncFuncOnNewThreadValueTask(initFunc);
                    _tcs.SetResult(Nothing.Value);
                }
                catch (Exception ex)
                {
                    _tcs.SetException(ex);
                }
            }
            else
            {
                await _task;
            }
        }

        /// <summary>
        /// Сбросить флаг инициализации.
        /// </summary>
        public static void ClearInstance()
        {
            Interlocked.Exchange(ref _instance, new TableVersionStatus());
        }
    }
}