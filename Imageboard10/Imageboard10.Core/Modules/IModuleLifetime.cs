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

        /// <summary>
        /// ������������� ������ ������.
        /// </summary>
        ValueTask<Nothing> SuspendModule();

        /// <summary>
        /// ����������� ������ ������.
        /// </summary>
        ValueTask<Nothing> ResumeModule();

        /// <summary>
        /// ��� ������ ������������.
        /// </summary>
        ValueTask<Nothing> AllModulesResumed();

        /// <summary>
        /// ��� ������ ����������������.
        /// </summary>
        ValueTask<Nothing> AllModulesInitialized();

        /// <summary>
        /// ������������ ������������ � ��������������.
        /// </summary>
        bool IsSuspendAware { get; }
    }
}