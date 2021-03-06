using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Metadata;

namespace Imageboard10.Core.ModelInterface
{
    /// <summary>
    /// ������ ������������ (����� ����������).
    /// </summary>
    public interface IObjectSerializationService
    {
        /// <summary>
        /// �������������.
        /// </summary>
        /// <param name="obj">������.</param>
        /// <returns>��������������� ������.</returns>
        string SerializeToString(ISerializableObject obj);

        /// <summary>
        /// �������������.
        /// </summary>
        /// <param name="obj">������.</param>
        /// <returns>��������������� �����.</returns>
        byte[] SerializeToBytes(ISerializableObject obj);

        /// <summary>
        /// ���������������.
        /// </summary>
        /// <param name="data">������.</param>
        /// <returns>������.</returns>
        [DefaultOverload]
        ISerializableObject Deserialize(string data);

        /// <summary>
        /// ���������������.
        /// </summary>
        /// <param name="data">������.</param>
        /// <returns>������.</returns>
        ISerializableObject Deserialize([ReadOnlyArray] byte[] data);

        /// <summary>
        /// ����� ������������.
        /// </summary>
        /// <param name="type">���.</param>
        /// <returns>������������.</returns>
        IObjectSerializer FindSerializer(Type type);

        /// <summary>
        /// ����� ������������.
        /// </summary>
        /// <param name="typeId">������������� ����.</param>
        /// <returns>������������.</returns>
        [DefaultOverload]
        IObjectSerializer FindSerializer(string typeId);
    }
}