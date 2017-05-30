using System;
using System.Threading.Tasks;
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
        ValueTask<Nothing> RunInTransaction(Func<bool> logic);

        /// <summary>
        /// ��������� ��� ����������.
        /// </summary>
        /// <param name="logic">������.</param>
        ValueTask<Nothing> Run(Action logic);

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
    }
}