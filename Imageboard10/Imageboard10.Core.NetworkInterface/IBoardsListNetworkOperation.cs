using System.Threading;
using Windows.Foundation;

namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Операция получения списка доступных досок.
    /// </summary>
    public interface IBoardsListNetworkOperation
    {
        /// <summary>
        /// Выполнить операцию.
        /// </summary>
        /// <returns>Результат.</returns>
        IAsyncOperationWithProgress<IBoardsListResult, NetworkProgress> Run();
    }
}