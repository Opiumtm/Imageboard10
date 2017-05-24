using System;
using Windows.Foundation;

namespace Imageboard10.ModuleInterface
{
    /// <summary>
    /// Модуль.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Инициализировать модуль.
        /// </summary>
        /// <param name="provider">Провайдер модулей.</param>
        IAsyncAction InitializeModule(IModuleProvider provider);

        /// <summary>
        /// Завершить работу модуля.
        /// </summary>
        /// <returns></returns>
        IAsyncAction DisposeModule();

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        object QueryView(Type viewType);

        /// <summary>
        /// Модуль готов к использованию.
        /// </summary>
        bool IsModuleReady { get; }
    }
}