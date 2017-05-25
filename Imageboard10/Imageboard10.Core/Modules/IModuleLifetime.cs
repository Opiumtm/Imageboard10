using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// ���������� �������� ����� ������.
    /// </summary>
    public interface IModuleLifetime
    {
        /// <summary>
        /// ���������������� ������.
        /// </summary>
        /// <param name="provider">��������� �������.</param>
        ValueTask<Nothing> InitializeModule(IModuleProvider provider);

        /// <summary>
        /// ��������� ������ ������.
        /// </summary>
        ValueTask<Nothing> DisposeModule();
    }
}