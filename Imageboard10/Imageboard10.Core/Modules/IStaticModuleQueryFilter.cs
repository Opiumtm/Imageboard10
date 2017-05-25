namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Фильтр для статического модуля.
    /// </summary>
    public interface IStaticModuleQueryFilter
    {
        /// <summary>
        /// Проверить запрос.
        /// </summary>
        /// <typeparam name="T">Тип запроса.</typeparam>
        /// <param name="query">Запрос.</param>
        /// <returns>Результат.</returns>
        bool CheckQuery<T>(T query);
    }
}