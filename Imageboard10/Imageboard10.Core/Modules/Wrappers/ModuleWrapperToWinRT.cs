using System;

namespace Imageboard10.Core.Modules.Wrappers
{
    /// <summary>
    /// Обёртка для модуля.
    /// </summary>
    /// <typeparam name="T">Тип исходного объекта.</typeparam>
    // ReSharper disable once InconsistentNaming
    public class ModuleWrapperToWinRT<T> : WrapperBase<T>, ModuleInterface.IModule
        where T : IModule
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleWrapperToWinRT(T wrapped)
            : base(wrapped)
        {
        }

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        public object QueryView(Type viewType)
        {
            if (viewType == typeof(ModuleInterface.IModuleLifetime))
            {
                return (Wrapped.QueryView(typeof(IModuleLifetime)) as IModuleLifetime).AsWinRTModuleLifetime();
            }
            if (viewType == typeof(ModuleInterface.IModuleProvider))
            {
                return (Wrapped.QueryView(typeof(IModuleProvider)) as IModuleProvider).AsWinRTProvider();
            }
            return Wrapped.QueryView(viewType);
        }

        /// <summary>
        /// Модуль готов к использованию.
        /// </summary>
        public bool IsModuleReady => Wrapped.IsModuleReady;
    }
}