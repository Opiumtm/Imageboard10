using HtmlAgilityPack;
using Imageboard10.Core.NetworkInterface.Html;

namespace Imageboard10.Core.Network.Html
{
    /// <summary>
    /// Нода HTML.
    /// </summary>
    /// <typeparam name="T">Тип ноды.</typeparam>
    public class AgilityHtmlTextNode<T> : AgilityHtmlNode<T>, IHtmlTextNode
        where T : HtmlTextNode
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="node">Исходный объект.</param>
        public AgilityHtmlTextNode(T node) : base(node)
        {
        }

        /// <summary>
        /// Текст.
        /// </summary>
        public string Text => Node.Text;
    }
}