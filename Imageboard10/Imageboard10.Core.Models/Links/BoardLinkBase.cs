using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Базовый класс для ссылки на борде.
    /// </summary>
    public abstract class BoardLinkBase : IDeepCloneable<BoardLinkBase>, ILink
    {
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

        /// <summary>
        /// Получить строку для отображения.
        /// </summary>
        /// <param name="context">Контекст изображения.</param>
        /// <returns>Строка для отображения.</returns>
        public abstract string GetDisplayString(LinkDisplayStringContext context);

        /// <summary>
        /// Получить идентификатор, "дружественный" файловой системе.
        /// </summary>
        /// <returns>Идентификатор.</returns>
        public virtual string GetFilesystemFriendlyId() => GetLinkHash();
    }
}