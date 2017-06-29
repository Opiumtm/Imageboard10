using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка на медиа в рамках доски.
    /// </summary>
    public class BoardMediaLink : BoardLink, IUriLink
    {
        /// <summary>
        /// URI.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.Media;

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new BoardMediaLink()
        {
            Engine = Engine,
            Board = Board,
            Uri = Uri,
        };

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"boardmedia-{Engine}-{Uri?.ToLowerInvariant()}";

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
            Other = Uri ?? ""
        };

        /// <summary>
        /// Получить строку для отображения.
        /// </summary>
        /// <param name="context">Контекст изображения.</param>
        /// <returns>Строка для отображения.</returns>
        public override string GetDisplayString(LinkDisplayStringContext context) => $"{Engine}://{Board}{Uri}";

        /// <summary>
        /// Абсолютная ссылка.
        /// </summary>
        public bool IsAbsolute => false;

        /// <summary>
        /// Получить абсолютную ссылку.
        /// </summary>
        /// <returns>Абсолютная ссылка.</returns>
        public string GetAbsoluteUrl() => null;

        /// <summary>
        /// Получить идентификатор, "дружественный" файловой системе.
        /// </summary>
        /// <returns>Идентификатор.</returns>
        public override string GetFilesystemFriendlyId() => $"boardmedia-{Engine}-{Board}-{Utility.StringHashCache.GetHashId((Uri ?? "").ToLowerInvariant())}";
    }
}