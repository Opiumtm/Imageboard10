using Imageboard10.Core.Config;

namespace Imageboard10.Makaba.Network.Config
{
    /// <summary>
    /// Сетевая конфигурация Makaba.
    /// </summary>
    public interface IMakabaNetworkConfig : IConfiguration
    {
        /// <summary>
        /// Базовый URI.
        /// </summary>
        System.Uri BaseUri { get; set; }
    }
}