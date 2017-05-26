﻿namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Ссылка с движком.
    /// </summary>
    public interface IEngineLink
    {
        /// <summary>
        /// Движок.
        /// </summary>
        string Engine { get; }

        /// <summary>
        /// Получить корневую ссылку.
        /// </summary>
        /// <returns>Корневая ссылка.</returns>
        BoardLinkBase GetRootLink();
    }
}