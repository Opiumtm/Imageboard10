using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Обратные вызовы логики времени жизни модуля.
    /// </summary>
    public interface IBaseModuleLogicCallbacks
    {
        /// <summary>
        /// Инициализация.
        /// </summary>
        /// <param name="provider">Провайдер.</param>
        ValueTask<Nothing> OnInitilizeLifetimeCallback(IModuleProvider provider);

        /// <summary>
        /// Завершение работы.
        /// </summary>
        ValueTask<Nothing> OnDisposeLifetimeCallback();

        /// <summary>
        /// Все модули инициализированы.
        /// </summary>
        ValueTask<Nothing> OnAllInitializedLifetimeCallback();
    }

    /// <summary>
    /// Обратные вызовы логики времени жизни модуля c поддержкой приостановки работы.
    /// </summary>
    public interface IBaseModuleLogicSuspendAwareCallbacks : IBaseModuleLogicCallbacks
    {
        /// <summary>
        /// Приостановка.
        /// </summary>
        ValueTask<Nothing> OnSuspendLifetimeCallback();

        /// <summary>
        /// Возобновление.
        /// </summary>
        ValueTask<Nothing> OnResumeLifetimeCallback();

        /// <summary>
        /// Всё возобновлено.
        /// </summary>
        ValueTask<Nothing> OnAllResumedLifetimeCallback();
    }

    /// <summary>
    /// Базовая логика времени жизни модуля.
    /// </summary>
    /// <typeparam name="TIntf">Интерфейс, реализуемый модулем.</typeparam>
    public sealed class BaseModuleLogic<TIntf> : IModule, IModuleLifetime, ModuleInterface.IModuleLifetimeEvents
        where TIntf: class 
    {
        private readonly object _host;
        private readonly IBaseModuleLogicCallbacks _callbacks;
        private readonly IBaseModuleLogicSuspendAwareCallbacks _suspendAwareCallbacks;
        private readonly bool _attachToParentDispose;
        private int _isDisposeAttached;
        private IModuleProvider _parent;
        private readonly bool _suspendedAware;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="host">Модуль.</param>
        /// <param name="callbacks">Обратные вызовы.</param>
        /// <param name="attachToParentEvents">Присоединить к родительским событиям.</param>
        public BaseModuleLogic(object host,
            IBaseModuleLogicCallbacks callbacks,
            bool attachToParentEvents = false)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _callbacks = callbacks;
            _attachToParentDispose = attachToParentEvents;
            _suspendedAware = false;
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="host">Модуль.</param>
        /// <param name="callbacks">Обратные вызовы.</param>
        /// <param name="attachToParentEvents">Присоединить к родительским событиям.</param>
        public BaseModuleLogic(object host,
            IBaseModuleLogicSuspendAwareCallbacks callbacks,
            bool attachToParentEvents = false)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _suspendAwareCallbacks = callbacks;
            _callbacks = callbacks;
            _attachToParentDispose = attachToParentEvents;
            _suspendedAware = true;
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
        private int _isDoneInitialize;
        private int _isDisposed;
        private int _isSuspended;

        /// <summary>
        /// Модуль готов к использованию.
        /// </summary>
        public bool IsModuleReady => Interlocked.CompareExchange(ref _isDoneInitialize, 0, 0) != 0 &&
                                     Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 0 &&
                                     (Interlocked.CompareExchange(ref _isSuspended, 0, 0) == 0 || !_suspendedAware);

        /// <summary>
        /// Инициализировать модуль.
        /// </summary>
        /// <param name="provider">Провайдер модулей.</param>
        public async ValueTask<Nothing> InitializeModule(IModuleProvider provider)
        {
            if (Interlocked.Exchange(ref _isInitialized, 1) == 0)
            {
                Interlocked.Exchange(ref _parent, provider);
                if (_callbacks != null)
                {
                    await _callbacks.OnInitilizeLifetimeCallback(provider);
                    Interlocked.Exchange(ref _isDoneInitialize, 1);
                }
                if (_attachToParentDispose)
                {
                    var lt = provider?.QueryView<ModuleInterface.IModuleLifetimeEvents>();
                    if (lt != null)
                    {
                        Interlocked.Exchange(ref _isDisposeAttached, 1);
                        lt.Disposed += ParentOnDisposed;
                        lt.AllInitialized += ParentOnAllInitialized;
                        lt.Suspended += ParentOnSuspended;
                        lt.Resumed += ParentOnResumed;
                        lt.AllResumed += ParentOnAllResumed;
                    }
                }
            }
            return Nothing.Value;
        }

        private async void ParentOnAllInitialized(object o)
        {
            try
            {
                await AllModulesInitialized();
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        private async void ParentOnSuspended(object o)
        {
            try
            {
                await SuspendModule();
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        private async void ParentOnResumed(object o)
        {
            try
            {
                await ResumeModule();
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        private async void ParentOnAllResumed(object o)
        {
            try
            {
                await AllModulesResumed();
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
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
                if (_callbacks != null)
                {
                    await _callbacks.OnDisposeLifetimeCallback();
                }
                Disposed?.Invoke(null);
                if (Interlocked.Exchange(ref _isDisposeAttached, 0) != 0)
                {
                    var provider = Interlocked.CompareExchange(ref _parent, null, null);
                    var lt = provider?.QueryView<ModuleInterface.IModuleLifetimeEvents>();
                    if (lt != null)
                    {
                        lt.Disposed -= ParentOnDisposed;
                        lt.AllInitialized -= ParentOnAllInitialized;
                        lt.Suspended -= ParentOnSuspended;
                        lt.Resumed -= ParentOnResumed;
                        lt.AllResumed -= ParentOnAllResumed;
                    }
                }
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Приостановить работу модуля.
        /// </summary>
        public async ValueTask<Nothing> SuspendModule()
        {
            if (_suspendedAware && Interlocked.Exchange(ref _isSuspended, 1) == 0)
            {
                if (_suspendAwareCallbacks != null)
                {
                    await _suspendAwareCallbacks.OnSuspendLifetimeCallback();
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
            if (_suspendedAware && Interlocked.Exchange(ref _isSuspended, 0) != 0)
            {
                if (_suspendAwareCallbacks != null)
                {
                    await _suspendAwareCallbacks.OnResumeLifetimeCallback();
                }
                Resumed?.Invoke(null);
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Все модули возобновлены.
        /// </summary>
        public async ValueTask<Nothing> AllModulesResumed()
        {
            if (_suspendedAware && _suspendAwareCallbacks != null)
            {
                await _suspendAwareCallbacks.OnAllResumedLifetimeCallback();
            }
            AllResumed?.Invoke(null);
            return Nothing.Value;
        }

        /// <summary>
        /// Все модули инициализированы.
        /// </summary>
        public async ValueTask<Nothing> AllModulesInitialized()
        {
            if (_callbacks != null)
            {
                await _callbacks.OnAllInitializedLifetimeCallback();
            }
            AllInitialized?.Invoke(null);
            return Nothing.Value;
        }

        /// <summary>
        /// Поддерживает приостановку и восстановление.
        /// </summary>
        public bool IsSuspendAware => _suspendedAware;

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