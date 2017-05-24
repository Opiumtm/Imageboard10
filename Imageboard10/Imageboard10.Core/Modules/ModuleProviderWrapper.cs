using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Обёртка для провайдера модулей.
    /// </summary>
    /// <typeparam name="T">Тип модуля.</typeparam>
    internal sealed class ModuleProviderWrapper<T> : ModuleInterface.IModuleProvider
        where T : IModuleProvider
    {
        private readonly T _wrapped;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleProviderWrapper(T wrapped)
        {
            if (wrapped == null)
            {
                throw new ArgumentNullException(nameof(wrapped));
            }
            _wrapped = wrapped;
        }

        /// <summary>
        /// Запросить модуль асинхронно.
        /// </summary>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public IAsyncOperation<ModuleInterface.IModule> QueryModuleAsync(Type moduleType, PropertySet query)
        {
            async Task<ModuleInterface.IModule> DoQueryModuleAsync()
            {
                return (await _wrapped.QueryModuleAsync(moduleType, query)).AsWinRTModule();
            }

            return DoQueryModuleAsync().AsAsyncOperation();
        }

        /// <summary>
        /// Запросить модуль.
        /// </summary>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public ModuleInterface.IModule QueryModule(Type moduleType, PropertySet query)
        {
            return _wrapped.QueryModule(moduleType, query).AsWinRTModule();
        }
    }
}