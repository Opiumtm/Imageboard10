using System;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Провайдер модулей.
    /// </summary>
    public interface IModuleProvider : IModule
    {
        /// <summary>
        /// Родительский провайдер модулей.
        /// </summary>
        IModuleProvider Parent { get; }

        /// <summary>
        /// Запросить модуль асинхронно.
        /// </summary>
        /// <typeparam name="T">Тип запроса. Должен поддерзживаться тип PropertySet, если предполагается запрос из WinRT-среды.</typeparam>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        ValueTask<IModule> QueryModuleAsync<T>(Type moduleType, T query);

        /// <summary>
        /// Запросить модуль.
        /// </summary>
        /// <typeparam name="T">Тип запроса. Должен поддерзживаться тип PropertySet, если предполагается запрос из WinRT-среды.</typeparam>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        IModule QueryModule<T>(Type moduleType, T query);
    }
}