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
        public static readonly Guid ApiGet = new Guid("{49429FEE-45B0-4A00-822A-8003330633DA}");

        /// <summary>
        /// Превью изображения.
        /// </summary>
        public static readonly Guid ThumbnailLink = new Guid("{B4BFE9AD-5580-4FB9-AC03-1177AE1B5194}");

        /// <summary>
        /// Вызов приложения YouTube.
        /// </summary>
        public static readonly Guid AppLaunchLink = new Guid("{AB6D0A15-2B6D-4656-A302-F1F72E94D685}");

        /// <summary>
        /// Количество постов в треде.
        /// </summary>
        public static readonly Guid ApiThreadPostCount = new Guid("{F919F82F-65CF-4719-B4A9-722B3C27F3C1}");

        /// <summary>
        /// Список досок.
        /// </summary>
        public static readonly Guid ApiBoardsList = new Guid("{DB2D7E0A-9DEA-4AF3-839E-A7D212469279}");

        /// <summary>
        /// Проверка.
        /// </summary>
        public static readonly Guid ApiCheck = new Guid("{68C49B20-FA44-4AC1-A78F-745C7A2BE44F}");

        /// <summary>
        /// Постинг.
        /// </summary>
        public static readonly Guid ApiPost = new Guid("{C800C1AA-883C-431D-8C21-DFDE336A3E01}");
    }
}