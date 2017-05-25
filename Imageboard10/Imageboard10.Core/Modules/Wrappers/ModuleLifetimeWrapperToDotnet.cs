using System;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules.Wrappers
{
    /// <summary>
    /// Обёртка времени жизни модуля.
    /// </summary>
    /// <typeparam name="T">Тип исходного объекта.</typeparam>
    public class ModuleLifetimeWrapperToDotnet<T> : WrapperBase<T>, IModuleLifetime
        where T : ModuleInterface.IModuleLifetime
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleLifetimeWrapperToDotnet(T wrapped) : base(wrapped)
        {
        }

        /// <summary>
        /// Инициализировать модуль.
        /// </summary>
        /// <param name="provider">Провайдер модулей.</param>
        public async ValueTask<Nothing> InitializeModule(IModuleProvider provider)
        {
            await Wrapped.InitializeModule(provider.AsWinRTProvider());
            return Nothing.Value;
        }

        /// <summary>
        /// Завершить работу модуля.
        /// </summary>
        public async ValueTask<Nothing> DisposeModule()
        {
            await Wrapped.DisposeModule();
            return Nothing.Value;
        }
    }
}