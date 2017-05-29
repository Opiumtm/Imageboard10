namespace Imageboard10.Core.Network
{
    /// <summary>
    /// Парсер объектов передачи данных.
    /// </summary>
    /// <typeparam name="TIn">Тип входных данных.</typeparam>
    /// <typeparam name="TOut">Тип результата.</typeparam>
    public interface INetworkDtoParser<in TIn, out TOut>
    {
        /// <summary>
        /// Распарсить.
        /// </summary>
        /// <param name="source">Источник.</param>
        /// <returns>Результат.</returns>
        TOut Parse(TIn source);
    }
}