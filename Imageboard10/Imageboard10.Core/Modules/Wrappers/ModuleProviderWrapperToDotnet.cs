using System;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Imageboard10.Core.Modules.Wrappers
{
    /// <summary>
    /// Обёртка для провайдера модулей.
    /// </summary>
    /// <typeparam name="T">Тип исходного объекта.</typeparam>
    public class ModuleProviderWrapperToDotnet<T> : ModuleWrapperToDontnet<T>, IModuleProvider
        where T : ModuleInterface.IModuleProvider
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleProviderWrapperToDotnet(T wrapped) : base(wrapped)
        {
            _wrappedParent = new Lazy<IModuleProvider>(() => Wrapped.Parent.AsDotnetProvider());
        }

        private readonly Lazy<IModuleProvider> _wrappedParent;

        /// <summary>
        /// Родительский провайдер модулей.
        /// </summary>
        public IModuleProvider Parent => _wrappedParent.Value;

        /// <summary>
        /// Запросить модуль асинхронно.
        /// </summary>
        /// <typeparam name="T1">Тип запроса. Должен поддерзживаться тип PropertySet, если предполагается запрос из WinRT-среды.</typeparam>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public async ValueTask<IModule> QueryModuleAsync<T1>(Type moduleType, T1 query)
        {
            return (await Wrapped.QueryModuleAsync(moduleType, WrapQuery(query))).AsDotnetModule();
        }

        /// <summary>
        /// Запросить модуль.
        /// </summary>
        /// <typeparam name="T1">Тип запроса. Должен поддерзживаться тип PropertySet, если предполагается запрос из WinRT-среды.</typeparam>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public IModule QueryModule<T1>(Type moduleType, T1 query)
        {
            return Wrapped.QueryModule(moduleType, WrapQuery(query)).AsDotnetModule();
        }

        private PropertySet WrapQuery<T1>(T1 query)
        {
            if (query == null)
            {
                return null;
            }
            if (query is IPropertySetConvertable c)
            {
                return c.AsPropertySet();
            }
            return new PropertySet
            {
                {"_", query}
            };
        }
    }
}