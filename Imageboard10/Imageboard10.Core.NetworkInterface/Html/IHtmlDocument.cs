namespace Imageboard10.Core.NetworkInterface.Html
{
    /// <summary>
    /// Документ HTML.
    /// </summary>
    public interface IHtmlDocument
    {
        /// <summary>
        /// Главная нода.
        /// </summary>
        IHtmlNode DocumentNode { get; }

        /// <summary>
        /// Исходный объект.
        /// </summary>
        object SourceObject { get; }
    }
}