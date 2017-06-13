using System;

namespace Imageboard10.Core.Network
{
    /// <summary>
    /// Контекст получения ссылок.
    /// </summary>
    public static class UriGetterContext
    {
        /// <summary>
        /// Ссылка на HTML-версию.
        /// </summary>
        public static readonly Guid HtmlLink = new Guid("{E60C5E42-0493-4932-8E06-8B8E6586A8A4}");

        /// <summary>
        /// Ссылка на метод JSON API.
        /// </summary>
        public static readonly Guid JsonApiLink = new Guid("{49429FEE-45B0-4A00-822A-8003330633DA}");
    }
}