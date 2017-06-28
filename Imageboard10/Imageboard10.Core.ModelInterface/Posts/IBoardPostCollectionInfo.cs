using System;
using System.Collections.Generic;
using Windows.Graphics;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Набор информации о коллекции постов.
    /// </summary>
    public interface IBoardPostCollectionInfoSet : ISerializableObject
    {
        /// <summary>
        /// Элементы.
        /// </summary>
        IReadOnlyList<IBoardPostCollectionInfo> Items { get; }
    }

    /// <summary>
    /// Дополнительная информация о коллекции постов.
    /// </summary>
    public interface IBoardPostCollectionInfo : ISerializableObject
    {
        /// <summary>
        /// Получить типs интерфейсов дополнительной информации.
        /// </summary>
        /// <returns>Типы интерфейса дополнительной информации.</returns>
        IReadOnlyList<Type> GetInfoInterfaceTypes();
    }

    /// <summary>
    /// Информация о доске.
    /// </summary>
    public interface IBoardPostCollectionInfoBoard : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Доска.
        /// </summary>
        string Board { get; }

        /// <summary>
        /// Имя доски.
        /// </summary>
        string BoardName { get; }
    }

    
    /// <summary>
    /// Описание доски.
    /// </summary>
    public interface IBoardPostCollectionInfoBoardDesc : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Дополнительная информация о доске (в виде распарсенного HTML).
        /// </summary>
        IPostDocument BoardInfo { get; }

        /// <summary>
        /// Дополнительная информация о доске (Outer?).
        /// </summary>
        string BoardInfoOuter { get; }
    }

    /// <summary>
    /// Баннер с рекламой другой доски.
    /// </summary>
    public interface IBoardPostCollectionInfoBoardBanner : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Размер баннера.
        /// </summary>
        SizeInt32? BannerSize { get; }

        /// <summary>
        /// Ссылка на изображение.
        /// </summary>
        ILink BannerImageLink { get; }

        /// <summary>
        /// Доска.
        /// </summary>
        ILink BannerBoardLink { get; }
    }

    /// <summary>
    /// Скорость постинга.
    /// </summary>
    public interface IBoardPostCollectionInfoPostingSpeed : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Скорость постинга (постов в час).
        /// </summary>
        int Speed { get; }
    }

    /// <summary>
    /// Расположение.
    /// </summary>
    public interface IBoardPostCollectionInfoLocation : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Доска.
        /// </summary>
        string Board { get; }

        /// <summary>
        /// Текущая страница.
        /// </summary>
        int? CurrentPage { get; }

        /// <summary>
        /// Текущий тред.
        /// </summary>
        int? CurrentThread { get; }

        /// <summary>
        /// Максимальный номер поста.
        /// </summary>
        int? MaxPostNumber { get; }
    }

    /// <summary>
    /// Иконки.
    /// </summary>
    public interface IBoardPostCollectionInfoIcons : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Иконки.
        /// </summary>
        IList<IBoardIcon> Icons { get; }
    }

    /// <summary>
    /// Флаги.
    /// </summary>
    public interface IBoardPostCollectionInfoFlags : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Флаги.
        /// </summary>
        IList<Guid> Flags { get; }
    }

    /// <summary>
    /// Значения по доске.
    /// </summary>
    public interface IBoardPostCollectionInfoBoardLimits : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Страницы доски (начиная с 0).
        /// </summary>
        IList<int> Pages { get; }

        /// <summary>
        /// Имя по умолчанию.
        /// </summary>
        string DefaultName { get; }

        /// <summary>
        /// Максимальный размер комментария.
        /// </summary>
        int? MaxComment { get; }

        /// <summary>
        /// Максимальный размер файлов.
        /// </summary>
        ulong? MaxFilesSize { get; }
    }

    /// <summary>
    /// Новости.
    /// </summary>
    public interface IBoardPostCollectionInfoNews : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Новости.
        /// </summary>
        IList<IBoardPostCollectionInfoNewsItem> NewsItems { get; }
    }

    /// <summary>
    /// Элемент новостей.
    /// </summary>
    public interface IBoardPostCollectionInfoNewsItem
    {
        /// <summary>
        /// Дата.
        /// </summary>
        string Date { get; }

        /// <summary>
        /// Ссылка на новость.
        /// </summary>
        ILink NewsLink { get; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        string Title { get; }
    }

    /// <summary>
    /// Реклама досок.
    /// </summary>
    public interface IBoardPostCollectionInfoBoardsAdvertisement : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Доски.
        /// </summary>
        IList<IBoardPostCollectionInfoBoardsAdvertisementItem> AdvertisementItems { get; }
    }

    /// <summary>
    /// Реклама доски.
    /// </summary>
    public interface IBoardPostCollectionInfoBoardsAdvertisementItem
    {
        /// <summary>
        /// Ссылка на доску.
        /// </summary>
        ILink BoardLink { get; }

        /// <summary>
        /// Имя.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Информация.
        /// </summary>
        string Info { get; }
    }

    /// <summary>
    /// Нижний рекламный баннер.
    /// </summary>
    public interface IBoardPostCollectionInfoBottomAdvertisement : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Ссылка на баннер.
        /// </summary>
        ILink AdvertisementBannerLink { get; }

        /// <summary>
        /// Ссылка для перехода.
        /// </summary>
        ILink AdvertisementClickLink { get; }
    }

    /// <summary>
    /// Уникальных постеров.
    /// </summary>
    public interface IBoardPostCollectionInfoUniquePosters : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Уникальный постеров.
        /// </summary>
        int? UniquePosters { get; }
    }

    /// <summary>
    /// Заголовок.
    /// </summary>
    public interface IBoardPostCollectionInfoTitle : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Заголовок.
        /// </summary>
        string Title { get; }
    }

    /// <summary>
    /// Фильтр каталога.
    /// </summary>
    public interface IBoardPostCollectionInfoCatalogFilter : IBoardPostCollectionInfo
    {
        /// <summary>
        /// Фильтр каталога.
        /// </summary>
        string CatalogFilter { get; }
    }

}