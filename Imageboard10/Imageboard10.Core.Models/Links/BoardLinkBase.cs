namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Базовый класс для ссылки на борде.
    /// </summary>
    public abstract class BoardLinkBase : IDeepCloneable<BoardLinkBase>
    {
        /// <summary>
        /// Движок.
        /// </summary>
        public string Engine { get; set; }

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public abstract BoardLinkKind LinkKind { get; }

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <returns>Клон.</returns>
        public abstract BoardLinkBase DeepClone();

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        public abstract string GetLinkHash();

        /// <summary>
        /// Получить значения для сравнения.
        /// </summary>
        /// <returns>Значения для сравнения.</returns>
        public abstract LinkCompareValues GetCompareValues();
    }
}