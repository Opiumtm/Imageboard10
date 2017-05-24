using System;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Обратное обёртка провайдера модулей.
    /// </summary>
    /// <typeparam name="TSrc">Тип модуля.</typeparam>
    internal sealed class ModuleProviderBackwardWrapper<TSrc> : IModuleProvider
        where TSrc : ModuleInterface.IModuleProvider
    {
        private readonly TSrc _wrapped;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="wrapped">Исходный объект.</param>
        public ModuleProviderBackwardWrapper(TSrc wrapped)
        {
            if (wrapped == null)
            {
                throw new ArgumentNullException(nameof(wrapped));
            }
            _wrapped = wrapped;
        }

        /// <summary>
        /// Запросить модуль асинхронно.
        /// </summary>
        /// <typeparam name="T">Тип запроса. Должен поддерзживаться тип PropertySet, если предполагается запрос из WinRT-среды.</typeparam>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public async ValueTask<IModule> QueryModuleAsync<T>(Type moduleType, T query)
        {
            return (await _wrapped.QueryModuleAsync(moduleType, GetPropertySet(query))).AsDotnetModule();
        }

        /// <summary>
        /// Запросить модуль.
        /// </summary>
        /// <typeparam name="T">Тип запроса. Должен поддерзживаться тип PropertySet, если предполагается запрос из WinRT-среды.</typeparam>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public IModule QueryModule<T>(Type moduleType, T query)
        {
            return _wrapped.QueryModule(moduleType, GetPropertySet(query)).AsDotnetModule();
        }

        private PropertySet GetPropertySet<T>(T query)
        {
            if (query == null)
            {
                return null;
            }
            var convertable = query as IPropertySetConvertable;
            if (convertable != null)
            {
                return convertable.AsPropertySet();
            }
            return new PropertySet
            {
                { "_", query }
            };
        }
    }
}