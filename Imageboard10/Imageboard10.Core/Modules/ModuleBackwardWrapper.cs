using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Обратная обёртка для модуля.
    /// </summary>
    /// <typeparam name="T">Тип модуля.</typeparam>
    internal sealed class ModuleBackwardWrapper<T> : ModuleInterface.IModule
        where T : IModule
    {
        private readonly T _wrapped;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleBackwardWrapper(T wrapped)
        {
            if (wrapped == null) throw new ArgumentNullException(nameof(wrapped));
            _wrapped = wrapped;
        }

        /// <summary>
        /// Инициализировать модуль.
        /// </summary>
        /// <param name="provider">Провайдер модулей.</param>
        public IAsyncAction InitializeModule(ModuleInterface.IModuleProvider provider)
        {
            async Task DoInitializeModule()
            {
                await _wrapped.InitializeModule(provider.AsDotnet());
            }

            return DoInitializeModule().AsAsyncAction();
        }

        /// <summary>
        /// Завершить работу модуля.
        /// </summary>
        /// <returns></returns>
        public IAsyncAction DisposeModule()
        {
            async Task DoDisposeModule()
            {
                await _wrapped.DisposeModule();
            }

            return DoDisposeModule().AsAsyncAction();
        }

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        public object QueryView(Type viewType)
        {
            return _wrapped.QueryView(viewType);
        }

        /// <summary>
        /// Модуль готов к использованию.
        /// </summary>
        public bool IsModuleReady => _wrapped.IsModuleReady;
    }
}