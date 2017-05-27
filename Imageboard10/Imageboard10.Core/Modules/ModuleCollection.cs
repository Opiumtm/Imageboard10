using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Коллекция модулей.
    /// </summary>
    public sealed class ModuleCollection : IModuleCollection
    {
        private readonly Provider _internalProvider = new Provider();

        private readonly IModuleProvider _parent;

        private int _isSealed;

        private int _isParentDisposeEvent;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="parent">Родительский провайдер (должен быть инициализирован).</param>
        /// <param name="attachEvents">Слушать события родителя.</param>
        public ModuleCollection(IModuleProvider parent = null)
        {
            _parent = parent;
        }

        /// <summary>
        /// Зарегистрировать провайдер модулей.
        /// </summary>
        /// <param name="moduleType">Тип модуля. Может быть NULL.</param>
        /// <param name="provider">Провайдер.</param>
        public void RegisterProvider(Type moduleType, IModuleProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (!CanRegisterProviders)
            {
                throw new InvalidOperationException("Нельзя регистрировать провайдер модуля после завершения этапа регистрации");
            }
            _internalProvider.Add(moduleType, provider);
        }

        private async void OnParentDisposed(object o)
        {
            try
            {
                await Dispose();
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        private async void OnParentSuspended(object o)
        {
            try
            {
                await Suspend();
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        private async void OnParentResumed(object o)
        {
            try
            {
                await Resume();
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        /// <summary>
        /// Можно регистрировать провайдеры.
        /// </summary>
        public bool CanRegisterProviders => Interlocked.CompareExchange(ref _isSealed, 0, 0) == 0;

        /// <summary>
        /// Можно получать провайдеры модулей.
        /// </summary>
        public bool CanGetModuleProvider => Interlocked.CompareExchange(ref _isSealed, 0, 0) != 0 && Interlocked.CompareExchange(ref _internalProvider._isDisposed, 0, 0) == 0;

        /// <summary>
        /// Получить сводный провайдер модулей.
        /// </summary>
        /// <returns>Провайдер модулей.</returns>
        public IModuleProvider GetModuleProvider()
        {
            if (!CanGetModuleProvider)
            {
                throw new InvalidOperationException("Нельзя получить провайдер модуля в данном состоянии объекта");
            }
            return _internalProvider;
        }

        /// <summary>
        /// Завершить регистрацию.
        /// </summary>
        public async ValueTask<Nothing> Seal()
        {
            if (Interlocked.Exchange(ref _isSealed, 1) == 0)
            {
                await _internalProvider.InitializeModule(_parent);
                if (_parent != null)
                {
                    if (Interlocked.CompareExchange(ref _isParentDisposeEvent, 0, 0) == 0)
                    {
                        var lt = _parent.QueryView<ModuleInterface.IModuleLifetimeEvents>();
                        if (lt != null)
                        {
                            Interlocked.Exchange(ref _isParentDisposeEvent, 1);
                            lt.Disposed += OnParentDisposed;
                            lt.Suspended += OnParentSuspended;
                            lt.Resumed += OnParentResumed;
                            lt.AllInitialized += ParentOnAllInitialized;
                            lt.AllResumed += ParentOnAllResumed;
                        }
                    }
                }
            }
            return Nothing.Value;
        }

        private async void ParentOnAllResumed(object o)
        {
            try
            {
                await _internalProvider.AllModulesResumed();
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        private async void ParentOnAllInitialized(object o)
        {
            try
            {
                await _internalProvider.AllModulesInitialized();
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        /// <summary>
        /// Завершить работу.
        /// </summary>
        public async ValueTask<Nothing> Dispose()
        {
            await _internalProvider.DisposeModule();
            if (Interlocked.Exchange(ref _isParentDisposeEvent, 0) != 0)
            {
                var lt = _parent?.QueryView<ModuleInterface.IModuleLifetimeEvents>();
                if (lt != null)
                {
                    lt.Disposed -= OnParentDisposed;
                    lt.Suspended -= OnParentSuspended;
                    lt.Resumed -= OnParentResumed;
                    lt.AllInitialized -= ParentOnAllInitialized;
                    lt.AllResumed -= ParentOnAllResumed;
                }
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Приостановить работу.
        /// </summary>
        public async ValueTask<Nothing> Suspend()
        {
            await _internalProvider.SuspendModule();
            return Nothing.Value;
        }

        /// <summary>
        /// Возобновить работу.
        /// </summary>
        public async ValueTask<Nothing> Resume()
        {
            await _internalProvider.ResumeModule();
            return Nothing.Value;
        }

        private sealed class Provider : IModuleProvider, IModuleLifetime, ModuleInterface.IModuleLifetimeEvents
        {
            private readonly Dictionary<Type, List<IModuleProvider>> _providers = new Dictionary<Type, List<IModuleProvider>>();

            private IModuleProvider _parent;

            public IModuleProvider Parent => Interlocked.CompareExchange(ref _parent, null, null);

            public async ValueTask<IModule> QueryModuleAsync<T>(Type moduleType, T query)
            {
                Type t = moduleType ?? typeof(Provider);
                if (_providers.ContainsKey(t))
                {
                    var l = _providers[t];
                    for (var i = l.Count - 1; i >= 0; i--)
                    {
                        var m = await l[i].QueryModuleAsync(moduleType, query);
                        if (m != null)
                        {
                            return m;
                        }
                    }
                }
                var p = Parent;
                if (p != null)
                {
                    return await p.QueryModuleAsync(moduleType, query);
                }
                return null;
            }

            public IModule QueryModule<T>(Type moduleType, T query)
            {
                Type t = moduleType ?? typeof(Provider);
                if (_providers.ContainsKey(t))
                {
                    var l = _providers[t];
                    for (var i = l.Count - 1; i >= 0; i--)
                    {
                        var m = l[i].QueryModule(moduleType, query);
                        if (m != null)
                        {
                            return m;
                        }
                    }
                }
                var p = Parent;
                return p?.QueryModule(moduleType, query);
            }

            public void Add(Type moduleType, IModuleProvider provider)
            {
                Type t = moduleType ?? typeof(Provider);
                lock (_providers)
                {
                    if (!_providers.ContainsKey(t))
                    {
                        _providers[t] = new List<IModuleProvider>();
                    }
                    _providers[t].Add(provider);
                }
            }

            public object QueryView(Type viewType)
            {
                if (viewType == typeof(IModule) || viewType == typeof(IModuleProvider) || viewType == typeof(IModuleLifetime) || viewType == typeof(ModuleInterface.IModuleLifetimeEvents))
                {
                    return this;
                }
                return null;
            }

            // ReSharper disable once InconsistentNaming
            public int _isDisposed;
            private int _isInitialized;
            private int _isSuspended;

            public bool IsModuleReady => Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 0 && Interlocked.CompareExchange(ref _isInitialized, 0, 0) != 0 && Interlocked.CompareExchange(ref _isSuspended, 0, 0) == 0;

            public async ValueTask<Nothing> InitializeModule(IModuleProvider provider)
            {
                if (Interlocked.Exchange(ref _isInitialized, 1) == 0)
                {
                    Interlocked.Exchange(ref _parent, provider);
                    foreach (var pt in _providers.Values)
                    {
                        foreach (var p in pt.Select(p => p.QueryView<IModuleLifetime>()).Where(p => p != null))
                        {
                            await p.InitializeModule(this);
                        }
                    }
                    await AllModulesInitialized();
                }
                return Nothing.Value;
            }

            public async ValueTask<Nothing> DisposeModule()
            {
                if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
                {
                    foreach (var pt in _providers.Values)
                    {
                        foreach (var p in pt.Select(p => p.QueryView<IModuleLifetime>()).Where(p => p != null))
                        {
                            await p.DisposeModule();
                        }
                    }
                    Disposed?.Invoke(null);
                }
                return Nothing.Value;
            }

            /// <summary>
            /// Приостановить работу модуля.
            /// </summary>
            public async ValueTask<Nothing> SuspendModule()
            {
                if (Interlocked.Exchange(ref _isSuspended, 1) == 0)
                {
                    foreach (var pt in _providers.Values)
                    {
                        foreach (var p in pt.Select(p => p.QueryView<IModuleLifetime>()).Where(p => p != null && p.IsSuspendAware))
                        {
                            await p.SuspendModule();
                        }
                    }
                    Suspended?.Invoke(null);
                }
                return Nothing.Value;
            }

            /// <summary>
            /// Возобновить работу модуля.
            /// </summary>
            public async ValueTask<Nothing> ResumeModule()
            {
                if (Interlocked.Exchange(ref _isSuspended, 0) != 0)
                {
                    foreach (var pt in _providers.Values)
                    {
                        foreach (var p in pt.Select(p => p.QueryView<IModuleLifetime>()).Where(p => p != null && p.IsSuspendAware))
                        {
                            await p.ResumeModule();
                        }
                    }
                    Resumed?.Invoke(null);
                    var parent = Interlocked.CompareExchange(ref _parent, null, null);
                    if (parent == null)
                    {
                        await AllModulesResumed();
                    }
                }
                return Nothing.Value;
            }

            /// <summary>
            /// Все модули возобновлены.
            /// </summary>
            public async ValueTask<Nothing> AllModulesResumed()
            {
                foreach (var pt in _providers.Values)
                {
                    foreach (var p in pt.Select(p => p.QueryView<IModuleLifetime>()).Where(p => p != null))
                    {
                        await p.AllModulesResumed();
                    }
                }
                AllResumed?.Invoke(null);
                return Nothing.Value;
            }

            /// <summary>
            /// Все модули инициализированы.
            /// </summary>
            public async ValueTask<Nothing> AllModulesInitialized()
            {
                foreach (var pt in _providers.Values)
                {
                    foreach (var p in pt.Select(p => p.QueryView<IModuleLifetime>()).Where(p => p != null))
                    {
                        await p.AllModulesInitialized();
                    }
                }
                AllInitialized?.Invoke(null);
                return Nothing.Value;
            }

            /// <summary>
            /// Поддерживает приостановку и восстановление.
            /// </summary>
            public bool IsSuspendAware => true;

            /// <summary>
            /// Работа модуля завершена.
            /// </summary>
            public event ModuleInterface.ModuleLifetimeEventHandler Disposed;

            /// <summary>
            /// Работа приостановлена.
            /// </summary>
            public event ModuleInterface.ModuleLifetimeEventHandler Suspended;

            /// <summary>
            /// Работа возобновлена.
            /// </summary>
            public event ModuleInterface.ModuleLifetimeEventHandler Resumed;

            /// <summary>
            /// Работа возобновлена для всех модулей.
            /// </summary>
            public event ModuleInterface.ModuleLifetimeEventHandler AllResumed;

            /// <summary>
            /// Все модули инициализированы.
            /// </summary>
            public event ModuleInterface.ModuleLifetimeEventHandler AllInitialized;
        }
    }
}