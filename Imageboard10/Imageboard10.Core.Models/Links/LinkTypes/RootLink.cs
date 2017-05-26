namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Корневая ссылка.
    /// </summary>
    public class RootLink : EngineLinkBase
    {
        public override BoardLinkKind LinkKind => BoardLinkKind.Other;

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new RootLink()
        {
            Engine = Engine
        };

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"root-{Engine}";

        /// <summary>
        /// Получить значения для сравнения.
        /// </summary>
        /// <returns>Значения для сравнения.</returns>
        public override LinkCompareValues GetCompareValues() => new LinkCompareValues()
        {
            Engine = Engine,
            Board = "",
            Other = "",
            Page = 0,
            Post = 0,
            Thread = 0
        };

        /// <summary>
        /// Получить строку для отображения.
        /// </summary>
        /// <param name="context">Контекст изображения.</param>
        /// <returns>Строка для отображения.</returns>
        public override string GetDisplayString(LinkDisplayStringContext context) => $"{Engine}://";

        /// <summary>
        /// Получить корневую ссылку.
        /// </summary>
        /// <returns>Корневая ссылка.</returns>
        public override BoardLinkBase GetRootLink() => this;
    }
}