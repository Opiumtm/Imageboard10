using System;
using Windows.Storage;
using Imageboard10.Core.Config;

namespace Imageboard10.Makaba.Network.Config
{
    /// <summary>
    /// Конфигурация для движка Makaba.
    /// </summary>
    /// <typeparam name="T">Тип интерфейса конфигурации.</typeparam>
    public abstract class MakabaRegistryConfigurationBase<T> : RegistryConfigurationBase<T>
        where T : class, IConfiguration
    {
        /// <summary>
        /// Имя контейнера.
        /// </summary>
        protected abstract string ConfigContainerName { get; }

        /// <summary>
        /// Перемещаемая конфигурация.
        /// </summary>
        protected virtual bool IsRoaming => false;

        /// <summary>
        /// Получить корневой контейнер.
        /// </summary>
        /// <returns>Корневой контейнер.</returns>
        protected sealed override ApplicationDataContainer GetСontainer()
        {
            var root = IsRoaming ? ApplicationData.Current.RoamingSettings : ApplicationData.Current.LocalSettings;
            // ReSharper disable once NotResolvedInText
            return root.CreateContainer("makaba", ApplicationDataCreateDisposition.Always).CreateContainer(ConfigContainerName ?? throw new ArgumentNullException("ConfigContainerName"), ApplicationDataCreateDisposition.Always);
        }
    }
}