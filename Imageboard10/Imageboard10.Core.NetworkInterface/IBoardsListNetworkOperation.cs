using System.Threading;
using Windows.Foundation;
using Imageboard10.ModuleInterface;

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
        IAsyncOperationWithProgress<IBoardsListResult, OperationProgress> Run();
    }
}