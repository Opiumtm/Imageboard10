﻿namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка, привязанная к движку имиджборды.
    /// </summary>
    public abstract class EngineLinkBase : BoardLinkBase, IEngineLink
    {
        /// <summary>
        /// Движок.
        /// </summary>
        public string Engine { get; set; }

        /// <summary>
        /// Получить корневую ссылку.
        /// </summary>
        /// <returns>Корневая ссылка.</returns>
        public virtual BoardLinkBase GetRootLink() => new RootLink() { Engine = Engine };
    }
}