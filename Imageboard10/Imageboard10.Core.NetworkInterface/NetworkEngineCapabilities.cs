using System;

namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Возможности сетевого движка.
    /// </summary>
    public static class NetworkEngineCapabilities
    {
        /// <summary>
        /// Запрос на часть треда.
        /// </summary>
        public static Guid PartialThreadRequest { get; } = new Guid("{597EC4CE-A367-4EF3-9CFA-D7C318CD35E2}");

        /// <summary>
        /// Запрос к статусу треда.
        /// </summary>
        public static Guid ThreadStatusRequest { get; } = new Guid("{B61E7987-785F-45DE-A436-852F74D50B6E}");

        /// <summary>
        /// Запрос к списку борд.
        /// </summary>
        public static Guid BoardsListRequest { get; } = new Guid("{815C1AA1-34B2-4385-BEE9-3D0B5F5C23FC}");

        /// <summary>
        /// Поиск по борде.
        /// </summary>
        public static Guid SearchRequest { get; } = new Guid("{791822A6-7560-486E-ADFE-64CBF1472B28}");

        /// <summary>
        /// Каталог топ постов.
        /// </summary>
        public static Guid TopPostsRequest { get; } = new Guid("{8F983BCC-7432-4AC6-8FD5-3D5399554669}");

        /// <summary>
        /// Запрос на последнее изменение (last modified header).
        /// </summary>
        public static Guid LastModifiedRequest { get; } = new Guid("{F7C2BCEB-0D5D-4486-AE0B-6C9B247A921D}");

        /// <summary>
        /// Без ввода капчи.
        /// </summary>
        public static Guid NoCaptcha { get; } = new Guid("{66AA3FB3-7A93-44C7-A443-8EC7A4D78905}");

        /// <summary>
        /// Каталог тредов.
        /// </summary>
        public static Guid Catalog { get; } = new Guid("{175DB178-4108-4402-8EC1-72AB844C5DF2}");

        /// <summary>
        /// Загрузить отдельный пост.
        /// </summary>
        public static Guid GetSinglePost { get; } = new Guid("{DE547CED-A928-4EC2-B13B-BB4F07A24F03}");

        /// <summary>
        /// Поддержка лайков.
        /// </summary>
        public static Guid Likes { get; } = new Guid("{6A717B23-9C72-44A1-9FB2-230F21A9861D}");
    }
}