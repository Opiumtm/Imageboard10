using System;
using System.Threading.Tasks;
using Windows.Storage;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Tasks;

namespace Imageboard10.Core.Config
{
    /// <summary>
    /// Базовый класс для конфигурации в реестре.
    /// </summary>
    /// <typeparam name="T">Тип интерфейса конфигурации.</typeparam>
    public abstract class RegistryConfigurationBase<T> : ModuleBase<T>, IConfiguration
        where T : class, IConfiguration
    {
        private ApplicationDataContainer _container;

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            _container = GetСontainer();
            LoadConfiguration(_container);
            return Nothing.Value;
        }

        /// <summary>
        /// Сохранить конфигурацию.
        /// </summary>
        /// <param name="container">Контейнер.</param>
        protected abstract void SaveConfiguration(ApplicationDataContainer container);

        /// <summary>
        /// Загрузить конфигурацию.
        /// </summary>
        /// <param name="container">Контейнер.</param>
        protected abstract void LoadConfiguration(ApplicationDataContainer container);

        /// <summary>
        /// Сохранить конфигурацию.
        /// </summary>
        public virtual async ValueTask<Nothing> Save()
        {
            SaveConfiguration(_container);
            await _saved.Invoke(this, EventArgs.Empty, TimeSpan.FromSeconds(10));
            return Nothing.Value;
        }

        private readonly AsyncEventList<EventArgs> _saved = new AsyncEventList<EventArgs>();

        /// <summary>
        /// Конфигурация сохранена.
        /// </summary>
        public AsyncLanguageEvent<EventArgs> Saved => _saved;

        /// <summary>
        /// Получить корневой контейнер.
        /// </summary>
        /// <returns>Корневой контейнер.</returns>
        protected abstract ApplicationDataContainer GetСontainer();
    }
}