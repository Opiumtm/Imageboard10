using System;
using System.Threading.Tasks;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// ������ ESENT.
    /// </summary>
    public interface IEsentSession : IDisposable
    {
        /// <summary>
        /// ���������.
        /// </summary>
        Instance Instance { get; }

        /// <summary>
        /// ���� ���� ������.
        /// </summary>
        string DatabaseFile { get; }

        /// <summary>
        /// ������ ��� ������.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// ������.
        /// </summary>
        Session Session { get; }

        /// <summary>
        /// ���� ������.
        /// </summary>
        JET_DBID Database { get; }

        /// <summary>
        /// ��������� � ����������.
        /// </summary>
        /// <param name="logic">������. ���������� true, ���� ���������� ����� ���������.</param>
        Task RunInTransaction(Func<bool> logic);

        /// <summary>
        /// ��������� ��� ����������.
        /// </summary>
        /// <param name="logic">������.</param>
        Task Run(Action logic);

        /// <summary>
        /// ������������ ������. �����������, ��� �������� ������ �� ����� ������������� ���������, ���� ������������ ���� �� ���� �� �������� ������.
        /// </summary>
        /// <returns>������������ � ���������� �������������.</returns>
        IDisposable UseSession();
    }
}