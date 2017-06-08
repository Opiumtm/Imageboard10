using Imageboard10.Core.ModelInterface;

namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// Объект, содержащий внешний контракт.
    /// </summary>
    public interface IExternalContractHost : ISerializableObject
    {
        /// <summary>
        /// Внешний контракт.
        /// </summary>
        ExternalContractData Contract { get; set; }
    }
}