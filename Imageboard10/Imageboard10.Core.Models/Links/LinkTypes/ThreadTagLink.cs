using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка на тэг тредов на доске.
    /// </summary>
    public class ThreadTagLink : BoardLink, IThreadTagLink
    {
        /// <summary>
        /// Тэг.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new ThreadTagLink()
        {
            Engine = Engine,
            Board = Board,
            Tag = Tag
        };

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"tag-{Board}-{Engine}-{Tag?.ToLowerInvariant()}";

        /// <summary>
        /// Получить идентификатор, "дружественный" файловой системе.
        /// </summary>
        /// <returns>Идентификатор.</returns>
        public override string GetFilesystemFriendlyId() => $"tag-{Board}-{Engine}-{Utility.StringHashCache.GetHashId((Tag ?? "").ToLowerInvariant())}";

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
            Other = Tag ?? ""
        };

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.ThreadTag;

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
                    return $"{Engine}://{Board}#{Tag}";
                default:
                    return $"/{Board}#{Tag}";
            }
        }
    }
}