using System;

namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка на тред.
    /// </summary>
    public class ThreadLink : BoardLink, IThreadLink
    {
        /// <summary>
        /// Номер ОП-поста.
        /// </summary>
        public int OpPostNum { get; set; }

        /// <summary>
        /// Получить значения для сравнения.
        /// </summary>
        /// <returns>Значения для сравнения.</returns>
        public override LinkCompareValues GetCompareValues() => new LinkCompareValues()
        {
            Engine = Engine,
            Board = Board,
            Other = "",
            Thread = OpPostNum,
            Post = OpPostNum,
            Page = 0
        };

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.Thread;

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new ThreadLink()
        {
            Engine = Engine,
            Board = Board,
            OpPostNum = OpPostNum
        };

        /// <summary>
        /// Пост находится в данном треде.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Результат проверки.</returns>
        public bool IsPostFromThisThread(BoardLinkBase link)
        {
            if (link is PostLink l)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(Engine, l.Engine) && StringComparer.OrdinalIgnoreCase.Equals(Board, l.Board) && OpPostNum == l.OpPostNum;
            }
            return false;
        }

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
                    return $"{Engine}://{Board}/{OpPostNum}";
                case LinkDisplayStringContext.Engine:
                    return $"/{Board}/{OpPostNum}";
                default:
                    return $"{OpPostNum}";
            }
        }

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"thread-{Engine}-{Board}-{OpPostNum}";

        /// <summary>
        /// Получить ссылку на тред.
        /// </summary>
        /// <returns>Ссылка на тред.</returns>
        public BoardLinkBase GetThreadLink() => GetType() == typeof(ThreadLink)
            ? this
            : new ThreadLink()
            {
                Engine = Engine,
                Board = Board,
                OpPostNum = OpPostNum
            };

        /// <summary>
        /// Получить ссылку на часть треда.
        /// </summary>
        /// <param name="fromPost">Начиная с номера поста.</param>
        /// <returns>Ссылка на часть треда.</returns>
        public BoardLinkBase GetThreadPart(int fromPost) => new ThreadPartLink()
        {
            Engine = Engine,
            Board = Board,
            OpPostNum = OpPostNum,
            FromPost = fromPost
        };

        /// <summary>
        /// Получить ссылку на пост.
        /// </summary>
        /// <param name="postNumber">Номер поста.</param>
        /// <returns>Ссылка на пост в треде.</returns>
        public BoardLinkBase GetPostLink(int postNumber) => new PostLink()
        {
            Engine = Engine,
            Board = Board,
            OpPostNum = OpPostNum,
            PostNum = postNumber
        };
    }
}