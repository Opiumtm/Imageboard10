using System;

namespace Imageboard10.Core.ModelInterface.Posting
{
    /// <summary>
    /// Семантическая роль поля постинга.
    /// </summary>
    public static class PostingFieldSemanticRole
    {
        /// <summary>
        /// Заголовок.
        /// </summary>
        public static Guid Title { get; } = new Guid("{F472CBCD-DDA7-4186-AC27-FCEC44A6EE37}");

        /// <summary>
        /// Комментарий.
        /// </summary>
        public static Guid Comment { get; } = new Guid("{B3452B1A-16FE-4BA9-845C-045BDB780DC0}");

        /// <summary>
        /// Адрес почты.
        /// </summary>
        public static Guid Email { get; } = new Guid("{B12A5C5D-4944-4A66-B1C9-8215E4D3D726}");

        /// <summary>
        /// Имя постера.
        /// </summary>
        public static Guid PosterName { get; } = new Guid("{B6802CE5-057E-480A-8AB6-CC8F411179B3}");

        /// <summary>
        /// Трипкод (1 часть).
        /// </summary>
        public static Guid PosterTrip { get; } = new Guid("{CFF50120-7101-494D-B4BA-0FB4FD4835B0}");

        /// <summary>
        /// Трипкод (2 часть).
        /// </summary>
        public static Guid PosterTrip2 { get; } = new Guid("{6C824923-A515-4679-A5F0-DE8042466A46}");

        /// <summary>
        /// Иконка (для политача и подобных борд).
        /// </summary>
        public static Guid Icon { get; } = new Guid("{C5ECBB76-F538-4C99-8EA1-BB5A786BA272}");

        /// <summary>
        /// Сажа.
        /// </summary>
        public static Guid SageFlag { get; } = new Guid("{E8F05665-B626-4003-A83E-A6738B3B7857}");

        /// <summary>
        /// Ватермарка.
        /// </summary>
        public static Guid WatermarkFlag { get; } = new Guid("{25ACB1FE-1EB3-470A-B978-24F3A3BE2436}");

        /// <summary>
        /// Флаг ОП-постера.
        /// </summary>
        public static Guid OpFlag { get; } = new Guid("{A3F1AAAE-3F4A-48E6-B327-A0D96DAEF8B5}");

        /// <summary>
        /// Медиа файл.
        /// </summary>
        public static Guid MediaFile { get; } = new Guid("{3DEEB9A8-F4BE-4E9A-8DF0-011E97F99100}");

        /// <summary>
        /// Капча.
        /// </summary>
        public static Guid Captcha { get; } = new Guid("{73642511-8B89-4D21-8A79-1954B31ADC98}");

        /// <summary>
        /// Тэг треда.
        /// </summary>
        public static Guid ThreadTag { get; } = new Guid("{3AB22621-653F-422C-81D1-44511942087C}");
    }
}