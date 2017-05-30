namespace Imageboard10.Core.ModelStorage.UnitTests
{
    /// <summary>
    /// Интерфейс для юнит-тестирования.
    /// </summary>
    public interface IBoardReferenceStoreForTests : IModelStorageForTests
    {
        /// <summary>
        /// Имя таблицы с досками.
        /// </summary>
        string BoardsTableName { get; }
    }
}