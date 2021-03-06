using System.Threading.Tasks;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// �������������� ��������� ������������.
    /// </summary>
    /// <typeparam name="T">��� �������.</typeparam>
    public interface IObjectSerializerCustomization<T>
        where T : class, ISerializableObject, new()
    {
        /// <summary>
        /// ��������� �������� ����� �������������.
        /// </summary>
        /// <param name="obj">�������� ������.</param>
        /// <returns>����������� ������.</returns>
        T ValidateContract(T obj);

        /// <summary>
        /// ��������� �������� ����� ������������.
        /// </summary>
        /// <param name="obj">�������� ������.</param>
        /// <returns>����������� ������.</returns>
        T ValidateAfterDeserialize(T obj);

        /// <summary>
        /// ���������������� ������.
        /// </summary>
        /// <param name="modules">������.</param>
        ValueTask<Nothing> Initialize(IModuleProvider modules);
    }
}