using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Boards;

namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Результат выполнения операции.
    /// </summary>
    public interface IBoardsListResult
    {
        IList<IBoardReference> Boards { get; }
    }
}