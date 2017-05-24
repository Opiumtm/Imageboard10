using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Imageboard10.ModuleInterface;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Коллекция модулей.
    /// </summary>
    public sealed class ModuleCollection : IModuleCollection
    {
        private readonly Provider _internalProvider = new Provider();

        private int _isSealed;

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
        public bool CanGetModuleProvider => Interlocked.CompareExchange(ref _isSealed, 0, 0) != 0;

        /// <summary>
        /// Получить сводный провайдер модулей.
        /// </summary>
        /// <returns>Провайдер модулей.</returns>
        public IModuleProvider GetModuleProvider()
        {
            if (!CanGetModuleProvider)
            {
                throw new InvalidOperationException("Нельзя получить провайдер модуля до завершения этапа регистрации");
            }
            return _internalProvider;
        }

        /// <summary>
        /// Завершить регистрацию.
        /// </summary>
        public void Seal()
        {
            Interlocked.Exchange(ref _isSealed, 1);
        }

        private sealed class Provider : IModuleProvider
        {
            private readonly Dictionary<Type, List<IModuleProvider>> _providers = new Dictionary<Type, List<IModuleProvider>>();

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
        }
    }
}