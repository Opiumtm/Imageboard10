using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка на каталог.
    /// </summary>
    public class CatalogLink : BoardLink, ICatalogLink
    {
        /// <summary>
        /// Режим сортировки.
        /// </summary>
        public BoardCatalogSort SortMode { get; set; }

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new CatalogLink()
        {
            Engine = Engine,
            Board = Board,
            SortMode = SortMode
        };

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"catalog-{Engine}-{Board}-{(int)SortMode}";

        /// <summary>
        /// Получить значения для сравнения.
        /// </summary>
        /// <returns>Значения для сравнения.</returns>
        public override LinkCompareValues GetCompareValues() => new LinkCompareValues()
        {
            Engine = Engine,
            Board = Board,
            Page = 0,
            Post = 0,
            Thread = 0,
            Other = ((int)SortMode).ToString()
        };

        /// <summary>
        /// Получить строку для отображения.
        /// </summary>
        /// <param name="context">Контекст изображения.</param>
        /// <returns>Строка для отображения.</returns>
        public override string GetDisplayString(LinkDisplayStringContext context)
        {
            switch (context)
            {
                case LinkDisplayStringContext.None:
                    return $"{Engine}://{Board}#cat{GetSortSuffix()}";
                default:
                    return $"/{Board}#cat{GetSortSuffix()}";
            }
        }

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.Catalog;

        private string GetSortSuffix()
        {
            switch (SortMode)
            {
                case BoardCatalogSort.Bump:
                    return "b";
                case BoardCatalogSort.CreateDate:
                    return "d";
                default:
                    return "";
            }
        }
    }
}