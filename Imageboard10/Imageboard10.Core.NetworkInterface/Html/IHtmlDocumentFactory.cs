using Windows.Foundation.Metadata;
using Windows.Storage.Streams;
using Imageboard10.ModuleInterface;

namespace Imageboard10.Core.NetworkInterface.Html
{
    /// <summary>
    /// Фабрика документов HTML.
    /// </summary>
    public interface IHtmlDocumentFactory : IModule
    {
        /// <summary>
        /// Загрузить документ.
        /// </summary>
        /// <param name="content">Содержимое.</param>
        /// <returns>Документ.</returns>
        [DefaultOverload]
        IHtmlDocument Load(string content);

        /// <summary>
        /// Загрузить документ.
        /// </summary>
        /// <param name="stream">Поток.</param>
        /// <returns>Документ.</returns>
        IHtmlDocument Load(IInputStream stream);

        /// <summary>
        /// Конвертировать все html entity в unicode.
        /// </summary>
        /// <param name="src">Исходная строка.</param>
        /// <returns>Результат.</returns>
        string DeEntitize(string src);

        /// <summary>
        /// Проверка на пересечение элементов.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <returns>Результат.</returns>
        bool IsOverlappedClosingElement(string text);
    }
}