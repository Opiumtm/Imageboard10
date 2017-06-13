using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using Imageboard10.Core.NetworkInterface.Html;

namespace Imageboard10.Core.Network.Html
{
    /// <summary>
    /// Нода HTML.
    /// </summary>
    /// <typeparam name="T">Тип ноды.</typeparam>
    public class AgilityHtmlNode<T> : IHtmlNode
        where T : HtmlNode
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="node">Нода.</param>
        public AgilityHtmlNode(T node)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
            _childNodes = new Lazy<IList<IHtmlNode>>(() =>
            {
                var r = new List<IHtmlNode>();
                if (_node.HasChildNodes && _node.ChildNodes != null)
                {
                    foreach (var n in _node.ChildNodes)
                    {
                        if (n != null)
                        {
                            r.Add(AgilityHtmlDocument.CreateNode(n, this));
                        }
                    }
                }
                return r;
            });
            _nodeType = new Lazy<Type>(() =>
            {
                if (_node is HtmlCommentNode)
                {
                    return typeof(IHtmlCommentNode);
                }
                if (_node is HtmlTextNode)
                {
                    return typeof(IHtmlTextNode);
                }
                return typeof(IHtmlNode);
            });
        }

        private readonly T _node;

        /// <summary>
        /// Исходный объект.
        /// </summary>
        protected T Node => _node;

        public string Name => _node.Name;

        /// <summary>
        /// Получить значение атрибута.
        /// </summary>
        /// <param name="name">Имя.</param>
        /// <param name="def">Значение по умолчанию.</param>
        /// <returns>Значение.</returns>
        public string GetAttributeValue(string name, string def)
        {
            return _node.GetAttributeValue(name, def);
        }

        /// <summary>
        /// Получить значение атрибута.
        /// </summary>
        /// <param name="name">Имя.</param>
        /// <param name="def">Значение по умолчанию.</param>
        /// <returns>Значение.</returns>
        public int GetAttributeValue(string name, int def)
        {
            return _node.GetAttributeValue(name, def);
        }

        /// <summary>
        /// Получить значение атрибута.
        /// </summary>
        /// <param name="name">Имя.</param>
        /// <param name="def">Значение по умолчанию.</param>
        /// <returns>Значение.</returns>
        public bool GetAttributeValue(string name, bool def)
        {
            return _node.GetAttributeValue(name, def);
        }

        private readonly Lazy<IList<IHtmlNode>> _childNodes;
        private readonly Lazy<Type> _nodeType;

        /// <summary>
        /// Дочерние ноды.
        /// </summary>
        public IList<IHtmlNode> ChildNodes => _childNodes.Value;

        public bool HasChildNodes => _node.HasChildNodes;

        /// <summary>
        /// Тип ноды (один из интерфейсов, унаследованных от <see cref="IHtmlNode"/>).
        /// </summary>
        public virtual Type NodeType => _nodeType.Value;

        /// <summary>
        /// Исходный объект.
        /// </summary>
        public object SourceObject => _node;

        /// <summary>
        /// Родительская нода.
        /// </summary>
        public IHtmlNode ParentNode { get; internal set; }
    }
}