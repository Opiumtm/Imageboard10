using Imageboard10.ModuleInterface;

namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Операция сетевого движка.
    /// </summary>
    public interface INetworkEngineCapability : IModule
    {
        /// <summary>
        /// Идентификатор движка.
        /// </summary>
        string EngineId { get; }
    }
}