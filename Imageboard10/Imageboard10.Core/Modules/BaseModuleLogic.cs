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
        private readonly Func<ValueTask<Nothing>> _allInitFunc;
        private readonly Func<ValueTask<Nothing>> _suspendFunc;
        private readonly Func<ValueTask<Nothing>> _resumeFunc;
        private readonly Func<ValueTask<Nothing>> _allResumedFunc;
        private readonly bool _attachToParentDispose;
        private int _isDisposeAttached;
        private IModuleProvider _parent;
        private readonly bool _suspendedAware;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="host">Модуль.</param>
        /// <param name="initFunc">Функция инициализации.</param>
        /// <param name="disposeFunc">Функция завершения работы.</param>
        /// <param name="allInitFunc">Функция по инициализации всех модулей.</param>
        /// <param name="attachToParentEvents">Присоединить к родительским событиям.</param>
        public BaseModuleLogic(object host, 
            Func<IModuleProvider, ValueTask<Nothing>> initFunc, 
            Func<ValueTask<Nothing>> disposeFunc, 
            Func<ValueTask<Nothing>> allInitFunc,
            bool attachToParentEvents = false)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _initFunc = initFunc;
            _disposeFunc = disposeFunc;
            _allInitFunc = allInitFunc;
            _attachToParentDispose = attachToParentEvents;
            _suspendedAware = false;
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="host">Модуль.</param>
        /// <param name="initFunc">Функция инициализации.</param>
        /// <param name="disposeFunc">Функция завершения работы.</param>
        /// <param name="allInitFunc">Функция по инициализации всех модулей.</param>
        /// <param name="suspendFunc">Функция по приостановке.</param>
        /// <param name="resumeFunc">Функция по возобновлению.</param>
        /// <param name="allResumedFunc">Функция по возобновлению всех модулей.</param>
        /// <param name="attachToParentEvents">Присоединить к родительским событиям.</param>
        public BaseModuleLogic(object host,
            Func<IModuleProvider, ValueTask<Nothing>> initFunc,
            Func<ValueTask<Nothing>> disposeFunc,
            Func<ValueTask<Nothing>> allInitFunc,
            Func<ValueTask<Nothing>> suspendFunc,
            Func<ValueTask<Nothing>> resumeFunc,
            Func<ValueTask<Nothing>> allResumedFunc,
            bool attachToParentEvents = false)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _initFunc = initFunc;
            _disposeFunc = disposeFunc;
            _allInitFunc = allInitFunc;
            _suspendFunc = suspendFunc;
            _resumeFunc = resumeFunc;
            _allResumedFunc = allResumedFunc;
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
        private int _isDisposed;
        private int _isSuspended;

        /// <summary>
        /// Модуль готов к использованию.
        /// </summary>
        public bool IsModuleReady => Interlocked.CompareExchange(ref _isInitialized, 0, 0) != 0 &&
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
                if (_suspendFunc != null)
                {
                    await _suspendFunc();
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
                if (_resumeFunc != null)
                {
                    await _resumeFunc();
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
            if (_suspendedAware && _allResumedFunc != null)
            {
                await _allResumedFunc();
            }
            AllResumed?.Invoke(null);
            return Nothing.Value;
        }

        /// <summary>
        /// Все модули инициализированы.
        /// </summary>
        public async ValueTask<Nothing> AllModulesInitialized()
        {
            if (_allInitFunc != null)
            {
                await _allInitFunc();
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