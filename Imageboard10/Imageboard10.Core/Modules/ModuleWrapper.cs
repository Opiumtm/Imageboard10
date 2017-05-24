using System;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Обёртка для модуля.
    /// </summary>
    /// <typeparam name="T">Тип модуля.</typeparam>
    internal sealed class ModuleWrapper<T> : IModule
        where T : ModuleInterface.IModule
    {
        private readonly T _wrapped;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleWrapper(T wrapped)
        {
            if (wrapped == null) throw new ArgumentNullException(nameof(wrapped));
            _wrapped = wrapped;
        }

        /// <summary>
        /// Инициализировать модуль.
        /// </summary>
        /// <param name="provider">Провайдер модулей.</param>
        public async ValueTask<Nothing> InitializeModule(IModuleProvider provider)
        {
            await _wrapped.InitializeModule(provider.AsWinRT());
            return Nothing.Value;
        }

        /// <summary>
        /// Завершить работу модуля.
        /// </summary>
        public async ValueTask<Nothing> DisposeModule()
        {
            await _wrapped.DisposeModule();
            return Nothing.Value;
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