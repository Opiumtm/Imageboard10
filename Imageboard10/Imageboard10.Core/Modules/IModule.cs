using System;
using System.Threading;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
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
        ValueTask<Nothing> InitializeModule(IModuleProvider provider);

        /// <summary>
        /// Завершить работу модуля.
        /// </summary>
        ValueTask<Nothing> DisposeModule();

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