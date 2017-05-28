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
        Task RunInTransaction(Func<bool> logic);

        /// <summary>
        /// Выполнить вне транзакции.
        /// </summary>
        /// <param name="logic">Логика.</param>
        Task Run(Action logic);

        /// <summary>
        /// Использовать сессию. Гарантирует, что основная сессия не будет автоматически завершена, если используется хотя бы одна из активных сессий.
        /// </summary>
        /// <returns>Сигнализация о завершении использования.</returns>
        IDisposable UseSession();
    }
}