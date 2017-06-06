using System.Threading.Tasks;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Провайдер экземпляров ESENT.
    /// </summary>
    public interface IEsentInstanceProvider
    {
        /// <summary>
        /// Основная сессия. Не вызывать Dispose(), т.к. временем жизни основной сессии управляет провайдер.
        /// </summary>
        IEsentSession MainSession { get; }

        /// <summary>
        /// Получить сессию только для чтения.
        /// </summary>
        /// <returns>Экземпляр.</returns>
        ValueTask<IEsentSession> GetReadOnlySession();

      
        /// <summary>
        /// Путь к базе данных.
        /// </summary>
        string DatabasePath { get; }

        /// <summary>
        /// Очистить при старте.
        /// </summary>
        bool Purge { get; }
    }
}