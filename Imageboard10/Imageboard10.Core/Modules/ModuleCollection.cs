using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="parent">Родительский провайдер.</param>
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
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Завершить работу.
        /// </summary>
        public async ValueTask<Nothing> Dispose()
        {
            await _internalProvider.DisposeModule();
            return Nothing.Value;
        }

        private sealed class Provider : IModuleProvider, IModuleLifetime
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
                return null;
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
                if (viewType == typeof(IModule) || viewType == typeof(IModuleProvider) || viewType == typeof(IModuleLifetime))
                {
                    return this;
                }
                return null;
            }

            // ReSharper disable once InconsistentNaming
            public int _isDisposed;
            private int _isInitialized;

            public bool IsModuleReady => Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 0 && Interlocked.CompareExchange(ref _isInitialized, 0, 0) != 0;

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
                }
                return Nothing.Value;
            }
        }
    }
}