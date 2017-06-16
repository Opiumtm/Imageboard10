using System;
using Windows.Storage;
using Imageboard10.Core.Config;

namespace Imageboard10.Makaba.Network.Config
{
    /// <summary>
    /// Сетевая конфигурация Makaba.
    /// </summary>
    public sealed class MakabaNetworkConfig : MakabaRegistryConfigurationBase<IMakabaNetworkConfig>, IMakabaNetworkConfig
    {
        /// <summary>
        /// Сохранить конфигурацию.
        /// </summary>
        /// <param name="container">Контейнер.</param>
        protected override void SaveConfiguration(ApplicationDataContainer container)
        {
            container.Values["baseUri"] = BaseUri?.ToString() ?? "https://2ch.hk/";
        }

        /// <summary>
        /// Загрузить конфигурацию.
        /// </summary>
        /// <param name="container">Контейнер.</param>
        protected override void LoadConfiguration(ApplicationDataContainer container)
        {
            try
            {
                BaseUri = (container.Values.ContainsKey("baseUri") ? new System.Uri((string) container.Values["baseUri"]) : null) ?? new System.Uri("https://2ch.hk/");
            }
            catch
            {
                BaseUri = new System.Uri("https://2ch.hk/");
            }
        }

        private System.Uri _baseUri;

        /// <summary>
        /// Базовый URI.
        /// </summary>
        public System.Uri BaseUri
        {
            get => _baseUri;
            set => _baseUri = value ?? new System.Uri("https://2ch.hk/");
        }

        /// <summary>
        /// Имя контейнера.
        /// </summary>
        protected override string ConfigContainerName => "network";
    }
}