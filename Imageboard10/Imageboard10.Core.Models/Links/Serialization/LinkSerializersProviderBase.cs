using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Провайдер сериализаторов ссылок.
    /// </summary>
    public abstract class LinkSerializersProviderBase : ModuleBase<IModuleProvider>, IModuleProvider
    {
        /// <summary>
        /// Родительский провайдер модулей.
        /// </summary>
        public IModuleProvider Parent => ModuleProvider;

        /// <summary>
        /// Запросить модуль асинхронно.
        /// </summary>
        /// <typeparam name="T">Тип запроса. Должен поддерзживаться тип PropertySet, если предполагается запрос из WinRT-среды.</typeparam>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public ValueTask<IModule> QueryModuleAsync<T>(Type moduleType, T query)
        {
            return new ValueTask<IModule>(QueryModule(moduleType, query));
        }

        /// <summary>
        /// Запросить модуль.
        /// </summary>
        /// <typeparam name="T">Тип запроса. Должен поддерзживаться тип PropertySet, если предполагается запрос из WinRT-среды.</typeparam>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public IModule QueryModule<T>(Type moduleType, T query)
        {
            if (query == null)
            {
                return null;
            }
            if (moduleType != typeof(ILinkSerializer))
            {
                return null;
            }
            if (query is Type)
            {
                var t = query as Type;
                if (_byType.ContainsKey(t))
                {
                    return _byType[t];
                }
            }
            if (query is string)
            {
                var s = query as string;
                if (_byId.ContainsKey(s))
                {
                    return _byId[s];
                }
            }
            return null;
        }

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            foreach (var ls in CreateSerializers())
            {
                if (ls is IModule m)
                {
                    var lt = m.QueryView<IModuleLifetime>();
                    if (lt != null)
                    {
                        await lt.InitializeModule(moduleProvider ?? this);
                    }
                    _byId[ls.LinkTypeId ?? ""] = m;
                    _byType[ls.LinkType ?? typeof(object)] = m;
                }
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Создать сериализаторы.
        /// </summary>
        /// <returns>Сериализаторы.</returns>
        protected abstract IEnumerable<ILinkSerializer> CreateSerializers();

        private readonly Dictionary<Type, IModule> _byType = new Dictionary<Type, IModule>();

        private readonly Dictionary<string, IModule> _byId = new Dictionary<string, IModule>(StringComparer.OrdinalIgnoreCase);
    }
}