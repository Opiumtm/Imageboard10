using System.Threading.Tasks;

namespace Imageboard10.Core.ModelStorage.UnitTests
{
    /// <summary>
    /// Интерфейс хранилища для юнит-тестов.
    /// </summary>
    public interface IModelStorageForTests
    {
        /// <summary>
        /// Проверить существование таблицы.
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        /// <returns>Результат.</returns>
        Task<bool> IsTablePresent(string tableName);

        /// <summary>
        /// Получить версию таблицы.
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        /// <returns>Версия.</returns>
        Task<int> GetTableVersion(string tableName);

        /// <summary>
        /// Имя таблицы с версиями.
        /// </summary>
        string TableversionTableName { get; }

        /// <summary>
        /// Ожидать инициализации.
        /// </summary>
        /// <returns>Результат.</returns>
        Task WaitForInitialization();
    }
}