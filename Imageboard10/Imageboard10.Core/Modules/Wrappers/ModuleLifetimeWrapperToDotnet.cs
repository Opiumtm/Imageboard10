using System;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules.Wrappers
{
    /// <summary>
    /// ������ ������� ����� ������.
    /// </summary>
    /// <typeparam name="T">��� ��������� �������.</typeparam>
    public class ModuleLifetimeWrapperToDotnet<T> : WrapperBase<T>, IModuleLifetime
        where T : ModuleInterface.IModuleLifetime
    {
        /// <summary>
        /// �����������.
        /// </summary>
        /// <param name="wrapped">�������� ������.</param>
        public ModuleLifetimeWrapperToDotnet(T wrapped) : base(wrapped)
        {
        }

        /// <summary>
        /// ���������������� ������.
        /// </summary>
        /// <param name="provider">��������� �������.</param>
        public async ValueTask<Nothing> InitializeModule(IModuleProvider provider)
        {
            await Wrapped.InitializeModule(provider.AsWinRTProvider());
            return Nothing.Value;
        }

        /// <summary>
        /// ��������� ������ ������.
        /// </summary>
        public async ValueTask<Nothing> DisposeModule()
        {
            await Wrapped.DisposeModule();
            return Nothing.Value;
        }
    }
}