using System;

namespace Imageboard10.Core.ModelStorage.UnitTests
{
    /// <summary>
    /// Флаги для юнит-тестирования.
    /// </summary>
    public static class UnitTestStoreFlags
    {
        /// <summary>
        /// Сохранение поста или коллекции в базу должно завершиться с ошибкой.
        /// </summary>
        public static readonly Guid ShouldFail = new Guid("{799892DB-9CDF-488D-A9DE-E393DC5ACE3D}");

        /// <summary>
        /// Всегда добавлять (не обновлять, если найдено с той же ссылкой).
        /// </summary>
        public static readonly Guid AlwaysInsert = new Guid("{388CED0D-C2EB-465C-AF1B-44787C398876}");
    }
}