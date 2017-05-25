using System;

namespace Imageboard10.Core.Modules.Wrappers
{
    /// <summary>
    /// Обёртка для модуля.
    /// </summary>
    /// <typeparam name="T">Тип исходного объекта.</typeparam>
    public class ModuleWrapperToDontnet<T> : WrapperBase<T>, IModule
        where T : ModuleInterface.IModule
    {
        /// <summary>
        /// Контруктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleWrapperToDontnet(T wrapped)
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
            if (viewType == typeof(IModuleLifetime))
            {
                return (Wrapped.QueryView(typeof(ModuleInterface.IModuleLifetime)) as ModuleInterface.IModuleLifetime).AsDotnetModuleLifetime();
            }
            if (viewType == typeof(IModuleProvider))
            {
                return (Wrapped.QueryView(typeof(ModuleInterface.IModuleProvider)) as ModuleInterface.IModuleProvider).AsDotnetProvider();
            }
            return Wrapped.QueryView(viewType);
        }

        /// <summary>
        /// Модуль готов к использованию.
        /// </summary>
        public bool IsModuleReady => Wrapped.IsModuleReady;
    }
}