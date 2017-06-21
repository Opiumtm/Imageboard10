using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Imageboard10.Core;
using Imageboard10.Core.Models.Posts;

namespace Imageboard10.Makaba.Models.Posts.Serialization
{
    /// <summary>
    /// Контракты.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class MakabaEntittyInfoModelContracts
    {
        /// <summary>
        /// Дополнительная информация о доске (в виде распарсенного HTML). Только для сериализации.
        /// </summary>
        [DataMember]
        public PostDocument BoardInfo { get; set; }

        /// <summary>
        /// Высота баннера.
        /// </summary>
        [DataMember]
        public int BannerHeight { get; set; }

        /// <summary>
        /// Ширина баннера.
        /// </summary>
        [DataMember]
        public int BannerWidth { get; set; }

        /// <summary>
        /// Есть баннер.
        /// </summary>
        [DataMember]
        public bool HasBannerSize { get; set; }

        /// <summary>
        /// Ссылка на изображение.
        /// </summary>
        [DataMember]
        public string BannerImageLink { get; set; }

        /// <summary>
        /// Доска.
        /// </summary>
        [DataMember]
        public string BannerBoardLink { get; set; }

        /// <summary>
        /// Иконки.
        /// </summary>
        [DataMember]
        public List<BoardIconContract> Icons { get; set; }

        /// <summary>
        /// Страницы.
        /// </summary>
        [DataMember]
        public List<int> Pages { get; set; }

        /// <summary>
        /// Новости.
        /// </summary>
        [DataMember]
        public List<MakabaBoardPostCollectionInfoNewsItemContract> NewsItems { get; set; }

        /// <summary>
        /// Доски.
        /// </summary>
        [DataMember]
        public List<MakabaBoardPostCollectionInfoBoardsAdvertisementItemContract> AdvertisementItems { get; set; }

        /// <summary>
        /// Ссылка на баннер.
        /// </summary>
        [DataMember]
        public string AdvertisementBannerLink { get; set; }

        /// <summary>
        /// Ссылка для перехода.
        /// </summary>
        [DataMember]
        public string AdvertisementClickLink { get; set; }

        /// <summary>
        /// Флаги.
        /// </summary>
        [DataMember]
        public List<Guid> Flags { get; set; }
    }
}