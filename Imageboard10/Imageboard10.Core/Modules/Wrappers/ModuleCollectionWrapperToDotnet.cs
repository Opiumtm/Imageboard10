using System;

namespace Imageboard10.Core.Modules.Wrappers
{
    /// <summary>
    /// Обёртка для провайдера модулей.
    /// </summary>
    /// <typeparam name="T">Тип исходного объекта.</typeparam>
    public class ModuleCollectionWrapperToDotnet<T> : WrapperBase<T>, IModuleCollection
        where T : ModuleInterface.IModuleCollection
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleCollectionWrapperToDotnet(T wrapped) : base(wrapped)
        {
        }

        /// <summary>
        /// Зарегистрировать провайдер модулей.
        /// </summary>
        /// <param name="moduleType">Тип модуля. Может быть NULL.</param>
        /// <param name="provider">Провайдер.</param>
        public void RegisterProvider(Type moduleType, IModuleProvider provider)
        {
            Wrapped.RegisterProvider(moduleType, provider.AsWinRTProvider());
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
        public IModuleProvider GetModuleProvider()
        {
            return Wrapped.GetModuleProvider().AsDotnetProvider();
        }
    }
}