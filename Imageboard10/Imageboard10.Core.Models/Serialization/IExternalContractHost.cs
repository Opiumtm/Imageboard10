namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// ������, ���������� ������� ��������.
    /// </summary>
    public interface IExternalContractHost
    {
        /// <summary>
        /// ������� ��������.
        /// </summary>
        ExternalContractData Contract { get; set; }
    }
}