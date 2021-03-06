using System.Threading.Tasks;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// ����������� ������������� ������ ESENT.
    /// </summary>
    internal interface IDisposeWaiters
    {
        /// <summary>
        /// ���������������� �������������.
        /// </summary>
        /// <param name="task">����.</param>
        void RegisterWaiter(Task task);

        /// <summary>
        /// ������� �������������.
        /// </summary>
        /// <param name="task">����.</param>
        void RemoveWaiter(Task task);

        /// <summary>
        /// ������ ���������� �� ���������� ������ �������������.
        /// </summary>
        object UsingLock { get; }
    }
}