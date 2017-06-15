namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Запросы к возможности движка.
    /// </summary>
    public static class EngineCapabilityQueries
    {
        /// <summary>
        /// Общие возможности, не зависящие от движка.
        /// </summary>
        public static EngineCapabilityQuery Common { get; } = new EngineCapabilityQuery() { EngineId = ""};
    }
}