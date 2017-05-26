using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Базовая логика времени жизни модуля.
    /// </summary>
    /// <typeparam name="TIntf">Интерфейс, реализуемый модулем.</typeparam>
    public sealed class BaseModuleLogic<TIntf> : IModule, IModuleLifetime, ModuleInterface.IModuleLifetimeEvents
        where TIntf: class 
    {
        private readonly object _host;
        private readonly Func<IModuleProvider, ValueTask<Nothing>> _initFunc;
        private readonly Func<ValueTask<Nothing>> _disposeFunc;
        private readonly bool _attachToParentDispose;
        private int _isDisposeAttached;
        private IModuleProvider _parent;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="host">Модуль.</param>
        /// <param name="initFunc">Функция инициализации.</param>
        /// <param name="disposeFunc">Функция завершения работы.</param>
        /// <param name="attachToParentDispose">Присоединить к родительскому событию по завершению работы.</param>
        public BaseModuleLogic(object host, Func<IModuleProvider, ValueTask<Nothing>> initFunc, Func<ValueTask<Nothing>> disposeFunc, bool attachToParentDispose = false)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _initFunc = initFunc;
            _disposeFunc = disposeFunc;
            _attachToParentDispose = attachToParentDispose;
        }

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        public object QueryView(Type viewType)
        {
            if (viewType == typeof(IModuleLifetime) || viewType == typeof(ModuleInterface.IModuleLifetimeEvents))
            {
                return this;
            }
            if (viewType == typeof(IModule))
            {
                return _host as IModule;
            }
            if (viewType == typeof(TIntf))
            {
                return _host as TIntf;
            }
            return null;
        }

        private int _isInitialized;
        private int _isDisposed;

        /// <summary>
        /// Модуль готов к использованию.
        /// </summary>
        public bool IsModuleReady => Interlocked.CompareExchange(ref _isInitialized, 0, 0) != 0 &&
                                     Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 0;

        /// <summary>
        /// Инициализировать модуль.
        /// </summary>
        /// <param name="provider">Провайдер модулей.</param>
        public async ValueTask<Nothing> InitializeModule(IModuleProvider provider)
        {
            if (Interlocked.Exchange(ref _isInitialized, 1) == 0)
            {
                Interlocked.Exchange(ref _parent, provider);
                if (_initFunc != null)
                {
                    await _initFunc(provider);
                }
                if (_attachToParentDispose)
                {
                    var lt = provider?.QueryView<ModuleInterface.IModuleLifetimeEvents>();
                    if (lt != null)
                    {
                        Interlocked.Exchange(ref _isDisposeAttached, 1);
                        lt.Disposed += ParentOnDisposed;
                    }
                }
            }
            return Nothing.Value;
        }

        private async void ParentOnDisposed(object o)
        {
            try
            {
                await DisposeModule();
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
        /// Завершить работу модуля.
        /// </summary>
        public async ValueTask<Nothing> DisposeModule()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                if (_disposeFunc != null)
                {
                    await _disposeFunc();
                }
                Disposed?.Invoke(null);
                if (Interlocked.Exchange(ref _isDisposeAttached, 0) != 0)
                {
                    var provider = Interlocked.CompareExchange(ref _parent, null, null);
                    var lt = provider?.QueryView<ModuleInterface.IModuleLifetimeEvents>();
                    if (lt != null)
                    {
                        lt.Disposed -= ParentOnDisposed;
                    }
                }
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Работа модуля завершена.
        /// </summary>
        public event ModuleInterface.ModuleLifetimeEventHandler Disposed;

        /// <summary>
        /// Получить родительский провайдер.
        /// </summary>
        /// <returns>Родительский провайдер.</returns>
        public IModuleProvider GetParent()
        {
            return Interlocked.CompareExchange(ref _parent, null, null);
        }
    }
}