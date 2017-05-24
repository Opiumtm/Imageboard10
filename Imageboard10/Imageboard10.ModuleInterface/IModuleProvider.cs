using System;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Imageboard10.ModuleInterface
{
    /// <summary>
    /// Провайдер модулей.
    /// </summary>
    public interface IModuleProvider
    {
        /// <summary>
        /// Запросить модуль асинхронно.
        /// </summary>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        IAsyncOperation<IModule> QueryModuleAsync(Type moduleType, PropertySet query);

        /// <summary>
        /// Запросить модуль.
        /// </summary>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        IModule QueryModule(Type moduleType, PropertySet query);
    }
}