using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка на часть треда.
    /// </summary>
    public class ThreadPartLink : ThreadLink
    {
        /// <summary>
        /// Начиная с поста.
        /// </summary>
        public int FromPost { get; set; }

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.PartialThread;

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new ThreadPartLink()
        {
            Engine = Engine,
            Board = Board,
            OpPostNum = OpPostNum,
            FromPost = FromPost
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
                    return $"{Engine}://{Board}/{OpPostNum}[{FromPost}]";
                case LinkDisplayStringContext.Engine:
                    return $"/{Board}/{OpPostNum}[{FromPost}]";
                case LinkDisplayStringContext.Board:
                    return $"{OpPostNum}[{FromPost}]";
                default:
                    return $"[{FromPost}]";
            }
        }

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"threadpart-{Engine}-{Board}-{OpPostNum}-{FromPost}";

        /// <summary>
        /// Получить значения для сравнения.
        /// </summary>
        /// <returns>Значения для сравнения.</returns>
        public override LinkCompareValues GetCompareValues() => new LinkCompareValues()
        {
            Board = Board,
            Engine = Engine,
            Page = 0,
            Post = OpPostNum,
            Thread = OpPostNum,
            Other = $"{FromPost}"
        };
    }
}