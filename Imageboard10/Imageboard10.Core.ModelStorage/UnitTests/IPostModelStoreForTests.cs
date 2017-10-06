using System.Threading.Tasks;

namespace Imageboard10.Core.ModelStorage.UnitTests
{
    /// <summary>
    /// Интерфейс для юнит-тестирования.
    /// </summary>
    public interface IPostModelStoreForTests : IModelStorageForTests
    {
        /// <summary>
        /// Получить размеры таблиц (в количествах записей).
        /// </summary>
        /// <returns>Размеры таблиц.</returns>
        Task<(string name, int count)[]> GetTableSizes();
    }
}