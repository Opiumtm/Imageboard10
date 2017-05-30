using System;
using System.Threading.Tasks;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Сессия ESENT.
    /// </summary>
    public interface IEsentSession : IDisposable
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
        /// Только для чтения.
        /// </summary>
        bool IsReadOnly { get; }

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
        ValueTask<Nothing> RunInTransaction(Func<bool> logic);

        /// <summary>
        /// Выполнить вне транзакции.
        /// </summary>
        /// <param name="logic">Логика.</param>
        ValueTask<Nothing> Run(Action logic);

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
        /// Асинхронное завершение работы.
        /// </summary>
        /// <returns>Результат.</returns>
        Task DisposeAsync();
    }
}