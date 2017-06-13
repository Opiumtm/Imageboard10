using System;
using HtmlAgilityPack;
using Imageboard10.Core.NetworkInterface.Html;

namespace Imageboard10.Core.Network.Html
{
    /// <summary>
    /// HTML документ.
    /// </summary>
    public class AgilityHtmlDocument : IHtmlDocument
    {
        private readonly HtmlDocument _document;

        /// <summary>
        /// Создать обёртну над узлом.
        /// </summary>
        /// <param name="n">Исходный узел.</param>
        /// <param name="parent">Родительский узел.</param>
        /// <returns>Обёртка.</returns>
        internal static IHtmlNode CreateNode(HtmlNode n, IHtmlNode parent)
        {
            switch (n)
            {
                case HtmlCommentNode c:
                    return new AgilityHtmlCommentNode<HtmlCommentNode>(c)
                    {
                        ParentNode = parent
                    };
                case HtmlTextNode t:
                    return new AgilityHtmlTextNode<HtmlTextNode>(t)
                    {
                        ParentNode = parent
                    };
                default:
                    return new AgilityHtmlNode<HtmlNode>(n)
                    {
                        ParentNode = parent
                    };
            }
        }

        public AgilityHtmlDocument(HtmlDocument document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _documentNode = new Lazy<IHtmlNode>(() =>
            {
                if (_document.DocumentNode == null)
                {
                    return null;
                }
                return CreateNode(_document.DocumentNode, null);
            });
        }

        private readonly Lazy<IHtmlNode> _documentNode;

        /// <summary>
        /// Главная нода.
        /// </summary>
        public IHtmlNode DocumentNode => _documentNode.Value;

        /// <summary>
        /// Исходный объект.
        /// </summary>
        public object SourceObject => _document;
    }
}