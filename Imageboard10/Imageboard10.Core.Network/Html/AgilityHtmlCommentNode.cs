using HtmlAgilityPack;
using Imageboard10.Core.NetworkInterface.Html;

namespace Imageboard10.Core.Network.Html
{
    /// <summary>
    /// Нода HTML.
    /// </summary>
    /// <typeparam name="T">Тип ноды.</typeparam>
    public class AgilityHtmlCommentNode<T> : AgilityHtmlNode<T>, IHtmlCommentNode
        where T : HtmlCommentNode
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="node">Исходный объект.</param>
        public AgilityHtmlCommentNode(T node) : base(node)
        {
        }

        /// <summary>
        /// Текст.
        /// </summary>
        public string Comment => Node.Comment;
    }
}