using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Windows.Graphics;
using Imageboard10.Core;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models;
using Imageboard10.Core.Models.Boards;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.Modules;
using Imageboard10.Makaba.Models.Posts.Serialization;

namespace Imageboard10.Makaba.Models.Posts
{
    /// <summary>
    /// Модель сущности makaba.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class MakabaEntityInfoModel : IBoardPostCollectionInfo, IBoardPostCollectionInfoSet,
        IBoardPostCollectionInfoBoard,
        IBoardPostCollectionInfoBoardDesc,
        IBoardPostCollectionInfoBoardBanner,
        IBoardPostCollectionInfoPostingSpeed,
        IBoardPostCollectionInfoLocation,
        IBoardPostCollectionInfoIcons,
        IBoardPostCollectionInfoBoardLimits,
        IBoardPostCollectionInfoNews,
        IBoardPostCollectionInfoBoardsAdvertisement,
        IBoardPostCollectionInfoBottomAdvertisement,
        IBoardPostCollectionInfoFlags,
        IBoardPostCollectionInfoUniquePosters,
        IBoardPostCollectionInfoTitle,
        IBoardPostCollectionInfoCatalogFilter
    {
        private static readonly List<Type> InterfaceTypes = new List<Type>()
        {
            typeof(IBoardPostCollectionInfoBoard),
            typeof(IBoardPostCollectionInfoBoardDesc),
            typeof(IBoardPostCollectionInfoBoardBanner),
            typeof(IBoardPostCollectionInfoPostingSpeed),
            typeof(IBoardPostCollectionInfoLocation),
            typeof(IBoardPostCollectionInfoIcons),
            typeof(IBoardPostCollectionInfoBoardLimits),
            typeof(IBoardPostCollectionInfoNews),
            typeof(IBoardPostCollectionInfoBoardsAdvertisement),
            typeof(IBoardPostCollectionInfoBottomAdvertisement),
            typeof(IBoardPostCollectionInfoFlags),
            typeof(IBoardPostCollectionInfoUniquePosters),
            typeof(IBoardPostCollectionInfoTitle),
            typeof(IBoardPostCollectionInfoCatalogFilter)
        };

        /// <summary>
        /// Конструктор.
        /// </summary>
        public MakabaEntityInfoModel()
        {
            var r = new List<IBoardPostCollectionInfo>() { this };
            _infoSet = r.AsReadOnly();
        }

        /// <summary>
        /// Контракты.
        /// </summary>
        [DataMember]
        public MakabaEntittyInfoModelContracts Contracts { get; set; }

        /// <summary>
        /// Получить тип для сериализации.
        /// </summary>
        /// <returns>Тип для сериализации.</returns>
        public Type GetTypeForSerializer() => GetType();

        private static readonly IReadOnlyList<Type> InterfaceTypesReadonly = InterfaceTypes.AsReadOnly();

        /// <summary>
        /// Получить типs интерфейсов дополнительной информации.
        /// </summary>
        /// <returns>Типы интерфейса дополнительной информации.</returns>
        public IReadOnlyList<Type> GetInfoInterfaceTypes() => InterfaceTypesReadonly;

        /// <summary>
        /// Доска.
        /// </summary>
        [DataMember]
        public string Board { get; set; }

        /// <summary>
        /// Текущая страница.
        /// </summary>
        [DataMember]
        public int? CurrentPage { get; set; }

        /// <summary>
        /// Текущий тред.
        /// </summary>
        [DataMember]
        public int? CurrentThread { get; set; }

        /// <summary>
        /// Максимальный номер поста.
        /// </summary>
        [DataMember]
        public int? MaxPostNumber { get; set; }

        /// <summary>
        /// Имя доски.
        /// </summary>
        [DataMember]
        public string BoardName { get; set; }

        /// <summary>
        /// Дополнительная информация о доске (в виде распарсенного HTML).
        /// </summary>
        [IgnoreDataMember]
        public IPostDocument BoardInfo { get; set; }

        /// <summary>
        /// Дополнительная информация о доске (Outer?).
        /// </summary>
        [DataMember]
        public string BoardInfoOuter { get; set; }

        /// <summary>
        /// Размер баннера.
        /// </summary>
        [IgnoreDataMember]
        public SizeInt32? BannerSize { get; set; }

        /// <summary>
        /// Ссылка на изображение.
        /// </summary>
        [IgnoreDataMember]
        public ILink BannerImageLink { get; set; }

        /// <summary>
        /// Доска.
        /// </summary>
        [IgnoreDataMember]
        public ILink BannerBoardLink { get; set; }

        /// <summary>
        /// Скорость.
        /// </summary>
        [DataMember]
        public int Speed { get; set; }

        /// <summary>
        /// Иконки.
        /// </summary>
        [IgnoreDataMember]
        public IList<IBoardIcon> Icons { get; set; }

        /// <summary>
        /// Страницы доски (начиная с 0).
        /// </summary>
        [IgnoreDataMember]
        public IList<int> Pages { get; set; }

        /// <summary>
        /// Имя по умолчанию.
        /// </summary>
        [DataMember]
        public string DefaultName { get; set; }

        /// <summary>
        /// Максимальный размер комментария.
        /// </summary>
        [DataMember]
        public int? MaxComment { get; set; }

        /// <summary>
        /// Максимальный размер файлов.
        /// </summary>
        [DataMember]
        public ulong? MaxFilesSize { get; set; }

        /// <summary>
        /// Новости.
        /// </summary>
        [IgnoreDataMember]
        public IList<IBoardPostCollectionInfoNewsItem> NewsItems { get; set; }

        /// <summary>
        /// Доски.
        /// </summary>
        [IgnoreDataMember]
        public IList<IBoardPostCollectionInfoBoardsAdvertisementItem> AdvertisementItems { get; set; }

        /// <summary>
        /// Ссылка на баннер.
        /// </summary>
        [IgnoreDataMember]
        public ILink AdvertisementBannerLink { get; set; }

        /// <summary>
        /// Ссылка для перехода.
        /// </summary>
        [IgnoreDataMember]
        public ILink AdvertisementClickLink { get; set; }

        /// <summary>
        /// Флаги.
        /// </summary>
        [IgnoreDataMember]
        public IList<Guid> Flags { get; set; }

        /// <summary>
        /// Уникальный постеров.
        /// </summary>
        [DataMember]
        public int? UniquePosters { get; set; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Фильтр каталога.
        /// </summary>
        [DataMember]
        public string CatalogFilter { get; set; }

        internal void BeforeSerialize(IModuleProvider moduleProvider)
        {
            var linkSerialization = moduleProvider.QueryModule<ILinkSerializationService>() ?? throw new ModuleNotFoundException(typeof(ILinkSerializationService));
            Contracts = new MakabaEntittyInfoModelContracts()
            {
                BoardInfo = moduleProvider.ValidateBeforeSerialize<IPostDocument, PostDocument, PostDocumentExternalContract>(BoardInfo),
                HasBannerSize = BannerSize != null,
                BannerWidth = BannerSize != null ? BannerSize.Value.Width : 0,
                BannerHeight = BannerSize != null ? BannerSize.Value.Height : 0,
                BannerImageLink = linkSerialization.Serialize(BannerImageLink),
                BannerBoardLink = linkSerialization.Serialize(BannerBoardLink),
                Icons = Icons?.Select(i => i != null ? new BoardIconContract()
                {
                    Id = i.Id,
                    Name = i.Name,
                    MediaLink = linkSerialization.Serialize(i.MediaLink)
                } : null)?.ToList(),
                Pages = Pages?.ToList(),
                NewsItems = NewsItems?.Select(i => i != null ? new MakabaBoardPostCollectionInfoNewsItemContract()
                {
                    Date = i.Date,
                    NewsLink = linkSerialization.Serialize(i.NewsLink),
                    Title = i.Title
                } : null)?.ToList(),
                AdvertisementItems = AdvertisementItems?.Select(i => i != null ? new MakabaBoardPostCollectionInfoBoardsAdvertisementItemContract()
                {
                    Info = i.Info,
                    Name = i.Name,
                    BoardLink = linkSerialization.Serialize(i.BoardLink)
                } : null)?.ToList(),
                AdvertisementBannerLink = linkSerialization.Serialize(AdvertisementBannerLink),
                AdvertisementClickLink = linkSerialization.Serialize(AdvertisementClickLink),
                Flags = Flags?.ToList()
            };
        }

        internal void AfterDeserialize(IModuleProvider moduleProvider)
        {
            var linkSerialization = moduleProvider.QueryModule<ILinkSerializationService>() ?? throw new ModuleNotFoundException(typeof(ILinkSerializationService));
            if (Contracts == null)
            {
                throw new SerializationException("MakabaEntityInfoModel: Неправильное содержимое сериализованных данных.");
            }

            BoardInfo = moduleProvider.ValidateAfterDeserialize<PostDocument, IPostDocument, PostDocumentExternalContract>(Contracts.BoardInfo);
            if (Contracts.HasBannerSize)
            {
                BannerSize = new SizeInt32() { Width = Contracts.BannerWidth, Height = Contracts.BannerHeight };
            }
            else
            {
                BannerSize = null;
            }
            BannerImageLink = linkSerialization.Deserialize(Contracts.BannerImageLink);
            BannerBoardLink = linkSerialization.Deserialize(Contracts.BannerBoardLink);
            Icons = Contracts.Icons?.Select(i => i != null ? new BoardIcon()
            {
                Id = i.Id,
                Name = i.Name,
                MediaLink = linkSerialization.Deserialize(i.MediaLink)
            } : null).OfType<IBoardIcon>().ToList();
            Pages = Contracts.Pages;
            NewsItems = Contracts.NewsItems?.Select(i => i != null ? new MakabaBoardPostCollectionInfoNewsItem()
            {
                Date = i.Date,
                Title = i.Title,
                NewsLink = linkSerialization.Deserialize(i.NewsLink)
            } : null).OfType<IBoardPostCollectionInfoNewsItem>().ToList();
            AdvertisementItems = Contracts.AdvertisementItems?.Select(i => i != null ? new MakabaBoardPostCollectionInfoBoardsAdvertisementItem()
            {
                BoardLink = linkSerialization.Deserialize(i.BoardLink),
                Name = i.Name,
                Info = i.Info
            } : null).OfType<IBoardPostCollectionInfoBoardsAdvertisementItem>().ToList();
            AdvertisementBannerLink = linkSerialization.Deserialize(Contracts.AdvertisementBannerLink);
            AdvertisementClickLink = linkSerialization.Deserialize(Contracts.AdvertisementClickLink);
            Flags = Contracts.Flags;

            Contracts = null;
            if (_infoSet == null)
            {
                var r = new List<IBoardPostCollectionInfo>() { this };
                _infoSet = r.AsReadOnly();
            }
        }

        [IgnoreDataMember]
        private IReadOnlyList<IBoardPostCollectionInfo> _infoSet;

        /// <summary>
        /// Элементы.
        /// </summary>
        [IgnoreDataMember]
        IReadOnlyList<IBoardPostCollectionInfo> IBoardPostCollectionInfoSet.Items => _infoSet;
    }
}