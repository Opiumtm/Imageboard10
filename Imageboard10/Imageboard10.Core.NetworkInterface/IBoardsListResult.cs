using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Boards;

namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// ��������� ���������� ��������.
    /// </summary>
    public interface IBoardsListResult
    {
        IList<IBoardReference> Boards { get; }
    }
}