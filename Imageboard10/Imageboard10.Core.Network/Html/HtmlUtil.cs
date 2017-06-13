using System.IO;
using System.Linq;
using HtmlAgilityPack;
using Imageboard10.Core.Modules;
using Imageboard10.Core.NetworkInterface.Html;

namespace Imageboard10.Core.Network.Html
{
    /// <summary>
    /// Утилиты HTML.
    /// </summary>
    public static class HtmlUtil
    {
        /// <summary>
        /// Найти первый не текстовый дочерний элемент.
        /// </summary>
        /// <param name="node">Элемент.</param>
        /// <returns>Первый не текстовый дочерний элемент.</returns>
        public static IHtmlNode FirstNonTextChild(this IHtmlNode node)
        {
            if (node == null)
            {
                return null;
            }
            if (!node.HasChildNodes)
            {
                return null;
            }
            return node.ChildNodes.FirstOrDefault(c => c.NodeType == typeof(IHtmlNode));
        }

        /// <summary>
        /// Конвертировать HTML в текст.
        /// </summary>
        /// <param name="provider">Провайдер модулей.</param>
        /// <param name="html">Текст html.</param>
        /// <returns></returns>
        public static string ConvertHtmlToText(IModuleProvider provider, string html)
        {
            var d = provider.QueryModule<IHtmlDocumentFactory, object>(null) ?? throw new ModuleNotFoundException(typeof(IHtmlDocumentFactory));

            IHtmlDocument doc = d.Load(html);

            StringWriter sw = new StringWriter();
            ConvertHtmlToText(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        /// <summary>
        /// Конвертировать HTML в текст.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="outText"></param>
        public static void ConvertHtmlToText(IHtmlNode node, TextWriter outText)
        {
            switch (node)
            {
                case IHtmlCommentNode _:
                    // don't output comments
                    break;
                case IHtmlTextNode tn:
                    // script and style must not be output
                    string parentName = tn.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    var html = tn.Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                default:
                    switch (node.Name)
                    {
                        case "p":
                            // treat paragraphs as crlf
                            outText.WriteLine();
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }
                    break;
            }
        }

        private static void ConvertContentTo(IHtmlNode node, TextWriter outText)
        {
            foreach (var subnode in node.ChildNodes)
            {
                ConvertHtmlToText(subnode, outText);
            }
        }
    }
}