namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка в рамках движка.
    /// </summary>
    public class EngineUriLink : EngineLinkBase, IUriLink
    {
        /// <summary>
        /// URI.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.Other;

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new EngineUriLink()
        {
            Engine = Engine,
            Uri = Uri
        };

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"engineuri-{Engine}-{Uri}";

        /// <summary>
        /// Получить значения для сравнения.
        /// </summary>
        /// <returns>Значения для сравнения.</returns>
        public override LinkCompareValues GetCompareValues() => new LinkCompareValues()
        {
            Engine = Engine,
            Board = "",
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
        public override string GetDisplayString(LinkDisplayStringContext context) => $"{Engine}:/{(Uri ?? "").ToLowerInvariant()}";

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
        public override string GetFilesystemFriendlyId() => $"uri-{Engine}-{Utility.StringHashCache.GetHashId((Uri ?? "").ToLowerInvariant())}";

    }
}