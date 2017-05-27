using System;
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
            if (link == null) throw new ArgumentNullException(nameof(link));
            if (modules == null) throw new ArgumentNullException(nameof(modules));
            return (modules.QueryModule<ILinkSerializationService>() ?? throw new ModuleNotFoundException())
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
            if (linkStr == null) throw new ArgumentNullException(nameof(linkStr));
            return (modules.QueryModule<ILinkSerializationService>() ?? throw new ModuleNotFoundException())
                .Deserialize(linkStr);
        }
    }
}