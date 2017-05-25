using System;
using System.Threading;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Базовая логика времени жизни модуля.
    /// </summary>
    /// <typeparam name="TIntf">Интерфейс, реализуемый модулем.</typeparam>
    public sealed class BaseModuleLogic<TIntf> : IModule, IModuleLifetime
        where TIntf: class 
    {
        private readonly object _host;
        private readonly Func<IModuleProvider, ValueTask<Nothing>> _initFunc;
        private readonly Func<ValueTask<Nothing>> _disposeFunc;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="host">Модуль.</param>
        /// <param name="initFunc">Функция инициализации.</param>
        /// <param name="disposeFunc">Функция завершения работы.</param>
        public BaseModuleLogic(object host, Func<IModuleProvider, ValueTask<Nothing>> initFunc, Func<ValueTask<Nothing>> disposeFunc)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _initFunc = initFunc;
            _disposeFunc = disposeFunc;
        }

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        public object QueryView(Type viewType)
        {
            if (viewType == typeof(IModuleLifetime))
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
                if (_initFunc != null)
                {
                    await _initFunc(provider);
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
                if (_disposeFunc != null)
                {
                    await _disposeFunc();
                }
            }
            return Nothing.Value;
        }
    }
}