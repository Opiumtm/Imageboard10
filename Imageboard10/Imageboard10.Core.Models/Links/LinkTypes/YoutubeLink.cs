using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка на ютуб.
    /// </summary>
    public class YoutubeLink : BoardLinkBase, IYoutubeLink
    {
        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.Youtube | BoardLinkKind.External;

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new YoutubeLink()
        {
            YoutubeId = YoutubeId
        };

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"youtube-{YoutubeId}";

        /// <summary>
        /// Получить идентификатор, "дружественный" файловой системе.
        /// </summary>
        /// <returns>Идентификатор.</returns>
        public override string GetFilesystemFriendlyId() => $"youtube-{Utility.StringHashCache.GetHashId(YoutubeId ?? "")}";

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
            Other = YoutubeId ?? ""
        };

        /// <summary>
        /// Получить строку для отображения.
        /// </summary>
        /// <param name="context">Контекст изображения.</param>
        /// <returns>Строка для отображения.</returns>
        public override string GetDisplayString(LinkDisplayStringContext context) => $"youtube://{YoutubeId}";

        /// <summary>
        /// Идентификатор ютуба.
        /// </summary>
        public string YoutubeId { get; set; }

        /// <summary>
        /// Получить URI предпросмотра.
        /// </summary>
        /// <returns>URI картинки предпросмотра.</returns>
        public string GetThumbnailUri() => $"http://i.ytimg.com/vi/{YoutubeId}/0.jpg";

        /// <summary>
        /// Абсолютная ссылка.
        /// </summary>
        public bool IsAbsolute => true;

        /// <summary>
        /// Получить абсолютную ссылку.
        /// </summary>
        /// <returns>Абсолютная ссылка.</returns>
        public string GetAbsoluteUrl() => $"http://www.youtube.com/watch?v={YoutubeId}";
    }
}