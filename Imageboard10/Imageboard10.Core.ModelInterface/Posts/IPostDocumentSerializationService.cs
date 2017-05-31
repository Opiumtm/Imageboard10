using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Metadata;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// ������ ������������ ���� ��������� �����.
    /// </summary>
    public interface IPostDocumentSerializationService
    {
        /// <summary>
        /// �������������.
        /// </summary>
        /// <param name="document">��������.</param>
        /// <returns>��������������� ��������.</returns>
        string SerializeToString(IPostDocument document);

        /// <summary>
        /// �������������.
        /// </summary>
        /// <param name="document">��������.</param>
        /// <returns>��������������� ��������.</returns>
        byte[] SerializeToBytes(IPostDocument document);

        /// <summary>
        /// ���������������.
        /// </summary>
        /// <param name="data">������.</param>
        /// <returns>��������.</returns>
        [DefaultOverload]
        IPostDocument Deserialize(string data);

        /// <summary>
        /// ���������������.
        /// </summary>
        /// <param name="data">������.</param>
        /// <returns>��������.</returns>
        IPostDocument Deserialize([ReadOnlyArray] byte[] data);
    }
}