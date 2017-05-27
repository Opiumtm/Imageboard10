using System;
using System.Collections.Generic;
using Windows.UI;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Описание сетевого движка.
    /// </summary>
    public interface INetworkEngineDescription
    {
        /// <summary>
        /// Возможности.
        /// </summary>
        IList<Guid> Capabilities { get; }

        /// <summary>
        /// Имя движка.
        /// </summary>
        string EngineName { get; }

        /// <summary>
        /// Имя ресурса.
        /// </summary>
        string ResourceName { get; }

        /// <summary>
        /// Цвет плитки.
        /// </summary>
        Color TileBackgroundColor { get; }

        /// <summary>
        /// Цвет фона по умолчанию.
        /// </summary>
        Color DefaultBackgroundColor { get; }

        /// <summary>
        /// Корневая ссылка.
        /// </summary>
        ILink RootLink { get; }
    }
}