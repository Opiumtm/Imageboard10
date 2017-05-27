using System;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Статический провайдер модуля.
    /// </summary>
    /// <typeparam name="T">Тип модуля.</typeparam>
    /// <typeparam name="TIntf">Тип интерфейса</typeparam>
    public sealed class StaticModuleProvider<T, TIntf> : ModuleBase<IModuleProvider>, IModuleProvider
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
            :base(GetSuspendAware(module), false)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            _module = module;
            _filter = filter;
        }

        private static bool GetSuspendAware(IModule module)
        {
            var lt = module?.QueryView<IModuleLifetime>();
            if (lt != null)
            {
                return lt.IsSuspendAware;
            }
            return false;
        }

        /// <summary>
        /// Действие по завершению работы.
        /// </summary>
        protected override async ValueTask<Nothing> OnDispose()
        {
            var lt = _module.QueryView<IModuleLifetime>();
            if (lt != null)
            {
                await lt.DisposeModule();
            }
            await base.OnDispose();
            return Nothing.Value;
        }

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            var lt = _module.QueryView<IModuleLifetime>();
            if (lt != null)
            {
                await lt.InitializeModule(moduleProvider);
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Все модули инициализированы.
        /// </summary>
        protected override async ValueTask<Nothing> OnAllInitialized()
        {
            await base.OnAllInitialized();
            var lt = _module.QueryView<IModuleLifetime>();
            if (lt != null)
            {
                await lt.AllModulesInitialized();
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Действие по приостановке работы.
        /// </summary>
        protected override async ValueTask<Nothing> OnSuspended()
        {
            await base.OnSuspended();
            var lt = _module.QueryView<IModuleLifetime>();
            if (lt != null)
            {
                await lt.SuspendModule();
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Действие по вовозбновлению работы.
        /// </summary>
        protected override async ValueTask<Nothing> OnResumed()
        {
            await base.OnResumed();
            var lt = _module.QueryView<IModuleLifetime>();
            if (lt != null)
            {
                await lt.ResumeModule();
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Действие по вовозбновлению работы всех модулей родителя.
        /// </summary>
        protected override async ValueTask<Nothing> OnAllResumed()
        {
            await base.OnAllResumed();
            var lt = _module.QueryView<IModuleLifetime>();
            if (lt != null)
            {
                await lt.AllModulesResumed();
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Родительский провайдер модулей.
        /// </summary>
        public IModuleProvider Parent => ModuleProvider;

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
    }
}