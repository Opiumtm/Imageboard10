using System;
using System.Threading.Tasks;
using Imageboard10.Core.Tasks;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Сессия ESENT.
    /// </summary>
    public interface IEsentSession
    {
        /// <summary>
        /// Экземпляр.
        /// </summary>
        Instance Instance { get; }

        /// <summary>
        /// Файл базы данных.
        /// </summary>
        string DatabaseFile { get; }

        /// <summary>
        /// Вторичная сессия.
        /// </summary>
        bool IsSecondarySession { get; }

        /// <summary>
        /// Сессия.
        /// </summary>
        Session Session { get; }

        /// <summary>
        /// База данных.
        /// </summary>
        JET_DBID Database { get; }

        /// <summary>
        /// Выполнить в транзакции.
        /// </summary>
        /// <param name="logic">Логика. Возвращает true, если транзакцию нужно завершить.</param>
        /// <param name="retrySecs">Количество секунд при попытках повторно сделать транзакцию при конфликтах. null - не делать повторных попыток.</param>
        /// <param name="grbit">Флаги коммита.</param>
        ValueTask<Nothing> RunInTransaction(Func<bool> logic, double? retrySecs = null, CommitTransactionGrbit grbit = CommitTransactionGrbit.None);

        /// <summary>
        /// Выполнить в транзакции.
        /// </summary>
        /// <param name="logic">Логика. Возвращает true, если транзакцию нужно завершить.</param>
        /// <param name="retrySecs">Количество секунд при попытках повторно сделать транзакцию при конфликтах. null - не делать повторных попыток.</param>
        /// <param name="grbit">Флаги коммита.</param>
        ValueTask<T> RunInTransaction<T>(Func<(bool commit, T result)> logic, double? retrySecs = null, CommitTransactionGrbit grbit = CommitTransactionGrbit.None);

        /// <summary>
        /// Выполнить вне транзакции.
        /// </summary>
        /// <param name="logic">Логика.</param>
        ValueTask<Nothing> Run(Action logic);

        /// <summary>
        /// Выполнить вне транзакции.
        /// </summary>
        /// <param name="logic">Логика.</param>
        ValueTask<T> Run<T>(Func<T> logic);

        /// <summary>
        /// Использовать сессию. Гарантирует, что основная сессия не будет автоматически завершена, если используется хотя бы одна из активных сессий.
        /// </summary>
        /// <returns>Сигнализация о завершении использования.</returns>
        IDisposable UseSession();

        /// <summary>
        /// Открыть таблицу.
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        /// <param name="grbit">Биты.</param>
        /// <returns>Таблица.</returns>
        EsentTable OpenTable(string tableName, OpenTableGrbit grbit);

        /// <summary>
        /// Открыть таблицу асинхронно (гарантирует выполнение на нужном потоке).
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        /// <param name="grbit">Имя таблицы.</param>
        /// <returns></returns>
        ValueTask<ThreadDisposableAccessGuard<EsentTable>> OpenTableAsync(string tableName, OpenTableGrbit grbit);
    }
}