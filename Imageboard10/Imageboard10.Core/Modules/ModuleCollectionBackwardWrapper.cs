using System;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Обратная обёртка для коллекции модулей.
    /// </summary>
    /// <typeparam name="T">Тип коллекции.</typeparam>
    internal sealed class ModuleCollectionBackwardWrapper<T> : IModuleCollection
        where T : ModuleInterface.IModuleCollection
    {
        private readonly T _wrapped;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleCollectionBackwardWrapper(T wrapped)
        {
            if (wrapped == null)
            {
                throw new ArgumentNullException(nameof(wrapped));
            }
            _wrapped = wrapped;
        }

        /// <summary>
        /// Зарегистрировать провайдер модулей.
        /// </summary>
        /// <param name="moduleType">Тип модуля. Может быть NULL.</param>
        /// <param name="provider">Провайдер.</param>
        public void RegisterProvider(Type moduleType, IModuleProvider provider)
        {
            _wrapped.RegisterProvider(moduleType, provider.AsWinRT());
        }

        /// <summary>
        /// Можно регистрировать провайдеры.
        /// </summary>
        public bool CanRegisterProviders => _wrapped.CanRegisterProviders;

        /// <summary>
        /// Можно получать провайдеры модулей.
        /// </summary>
        public bool CanGetModuleProvider => _wrapped.CanGetModuleProvider;

        /// <summary>
        /// Получить сводный провайдер модулей.
        /// </summary>
        /// <returns>Провайдер модулей.</returns>
        public IModuleProvider GetModuleProvider()
        {
            return _wrapped.GetModuleProvider().AsDotnet();
        }
    }
}