using System;
using System.Collections.Generic;

namespace Imageboard10.Makaba
{
    /// <summary>
    /// Константы движка makaba.
    /// </summary>
    public static class MakabaConstants
    {
        /// <summary>
        /// Идентификатор движка.
        /// </summary>
        public const string MakabaEngineId = "makaba";

        /// <summary>
        /// Имя ресурса.
        /// </summary>
        public const string MakabaResourceName = "Два.ч";

        /// <summary>
        /// Взрослые доски.
        /// </summary>
        public static readonly HashSet<string> AdultBoards = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "b", "fag", "fg", "fur", "g", "ga", "h", "ho", "sex", "fet", "e", "hc", "gb", "ya", "r34", "hm", "guro",
            "vn", "hg", "es"
        };

        /// <summary>
        /// Доски, на которых нет заголовка.
        /// </summary>
        public static readonly HashSet<string> BoardsWithoutTitle = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "b"
        };

        /// <summary>
        /// Доски, на которых нет загрузки медиа-файлов.
        /// </summary>
        public static readonly HashSet<string> BoardsWithoutMedia = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "d"
        };

        /// <summary>
        /// Типы разметки.
        /// </summary>
        public static class MarkupTypes
        {
            /// <summary>
            /// Макаба-разметка.
            /// </summary>
            public static readonly Guid Makaba = new Guid("{E5614E75-78E4-4624-8A2F-902AD3D14810}");
        }

        /// <summary>
        /// Типы капчи.
        /// </summary>
        public static class CaptchaTypes
        {
            /// <summary>
            /// двач.капча
            /// </summary>
            public static readonly Guid DvachCaptcha = new Guid("{A8B337C6-8568-4DA8-B5EB-4BD4B072CC2A}");

            /// <summary>
            /// Без капчи.
            /// </summary>
            public static readonly Guid NoCaptcha = new Guid("{5E382F6C-3A76-4D1A-B757-C52B26ED3265}");
        }
    }
}