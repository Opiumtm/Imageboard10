using System;
using System.Collections.Generic;
using Windows.UI;
using Imageboard10.Core.ModelInterface.Boards;
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

        /// <summary>
        /// Получить описание доски по умолчанию.
        /// </summary>
        /// <param name="category">Категория.</param>
        /// <param name="boardLink">Ссылка на доску.</param>
        /// <returns>Описание по умолчанию.</returns>
        IBoardReference GetDefaultBoardReference(string category, ILink boardLink);

        /// <summary>
        /// Создать ссылку на доску.
        /// </summary>
        /// <param name="id">Идентификатор доски.</param>
        /// <returns>Ссылка на доску.</returns>
        ILink CreateBoardLink(string id);
    }
}