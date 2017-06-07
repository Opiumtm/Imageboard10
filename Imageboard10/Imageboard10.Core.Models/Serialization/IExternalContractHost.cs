namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// Объект, содержащий внешний контракт.
    /// </summary>
    public interface IExternalContractHost
    {
        /// <summary>
        /// Внешний контракт.
        /// </summary>
        ExternalContractData Contract { get; set; }
    }
}