using System;
using System.Threading.Tasks;
using Imageboard10.Core.Tasks;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// ������ ESENT.
    /// </summary>
    public interface IEsentSession
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
        /// ��������� ������.
        /// </summary>
        bool IsSecondarySession { get; }

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
        /// <param name="retrySecs">���������� ������ ��� �������� �������� ������� ���������� ��� ����������. null - �� ������ ��������� �������.</param>
        /// <param name="grbit">����� �������.</param>
        ValueTask<Nothing> RunInTransaction(Func<bool> logic, double? retrySecs = null, CommitTransactionGrbit grbit = CommitTransactionGrbit.None);

        /// <summary>
        /// ��������� � ����������.
        /// </summary>
        /// <param name="logic">������. ���������� true, ���� ���������� ����� ���������.</param>
        /// <param name="retrySecs">���������� ������ ��� �������� �������� ������� ���������� ��� ����������. null - �� ������ ��������� �������.</param>
        /// <param name="grbit">����� �������.</param>
        ValueTask<T> RunInTransaction<T>(Func<(bool commit, T result)> logic, double? retrySecs = null, CommitTransactionGrbit grbit = CommitTransactionGrbit.None);

        /// <summary>
        /// ��������� ��� ����������.
        /// </summary>
        /// <param name="logic">������.</param>
        ValueTask<Nothing> Run(Action logic);

        /// <summary>
        /// ��������� ��� ����������.
        /// </summary>
        /// <param name="logic">������.</param>
        ValueTask<T> Run<T>(Func<T> logic);

        /// <summary>
        /// ������������ ������. �����������, ��� �������� ������ �� ����� ������������� ���������, ���� ������������ ���� �� ���� �� �������� ������.
        /// </summary>
        /// <returns>������������ � ���������� �������������.</returns>
        IDisposable UseSession();

        /// <summary>
        /// ������� �������.
        /// </summary>
        /// <param name="tableName">��� �������.</param>
        /// <param name="grbit">����.</param>
        /// <returns>�������.</returns>
        EsentTable OpenTable(string tableName, OpenTableGrbit grbit);

        /// <summary>
        /// ������� ������� ���������� (����������� ���������� �� ������ ������).
        /// </summary>
        /// <param name="tableName">��� �������.</param>
        /// <param name="grbit">��� �������.</param>
        /// <returns></returns>
        ValueTask<ThreadDisposableAccessGuard<EsentTable>> OpenTableAsync(string tableName, OpenTableGrbit grbit);
    }
}