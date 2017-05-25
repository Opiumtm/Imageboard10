using System;
using System.Threading;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Статический провайдер модуля.
    /// </summary>
    /// <typeparam name="T">Тип модуля.</typeparam>
    /// <typeparam name="TIntf">Тип интерфейса</typeparam>
    public sealed class StaticModuleProvider<T, TIntf> : IModuleProvider, IModuleLifetime
        where T : IModule, TIntf
    {
        private readonly T _module;

        private readonly IStaticModuleQueryFilter _filter;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="module">Модуль.</param>
        /// <param name="filter">Фильтр запроса на модуль.</param>
        public StaticModuleProvider(T module, IStaticModuleQueryFilter filter = null)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            _module = module;
            _filter = filter;
        }

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        public object QueryView(Type viewType)
        {
            if (viewType == typeof(IModule) || viewType == typeof(IModuleProvider) ||
                viewType == typeof(IModuleLifetime))
            {
                return this;
            }
            return null;
        }

        /// <summary>
        /// Модуль готов к использованию.
        /// </summary>
        public bool IsModuleReady => Interlocked.CompareExchange(ref _isInitialized, 0, 0) != 0 &&
                                     Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 0;

        private int _isInitialized;

        private int _isDisposed;

        private IModuleProvider _parent;

        /// <summary>
        /// Родительский провайдер модулей.
        /// </summary>
        public IModuleProvider Parent => Interlocked.CompareExchange(ref _parent, null, null);

        /// <summary>
        /// Запросить модуль асинхронно.
        /// </summary>
        /// <typeparam name="T1">Тип запроса. Должен поддерзживаться тип PropertySet, если предполагается запрос из WinRT-среды.</typeparam>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public ValueTask<IModule> QueryModuleAsync<T1>(Type moduleType, T1 query)
        {
            return new ValueTask<IModule>(QueryModule(moduleType, query));
        }

        /// <summary>
        /// Запросить модуль.
        /// </summary>
        /// <typeparam name="T1">Тип запроса. Должен поддерзживаться тип PropertySet, если предполагается запрос из WinRT-среды.</typeparam>
        /// <param name="moduleType">Тип модуля. Может быть null.</param>
        /// <param name="query">Запрос. Может быть null.</param>
        public IModule QueryModule<T1>(Type moduleType, T1 query)
        {
            if (moduleType == typeof(TIntf))
            {
                if (_filter?.CheckQuery(query) ?? true)
                {
                    return _module;
                }
            }
            return null;
        }

        /// <summary>
        /// Инициализировать модуль.
        /// </summary>
        /// <param name="provider">Провайдер модулей.</param>
        public async ValueTask<Nothing> InitializeModule(IModuleProvider provider)
        {
            if (Interlocked.Exchange(ref _isInitialized, 1) == 0)
            {
                Interlocked.Exchange(ref _parent, provider);
                var lt = _module.QueryView<IModuleLifetime>();
                if (lt != null)
                {
                    await lt.InitializeModule(provider);
                }
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Завершить работу модуля.
        /// </summary>
        public async ValueTask<Nothing> DisposeModule()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                var lt = _module.QueryView<IModuleLifetime>();
                if (lt != null)
                {
                    await lt.DisposeModule();
                }
            }
            return Nothing.Value;
        }
    }
}