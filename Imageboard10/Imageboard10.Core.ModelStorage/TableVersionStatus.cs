using System;
using System.Threading;
using System.Threading.Tasks;
using Imageboard10.Core.Tasks;

namespace Imageboard10.Core.ModelStorage
{
    /// <summary>
    /// ������ ���������� � ������� ������.
    /// </summary>
    internal class TableVersionStatus
    {
        private static TableVersionStatus _instance;

        /// <summary>
        /// ���������.
        /// </summary>
        public static TableVersionStatus Instance => Interlocked.CompareExchange(ref _instance, null, null);

        private int _isProcessing;

        private readonly TaskCompletionSource<Nothing> _tcs;

        private readonly Task _task;

        /// <summary>
        /// �����������.
        /// </summary>
        public TableVersionStatus()
        {
            _tcs = new TaskCompletionSource<Nothing>();
            _task = _tcs.Task;
        }

        /// <summary>
        /// ���� ��� ������� ������������� ������� tableversion ��� �����, ���� ������������� ��� ��������.
        /// </summary>
        /// <param name="initFunc">������� �������������.</param>
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
        /// �������� ���� �������������.
        /// </summary>
        public static void ClearInstance()
        {
            Interlocked.Exchange(ref _instance, new TableVersionStatus());
        }
    }
}