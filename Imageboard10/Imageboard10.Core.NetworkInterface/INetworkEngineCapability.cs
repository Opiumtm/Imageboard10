namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Операция сетевого движка.
    /// </summary>
    public interface INetworkEngineCapability
    {
        /// <summary>
        /// Идентификатор движка.
        /// </summary>
        string EngineId { get; }
    }
}