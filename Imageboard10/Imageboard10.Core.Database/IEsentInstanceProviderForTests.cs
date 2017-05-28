using System;
using System.Threading.Tasks;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Информация для юнит-тестов.
    /// </summary>
    public interface IEsentInstanceProviderForTests
    {
        /// <summary>
        /// Получить путь к папке базы данных.
        /// </summary>
        /// <returns>Путь к папке базы данных.</returns>
        Task<string> GetDatabaseFolder();

        /// <summary>
        /// Таймаут последнего завершения.
        /// </summary>
        bool LastShutdownTimeout { get; }

        /// <summary>
        /// Установить таймаут завершения.
        /// </summary>
        /// <param name="timeout">Таймаут.</param>
        void SetShutdownTimeout(TimeSpan timeout);

        /// <summary>
        /// Инстансов создано.
        /// </summary>
        int InstancesCreated { get; }

        /// <summary>
        /// Работа приостановлена.
        /// </summary>
        bool IsSuspended { get; }

        /// <summary>
        /// Остановка запрошена.
        /// </summary>
        bool IsSuspendRequested { get; }
    }
}