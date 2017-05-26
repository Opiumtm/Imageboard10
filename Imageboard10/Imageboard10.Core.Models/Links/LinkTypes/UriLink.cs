namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка на медиа.
    /// </summary>
    public class UriLink : BoardLinkBase, IUriLink
    {
        /// <summary>
        /// URI.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.External | BoardLinkKind.Other;

        public override BoardLinkBase DeepClone() => new UriLink()
        {
            Uri = Uri
        };

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"uri-{Uri?.ToLowerInvariant()}";

        /// <summary>
        /// Получить значения для сравнения.
        /// </summary>
        /// <returns>Значения для сравнения.</returns>
        public override LinkCompareValues GetCompareValues() => new LinkCompareValues()
        {
            Engine = "",
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
        public override string GetDisplayString(LinkDisplayStringContext context) => Uri ?? "";

        /// <summary>
        /// Абсолютная ссылка.
        /// </summary>
        public bool IsAbsolute => true;

        /// <summary>
        /// Получить абсолютную ссылку.
        /// </summary>
        /// <returns>Абсолютная ссылка.</returns>
        public string GetAbsoluteUrl() => Uri;

        /// <summary>
        /// Получить идентификатор, "дружественный" файловой системе.
        /// </summary>
        /// <returns>Идентификатор.</returns>
        public override string GetFilesystemFriendlyId() => $"uri-{Utility.StringHashCache.GetHashId((Uri ?? "").ToLowerInvariant())}";
    }
}