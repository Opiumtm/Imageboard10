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

        /// <summary>
        /// Приостановить работу модуля.
        /// </summary>
        public async ValueTask<Nothing> SuspendModule()
        {
            await Wrapped.SuspendModule();
            return Nothing.Value;
        }

        /// <summary>
        /// Возобновить работу модуля.
        /// </summary>
        public async ValueTask<Nothing> ResumeModule()
        {
            await Wrapped.ResumeModule();
            return Nothing.Value;
        }

        /// <summary>
        /// Все модули возобновлены.
        /// </summary>
        public async ValueTask<Nothing> AllModulesResumed()
        {
            await Wrapped.AllModulesResumed();
            return Nothing.Value;
        }

        /// <summary>
        /// Все модули инициализированы.
        /// </summary>
        public async ValueTask<Nothing> AllModulesInitialized()
        {
            await Wrapped.AllModulesInitialized();
            return Nothing.Value;
        }

        /// <summary>
        /// Поддерживает приостановку и восстановление.
        /// </summary>
        public bool IsSuspendAware => Wrapped.IsSuspendAware;
    }
}