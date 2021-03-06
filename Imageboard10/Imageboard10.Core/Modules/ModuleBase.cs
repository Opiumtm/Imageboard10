﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Базовый класс модулей.
    /// </summary>
    /// <typeparam name="TIntf">Тип интерфейса.</typeparam>
    public abstract class ModuleBase<TIntf> : IModule, IBaseModuleLogicSuspendAwareCallbacks
        where TIntf: class 
    {
        private readonly BaseModuleLogic<TIntf> _moduleLifetime;

        /// <summary>
        /// Конструктор.
        /// </summary>
        protected ModuleBase()
        {
            IBaseModuleLogicCallbacks callbacks = this;
            _moduleLifetime = new BaseModuleLogic<TIntf>(this, callbacks);
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="attachToParentDispose">Присоединить к родительскому событию по завершению работы.</param>
        protected ModuleBase(bool attachToParentDispose)
        {
            IBaseModuleLogicCallbacks callbacks = this;
            _moduleLifetime = new BaseModuleLogic<TIntf>(this, callbacks, attachToParentDispose);
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="suspendedAware">Поддерживает приостановку работы.</param>
        /// <param name="attachToParentDispose">Присоединить к родительскому событию по завершению работы.</param>
        protected ModuleBase(bool suspendedAware, bool attachToParentDispose)
        {
            if (suspendedAware)
            {
                IBaseModuleLogicSuspendAwareCallbacks callbacks = this;
                _moduleLifetime = new BaseModuleLogic<TIntf>(this, callbacks, attachToParentDispose);
            }
            else
            {
                IBaseModuleLogicCallbacks callbacks = this;
                _moduleLifetime = new BaseModuleLogic<TIntf>(this, callbacks, attachToParentDispose);
            }
        }

        /// <summary>
        /// Действие по завершению работы.
        /// </summary>
        protected virtual ValueTask<Nothing> OnDispose()
        {
            return new ValueTask<Nothing>(Nothing.Value);
        }

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected virtual ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            Interlocked.Exchange(ref _moduleProvider, moduleProvider);
            return new ValueTask<Nothing>(Nothing.Value);
        }

        /// <summary>
        /// Все модули инициализированы.
        /// </summary>
        protected virtual ValueTask<Nothing> OnAllInitialized()
        {
            return new ValueTask<Nothing>(Nothing.Value);
        }

        /// <summary>
        /// Действие по приостановке работы.
        /// </summary>
        protected virtual ValueTask<Nothing> OnSuspended()
        {
            return new ValueTask<Nothing>(Nothing.Value);
        }

        /// <summary>
        /// Действие по вовозбновлению работы.
        /// </summary>
        protected virtual ValueTask<Nothing> OnResumed()
        {
            return new ValueTask<Nothing>(Nothing.Value);
        }

        /// <summary>
        /// Действие по вовозбновлению работы всех модулей родителя.
        /// </summary>
        protected virtual ValueTask<Nothing> OnAllResumed()
        {
            return new ValueTask<Nothing>(Nothing.Value);
        }

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        public virtual object QueryView(Type viewType) => _moduleLifetime.QueryView(viewType);

        /// <summary>
        /// Модуль готов к использованию.
        /// </summary>
        public bool IsModuleReady => _moduleLifetime.IsModuleReady;

        private IModuleProvider _moduleProvider;

        /// <summary>
        /// Провайдер модулей.
        /// </summary>
        protected IModuleProvider ModuleProvider => Interlocked.CompareExchange(ref _moduleProvider, null, null);

        /// <summary>
        /// Проверка на готовность модуля.
        /// </summary>
        protected void CheckModuleReady()
        {
            if (!_moduleLifetime.IsModuleReady)
            {
                throw new ModuleNotReadyException();
            }
        }

        ValueTask<Nothing> IBaseModuleLogicCallbacks.OnInitilizeLifetimeCallback(IModuleProvider provider)
        {
            return OnInitialize(provider);
        }

        ValueTask<Nothing> IBaseModuleLogicCallbacks.OnDisposeLifetimeCallback()
        {
            return OnDispose();
        }

        ValueTask<Nothing> IBaseModuleLogicCallbacks.OnAllInitializedLifetimeCallback()
        {
            return OnAllInitialized();
        }

        ValueTask<Nothing> IBaseModuleLogicSuspendAwareCallbacks.OnSuspendLifetimeCallback()
        {
            return OnSuspended();
        }

        ValueTask<Nothing> IBaseModuleLogicSuspendAwareCallbacks.OnResumeLifetimeCallback()
        {
            return OnResumed();
        }

        ValueTask<Nothing> IBaseModuleLogicSuspendAwareCallbacks.OnAllResumedLifetimeCallback()
        {
            return OnAllResumed();
        }
    }
}