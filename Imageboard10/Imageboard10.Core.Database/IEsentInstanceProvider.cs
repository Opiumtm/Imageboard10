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
        /// Получить вторичную сессию.
        /// </summary>
        /// <returns>Экземпляр.</returns>
        ValueTask<IEsentSession> GetSecondarySession();

      
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