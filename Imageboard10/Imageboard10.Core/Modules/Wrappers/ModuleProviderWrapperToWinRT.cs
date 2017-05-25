using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Imageboard10.Core.Modules.Wrappers
{
    /// <summary>
    /// Обёртка для провайдера модулей.
    /// </summary>
    /// <typeparam name="T">Тип исходного объекта.</typeparam>
    // ReSharper disable once InconsistentNaming
    public class ModuleProviderWrapperToWinRT<T> : ModuleWrapperToWinRT<T>, ModuleInterface.IModuleProvider
        where T : IModuleProvider
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleProviderWrapperToWinRT(T wrapped) : base(wrapped)
        {
            _wrappedParent = new Lazy<ModuleInterface.IModuleProvider>(() => Wrapped.Parent.AsWinRTProvider());
        }

        private readonly Lazy<ModuleInterface.IModuleProvider> _wrappedParent;

        /// <summary>
        /// Родительский провайдер модулей.
        /// </summary>
        public ModuleInterface.IModuleProvider Parent => _wrappedParent.Value;

        /// <summary>
        /// Запросить модуль асинхронно.
        /// </summary>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public IAsyncOperation<ModuleInterface.IModule> QueryModuleAsync(Type moduleType, PropertySet query)
        {
            async Task<ModuleInterface.IModule> Do()
            {
                return (await Wrapped.QueryModuleAsync(moduleType, query)).AsWinRTModule();
            }

            return Do().AsAsyncOperation();
        }

        /// <summary>
        /// Запросить модуль.
        /// </summary>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public ModuleInterface.IModule QueryModule(Type moduleType, PropertySet query)
        {
            return Wrapped.QueryModule(moduleType, query).AsWinRTModule();
        }
    }
}