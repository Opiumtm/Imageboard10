using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Imageboard10.Core.Modules.Wrappers
{
    /// <summary>
    /// Обёртка времени жизни модуля.
    /// </summary>
    /// <typeparam name="T">Тип исходного объекта.</typeparam>
    public class ModuleLifetimeWrapperToWinRt<T> : WrapperBase<T>, ModuleInterface.IModuleLifetime
        where T : IModuleLifetime
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleLifetimeWrapperToWinRt(T wrapped) : base(wrapped)
        {
        }

        /// <summary>
        /// Инициализировать модуль.
        /// </summary>
        /// <param name="provider">Провайдер модулей.</param>
        public IAsyncAction InitializeModule(ModuleInterface.IModuleProvider provider)
        {
            async Task Do()
            {
                await Wrapped.InitializeModule(provider.AsDotnetProvider());
            }

            return Do().AsAsyncAction();
        }

        /// <summary>
        /// Завершить работу модуля.
        /// </summary>
        /// <returns></returns>
        public IAsyncAction DisposeModule()
        {
            async Task Do()
            {
                await Wrapped.DisposeModule();
            }

            return Do().AsAsyncAction();
        }
    }
}