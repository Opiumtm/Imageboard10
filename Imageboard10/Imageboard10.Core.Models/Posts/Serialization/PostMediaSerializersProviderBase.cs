using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Базовый класс провайдера сериализации медиа в постах.
    /// </summary>
    public abstract class PostMediaSerializersProviderBase : ModuleBase<IModuleProvider>, IModuleProvider
    {
        /// <summary>
        /// Родительский провайдер модулей.
        /// </summary>
        public IModuleProvider Parent => ModuleProvider;

        protected PostMediaSerializersProviderBase()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            foreach (var ls in CreateSerializers())
            {
                if (ls is IModule m)
                {
                    _byId[ls.TypeId ?? ""] = m;
                    _byType[ls.Type ?? typeof(object)] = m;
                }
            }
        }

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
            if (moduleType != typeof(IPostMediaSerializer))
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
            foreach (var m in _byType.Values)
            {
                var lt = m.QueryView<IModuleLifetime>();
                if (lt != null)
                {
                    await lt.InitializeModule(moduleProvider ?? this);
                }
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Создать сериализаторы.
        /// </summary>
        /// <returns>Сериализаторы.</returns>
        protected abstract IEnumerable<IPostMediaSerializer> CreateSerializers();

        private readonly Dictionary<Type, IModule> _byType = new Dictionary<Type, IModule>();

        private readonly Dictionary<string, IModule> _byId = new Dictionary<string, IModule>(StringComparer.OrdinalIgnoreCase);
    }
}