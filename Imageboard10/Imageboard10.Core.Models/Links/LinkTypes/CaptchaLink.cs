using System;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links.LinkTypes
{
    /// <summary>
    /// Ссылка на капчу.
    /// </summary>
    public class CaptchaLink : EngineLinkBase, ICaptchaLink
    {
        /// <summary>
        /// Тип капчи.
        /// </summary>
        public Guid CaptchaType { get; set; }

        /// <summary>
        /// Контекст капчи.
        /// </summary>
        public Guid CaptchaContext { get; set; }

        /// <summary>
        /// Идентификатор капчи.
        /// </summary>
        public string CaptchaId { get; set; }

        /// <summary>
        /// Доска или null.
        /// </summary>
        public string Board { get; set; }

        /// <summary>
        /// Идентификатор треда или 0, если не важно.
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public override BoardLinkKind LinkKind => BoardLinkKind.Captcha;

        /// <summary>
        /// Получить значения для сравнения.
        /// </summary>
        /// <returns>Значения для сравнения.</returns>
        public override LinkCompareValues GetCompareValues()
        {
            return new LinkCompareValues()
            {
                Engine = Engine,
                Board = Board ?? "",
                Other = $"{CaptchaType}|{CaptchaContext}|{CaptchaId ?? ""}",
                Page = 0,
                Post = 0,
                Thread = ThreadId
            };
        }

        /// <summary>
        /// Получить строку для отображения.
        /// </summary>
        /// <param name="context">Контекст изображения.</param>
        /// <returns>Строка для отображения.</returns>
        public override string GetDisplayString(LinkDisplayStringContext context) => $"captcha://{CaptchaType}?{CaptchaContext}#{CaptchaId ?? ""}";

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public override BoardLinkBase DeepClone() => new CaptchaLink()
        {
            Engine = Engine,
            CaptchaType = CaptchaType,
            CaptchaContext = CaptchaContext,
            CaptchaId = CaptchaId,
            Board = Board,
            ThreadId = ThreadId
        };

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public override string GetLinkHash() => $"captcha-{CaptchaType}-{CaptchaContext}-{Board ?? ""}-t{ThreadId}-{CaptchaId ?? ""}";
    }
}