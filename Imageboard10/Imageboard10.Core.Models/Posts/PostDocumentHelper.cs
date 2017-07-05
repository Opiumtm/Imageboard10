using System.Collections.Generic;
using System.Linq;
using System.Text;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Utility;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Класс-помощник для документов.
    /// </summary>
    public static class PostDocumentHelper
    {
        /// <summary>
        /// Перевести в простой текст.
        /// </summary>
        /// <param name="tree">Дерево поста.</param>
        /// <returns>Текст.</returns>
        public static IList<string> ToPlainText(this IPostDocument tree)
        {
            var context = new { sb = new StringBuilder(), result = new List<string>() };
            var rules = tree.Nodes.TreeWalk(context)
                .GetChildren(n => (n as ICompositePostNode)?.Children)
                .If(n => n is ITextPostNode, (n, ctx) =>
                {
                    ctx.sb.Append(((ITextPostNode)n).Text);
                    return ctx;
                })
                .If(n => IsAttribute(n, PostBasicAttributes.Paragraph) || n is ILineBreakPostNode, (n, ctx) =>
                {
                    ctx.result.Add(ctx.sb.ToString());
                    ctx.sb.Clear();
                    return ctx;
                })
                .If(n => n is IBoardLinkPostNode, (n, ctx) =>
                {
                    var l = (IBoardLinkPostNode)n;
                    var pl = l.BoardLink as IPostLink;
                    var tl = l.BoardLink as IThreadLink;
                    if (pl != null)
                    {
                        ctx.sb.Append(">>" + pl.GetPostNumberString());
                    }
                    else if (tl != null)
                    {
                        ctx.sb.Append(">>" + tl.GetThreadNumberString());
                    }
                    return ctx;
                })
                .Else((n, c) => c);
            rules.Run();
            if (context.sb.Length > 0)
            {
                context.result.Add(context.sb.ToString());
            }
            return context.result;
        }

        private static bool IsAttribute(IPostNode node, string attribute)
        {
            var n = node as ICompositePostNode;
            var a = n?.Attribute as IPostBasicAttribute;
            if (a == null)
            {
                return false;
            }
            return a.Attribute == attribute;
        }

        /// <summary>
        /// Получить цитируемые посты.
        /// </summary>
        /// <param name="document">Документ.</param>
        /// <returns>Цитируемые посты.</returns>
        public static IList<ILink> GetQuotes(this IPostDocument document)
        {
            if (document?.Nodes == null)
            {
                return new List<ILink>();
            }
            return document.Nodes.FlatHierarchy(n =>
            {
                if (n is ICompositePostNode cn)
                {
                    return cn.Children;
                }
                return null;
            }).OfType<IBoardLinkPostNode>().Select(l => l?.BoardLink).Where(l => l != null).ToList();
        }
    }
}