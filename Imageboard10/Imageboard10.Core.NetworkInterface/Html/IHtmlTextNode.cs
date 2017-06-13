namespace Imageboard10.Core.NetworkInterface.Html
{
    /// <summary>
    /// Текстовая нода HTML.
    /// </summary>
    public interface IHtmlTextNode : IHtmlNode
    {
        /// <summary>
        /// Текст.
        /// </summary>
        string Text { get; }
    }
}