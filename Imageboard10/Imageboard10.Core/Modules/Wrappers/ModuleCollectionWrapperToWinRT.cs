using System;

namespace Imageboard10.Core.Modules.Wrappers
{
    /// <summary>
    /// Обёртка для провайдера модулей.
    /// </summary>
    /// <typeparam name="T">Тип исходного объекта.</typeparam>
    // ReSharper disable once InconsistentNaming
    public class ModuleCollectionWrapperToWinRT<T> : WrapperBase<T>, ModuleInterface.IModuleCollection
        where T : IModuleCollection
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleCollectionWrapperToWinRT(T wrapped) : base(wrapped)
        {
        }

        /// <summary>
        /// Зарегистрировать провайдер модулей.
        /// </summary>
        /// <param name="moduleType">Тип модуля. Может быть NULL.</param>
        /// <param name="provider">Провайдер.</param>
        public void RegisterProvider(Type moduleType, ModuleInterface.IModuleProvider provider)
        {
            Wrapped.RegisterProvider(moduleType, provider.AsDotnetProvider());
        }

        /// <summary>
        /// Можно регистрировать провайдеры.
        /// </summary>
        public bool CanRegisterProviders => Wrapped.CanRegisterProviders;

        /// <summary>
        /// Можно получать провайдеры модулей.
        /// </summary>
        public bool CanGetModuleProvider => Wrapped.CanGetModuleProvider;

        /// <summary>
        /// Получить сводный провайдер модулей.
        /// </summary>
        /// <returns>Провайдер модулей.</returns>
        public ModuleInterface.IModuleProvider GetModuleProvider()
        {
            return Wrapped.GetModuleProvider().AsWinRTProvider();
        }
    }
}