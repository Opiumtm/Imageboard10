using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка на доску.
    /// </summary>
    public class BoardLink : EngineLinkBase, IBoardLink
    {
        /// <summary>
        /// Доска.
        /// </summary>
        public string Board { get; set; }

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.BoardPage;

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new BoardLink()
        {
            Engine = Engine,
            Board = Board
        };

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"board-{Engine}-{Board}";

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
            Page = 0
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
                    return $"{Engine}://{Board}";
                default:
                    return $"/{Board}";
            }
        }

        /// <summary>
        /// Получить ссылку на борду.
        /// </summary>
        /// <returns>Ссылка на борду.</returns>
        public ILink GetBoardLink() => GetType() == typeof(BoardLink) ? this : new BoardLink() { Engine = Engine, Board = Board };

        /// <summary>
        /// Получить ссылку на страницу доски.
        /// </summary>
        /// <param name="page">Страница.</param>
        /// <returns></returns>
        public ILink GetBoardPageLink(int page) => new BoardPageLink() { Engine = Engine, Board = Board, Page = page };
    }
}