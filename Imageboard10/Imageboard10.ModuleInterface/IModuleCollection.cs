using System;

namespace Imageboard10.ModuleInterface
{
    /// <summary>
    /// Коллекция модулей.
    /// </summary>
    public interface IModuleCollection
    {
        /// <summary>
        /// Зарегистрировать провайдер модулей.
        /// </summary>
        /// <param name="moduleType">Тип модуля. Может быть NULL.</param>
        /// <param name="provider">Провайдер.</param>
        void RegisterProvider(Type moduleType, IModuleProvider provider);

        /// <summary>
        /// Можно регистрировать провайдеры.
        /// </summary>
        bool CanRegisterProviders { get; }

        /// <summary>
        /// Можно получать провайдеры модулей.
        /// </summary>
        bool CanGetModuleProvider { get; }

        /// <summary>
        /// Получить сводный провайдер модулей.
        /// </summary>
        /// <returns>Провайдер модулей.</returns>
        IModuleProvider GetModuleProvider();
    }
}