using System;

namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Контекст капчи.
    /// </summary>
    public static class CaptchaLinkContext
    {
        /// <summary>
        /// Тред.
        /// </summary>
        public static Guid Thread { get; } = new Guid("{B597542C-C62C-4CFA-99F1-94B4AED7A3C0}");

        /// <summary>
        /// Новый тред.
        /// </summary>
        public static Guid NewThread { get; } = new Guid("{6517C5C4-9872-4443-B792-B723CA089C53}");
    }
}