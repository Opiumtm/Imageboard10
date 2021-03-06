﻿using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка на пост.
    /// </summary>
    public class PostLink : ThreadLink, IPostLink
    {
        /// <summary>
        /// Номер поста.
        /// </summary>
        public int PostNum { get; set; }

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new PostLink()
        {
            Engine = Engine,
            Board = Board,
            OpPostNum = OpPostNum,
            PostNum = PostNum
        };

        /// <summary>
        /// Получить значения для сравнения.
        /// </summary>
        /// <returns>Значения для сравнения.</returns>
        public override LinkCompareValues GetCompareValues() => new LinkCompareValues()
        {
            Engine = Engine,
            Board = Board,
            Page = 0,
            Post = PostNum,
            Thread = OpPostNum,
            Other = ""
        };

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"post-{Engine}-{Board}-{OpPostNum}-{PostNum}";

        /// <summary>
        /// Получить строку с номером поста.
        /// </summary>
        /// <returns>Строка с номером поста.</returns>
        public string GetPostNumberString() => PostNum.ToString();

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.Post;

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
                    return $"{Engine}://{Board}/{OpPostNum}#{PostNum}";
                case LinkDisplayStringContext.Engine:
                    return $"/{Board}/{OpPostNum}#{PostNum}";
                case LinkDisplayStringContext.Board:
                    return $"{OpPostNum}#{PostNum}";
                default:
                    return $"{PostNum}";
            }
        }
    }
}