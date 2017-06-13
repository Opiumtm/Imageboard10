namespace Imageboard10.Core.NetworkInterface.Html
{
    /// <summary>
    /// Комментарий HTML.
    /// </summary>
    public interface IHtmlCommentNode : IHtmlNode
    {
        /// <summary>
        /// Текст.
        /// </summary>
        string Comment { get; }
    }
}