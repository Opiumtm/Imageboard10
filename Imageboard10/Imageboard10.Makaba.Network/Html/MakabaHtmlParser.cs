using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Imageboard10.Core;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.Models.Posts.PostNodes;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Core.Network.Html;
using Imageboard10.Core.NetworkInterface;
using Imageboard10.Core.NetworkInterface.Html;
using Imageboard10.Core.Utility;

namespace Imageboard10.Makaba.Network.Html
{

    /// <summary>
    /// Парсер HTML для движка Makaba.
    /// </summary>
    public sealed class MakabaHtmlParser : MakabaEngineModuleBase<IHtmlParser>, IHtmlParser
    {
        private IHtmlDocumentFactory _htmlDocumentFactory;

        private IYoutubeIdService _youtubeIdService;

        private IEngineLinkParser _linkParser;

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            _htmlDocumentFactory = await moduleProvider.QueryModuleAsync<IHtmlDocumentFactory>() ?? throw new ModuleNotFoundException(typeof(IHtmlDocumentFactory));
            _youtubeIdService = await moduleProvider.QueryModuleAsync<IYoutubeIdService>() ?? throw new ModuleNotFoundException(typeof(IYoutubeIdService));
            _linkParser = await moduleProvider.QueryEngineCapabilityAsync<IEngineLinkParser>(MakabaConstants.MakabaEngineId) ?? throw new ModuleNotFoundException(typeof(IEngineLinkParser));
            return Nothing.Value;
        }

        /// <summary>
        /// Получить документ.
        /// </summary>
        /// <param name="comment">HTML.</param>
        /// <param name="baseLink">Базовая ссылка.</param>
        /// <returns>Документ.</returns>
        public IPostDocument ParseHtml(string comment, ILink baseLink)
        {
            var html = _htmlDocumentFactory.Load(comment);
            IList<IPostNode> result = new List<IPostNode>();

            result = html.DocumentNode.ChildNodes.TreeWalk(new ParseContext() { Nodes = result, BaseLink = baseLink })
                .GetChildren(node => node.ChildNodes)
                .If(node => node.NodeType == typeof(IHtmlTextNode), (node, res) => AddToResult(res, new TextPostNode() { Text = WebUtility.HtmlDecode(node.InnerText) }), node => null)
                .If(IsPostLink, AddPostLink, node => null)
                .If(node => node.Name.EqualsNc("br"), (node, res) => AddToResult(res, new LineBreakPostNode()))
                .If(node => node.Name.EqualsNc("p"), (node, res) => CreateAttribute(res, PostBasicAttributes.Paragraph))
                .If(node => node.Name.EqualsNc("em"), (node, res) => CreateAttribute(res, PostBasicAttributes.Italic))
                .If(node => node.Name.EqualsNc("strong"), (node, res) => CreateAttribute(res, PostBasicAttributes.Bold))
                .If(node => GetPreformatNode(node) != null, (node, res) => CreateAttribute(res, PostBasicAttributes.Monospace), node => GetPreformatNode(node).ChildNodes)
                .If(node => CheckSpan(node, "u"), (node, res) => CreateAttribute(res, PostBasicAttributes.Underscore))
                .If(node => CheckSpan(node, "o"), (node, res) => CreateAttribute(res, PostBasicAttributes.Overscore))
                .If(node => CheckSpan(node, "spoiler"), (node, res) => CreateAttribute(res, PostBasicAttributes.Spoiler))
                .If(node => CheckSpan(node, "s"), (node, res) => CreateAttribute(res, PostBasicAttributes.Strikeout))
                .If(node => node.Name.EqualsNc("sub"), (node, res) => CreateAttribute(res, PostBasicAttributes.Sub))
                .If(node => node.Name.EqualsNc("sup"), (node, res) => CreateAttribute(res, PostBasicAttributes.Sup))
                .If(node => CheckSpan(node, "unkfunc"), (node, res) => CreateAttribute(res, PostBasicAttributes.Quote))
                .If(node => node.Name.EqualsNc("a") && !string.IsNullOrWhiteSpace(node.GetAttributeValue("href", null)), CreateLinkAttrNode)
                .Else((node, res) => res)
                .Run()
                .Nodes;

            return new PostDocument() { Nodes = result };
        }

        private ParseContext CreateLinkAttrNode(IHtmlNode node, ParseContext res)
        {
            var linkUri = GetLinkText(node.GetAttributeValue("href", null));
            var detectedLink = _linkParser.TryParseLink(linkUri, true);
            if (detectedLink != null)
            {
                return CreateNode(res, new PostLinkAttribute()
                {
                    Link = detectedLink
                });
            }
            var youtubeId = _youtubeIdService.GetYoutubeIdFromUri(linkUri);
            if (youtubeId != null)
            {
                return CreateNode(res, new PostLinkAttribute()
                {
                    Link = new YoutubeLink()
                    {
                        YoutubeId = youtubeId
                    }
                });
            }
            return CreateNode(res, new PostLinkAttribute()
            {                
                Link = new UriLink() {  Uri = linkUri }
            });
        }
        private string GetLinkText(string t)
        {
            return WebUtility.HtmlDecode(t);
        }

        private ParseContext CreateNode(ParseContext result, IPostAttribute attribute)
        {
            var r = new CompositePostNode()
            {
                Attribute = attribute,
                Children = new List<IPostNode>()
            };
            result.Nodes.Add(r);
            return new ParseContext()
            {
                Nodes = r.Children,
                BaseLink = result.BaseLink
            };
        }

        private ParseContext AddToResult(ParseContext result, IPostNode node)
        {
            result.Nodes.Add(node);
            return result;
        }

        private bool IsPostLink(IHtmlNode node)
        {
            if (!node.Name.EqualsNc("a"))
            {
                return false;
            }
            if (node.GetAttributeValue("href", null) == null)
            {
                return false;
            }
            if ("post-reply-link".Equals(node.GetAttributeValue("class", null), StringComparison.OrdinalIgnoreCase))
            {

            }
            else
            {
                if (node.GetAttributeValue("onclick", null) == null)
                {
                    return false;
                }
                if (!node.GetAttributeValue("onclick", "").StartsWith("highlight", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return _linkParser.IsLinkForEngine(node.GetAttributeValue("href", ""), true);
        }

        private ParseContext AddPostLink(IHtmlNode node, ParseContext result)
        {
            var href = node.GetAttributeValue("href", "");
            var link = _linkParser.TryParseLink(href, true);
            if (link != null)
            {
                result.Nodes.Add(new BoardLinkPostNode()
                {
                    BoardLink = link
                });
            }
            return result;
        }

        private IHtmlNode GetPreformatNode(IHtmlNode node)
        {
            return node
                .WalkTemplate(n => n.Name.EqualsNc("pre"), n => n.FirstNonTextChild())
                .WalkTemplate(n => n.Name.EqualsNc("code"), n => n);
        }

        private ParseContext CreateAttribute(ParseContext result, string attribute)
        {
            return CreateNode(result, new PostBasicAttribute() { Attribute = attribute });
        }

        private bool CheckSpan(IHtmlNode node, string className)
        {
            if (!node.Name.EqualsNc("span"))
            {
                return false;
            }
            return node.GetAttributeValue("class", null).EqualsNc(className);
        }

        private struct ParseContext
        {
            public IList<IPostNode> Nodes;
            public ILink BaseLink;
        }
    }
}