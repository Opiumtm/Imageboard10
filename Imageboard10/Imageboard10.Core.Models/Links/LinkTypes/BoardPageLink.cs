namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка на страницу доски.
    /// </summary>
    public class BoardPageLink : BoardLink
    {
        /// <summary>
        /// Номер страницы.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.BoardPage;

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new BoardPageLink()
        {
            Engine = Engine,
            Board = Board,
            Page = Page
        };

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"boardpage-{Engine}-{Board}-{Page}";

        /// <summary>
        /// Получить значения для сравнения.
        /// </summary>
        /// <returns>Значения для сравнения.</returns>
        public override LinkCompareValues GetCompareValues() => new LinkCompareValues()
        {
            Engine = Engine,
            Board = Board,
            Other = "",
            Post = 0,
            Thread = 0,
            Page = Page
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
                    return $"{Engine}://{Board}#{Page}";
                default:
                    return $"/{Board}#{Page}";
            }
        }
    }
}