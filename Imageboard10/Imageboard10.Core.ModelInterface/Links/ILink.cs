using System;

namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Ссылка.
    /// </summary>
    public interface ILink
    {
        /// <summary>
        /// Тип ссылки.
        /// </summary>
        BoardLinkKind LinkKind { get; }

        /// <summary>
        /// Получить значения для сравнения.
        /// </summary>
        /// <returns>Значения для сравнения.</returns>
        LinkCompareValues GetCompareValues();

        /// <summary>
        /// Получить строку для отображения.
        /// </summary>
        /// <param name="context">Контекст изображения.</param>
        /// <returns>Строка для отображения.</returns>
        string GetDisplayString(LinkDisplayStringContext context);

        /// <summary>
        /// Получить идентификатор, "дружественный" файловой системе.
        /// </summary>
        /// <returns>Идентификатор.</returns>
        string GetFilesystemFriendlyId();

        /// <summary>
        /// Получить хэш ссылки для сравнения.
        /// </summary>
        /// <returns>Хэш ссылки.</returns>
        string GetLinkHash();

        /// <summary>
        /// Получить тип для сериализатора.
        /// </summary>
        /// <returns>Тип для сериализатора.</returns>
        Type GetTypeForSerializer();
    }
}