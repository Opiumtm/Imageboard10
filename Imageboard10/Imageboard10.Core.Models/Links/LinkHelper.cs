﻿using System;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Класс-помощник для ссылок.
    /// </summary>
    public static class LinkHelper
    {
        /// <summary>
        /// Сериализовать ссылку.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <param name="modules">Модули.</param>
        /// <returns>Сериализованная ссылка.</returns>
        public static string Serialize(this ILink link, IModuleProvider modules)
        {
            if (modules == null) throw new ArgumentNullException(nameof(modules));
            if (link == null)
            {
                return null;
            }
            return (modules.QueryModule<ILinkSerializationService>() ?? throw new ModuleNotFoundException(typeof(ILinkSerializationService)))
                .Serialize(link);
        }

        /// <summary>
        /// Десериализовать ссылку.
        /// </summary>
        /// <param name="modules">Модули.</param>
        /// <param name="linkStr">Строка ссылки.</param>
        /// <returns>Ссылка.</returns>
        public static ILink DeserializeLink(this IModuleProvider modules, string linkStr)
        {
            if (modules == null) throw new ArgumentNullException(nameof(modules));
            if (linkStr == null)
            {
                return null;
            }
            return (modules.QueryModule<ILinkSerializationService>() ?? throw new ModuleNotFoundException(typeof(ILinkSerializationService)))
                .Deserialize(linkStr);
        }

        /// <summary>
        /// Клонировать ссылку.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <param name="modules">Модули.</param>
        /// <returns>Клонированная ссылка.</returns>
        public static ILink CloneLink(this ILink link, IModuleProvider modules)
        {
            if (modules == null) throw new ArgumentNullException(nameof(modules));
            if (link == null)
            {
                return null;
            }
            if (link is IDeepCloneable<BoardLinkBase> dc)
            {
                return dc.DeepClone(modules);
            }
            var serializer = modules.QueryModule<ILinkSerializationService>() ?? throw new ModuleNotFoundException(typeof(ILinkSerializationService));
            return serializer.Deserialize(serializer.Serialize(link));
        }
    }
}