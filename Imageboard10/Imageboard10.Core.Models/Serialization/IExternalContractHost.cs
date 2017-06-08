using Imageboard10.Core.ModelInterface;

namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// ������, ���������� ������� ��������.
    /// </summary>
    public interface IExternalContractHost : ISerializableObject
    {
        /// <summary>
        /// ������� ��������.
        /// </summary>
        ExternalContractData Contract { get; set; }
    }
}