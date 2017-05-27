namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Базовые атрибуты.
    /// </summary>
    public static class PostBasicAttributes
    {
        /// <summary>
        /// Наклонный текст.
        /// </summary>
        public static string Italic { get; } = "i";

        /// <summary>
        /// Полужирный.
        /// </summary>
        public static string Bold { get; } = "b";

        /// <summary>
        /// Одинаковая ширина букв.
        /// </summary>
        public static string Monospace { get; } = "pre";

        /// <summary>
        /// Подчёркнутый текст.
        /// </summary>
        public static string Underscore { get; } = "u";

        /// <summary>
        /// Текст с чертой сверху.
        /// </summary>
        public static string Overscore { get; } = "o";

        /// <summary>
        /// Спойлер.
        /// </summary>
        public static string Spoiler { get; } = "spoiler";

        /// <summary>
        /// Зачёркнутый текст.
        /// </summary>
        public static string Strikeout { get; } = "s";

        /// <summary>
        /// Верхний индекс.
        /// </summary>
        public static string Sup { get; } = "sup";

        /// <summary>
        /// Нижний индекс.
        /// </summary>
        public static string Sub { get; } = "sub";

        /// <summary>
        /// Квота.
        /// </summary>
        public static string Quote { get; } = "quote";

        /// <summary>
        /// Сообщение от модератора.
        /// </summary>
        public static string Moderatorial { get; } = "mod";

        /// <summary>
        /// Элемент ненумерованного списка.
        /// </summary>
        public static string UnorderedList { get; } = "bullet";

        /// <summary>
        /// Параграф.
        /// </summary>
        public static string Paragraph { get; } = "par";
    }
}