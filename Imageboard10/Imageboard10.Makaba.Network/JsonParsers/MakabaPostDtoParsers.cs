using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Imageboard10.Core;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.Models.Posts.PostMedia;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Core.Network.Html;
using Imageboard10.Core.NetworkInterface;
using Imageboard10.Core.NetworkInterface.Html;
using Imageboard10.Core.Utility;
using Imageboard10.Makaba.Network.Json;

namespace Imageboard10.Makaba.Network.JsonParsers
{
    /// <summary>
    /// Парсер данных поста.
    /// </summary>
    public class MakabaPostDtoParsers : NetworkDtoParsersBase, 
        INetworkDtoParser<BoardPost2WithParentLink, IBoardPost>
    {
        private IHtmlParser _htmlParser;
        private IHtmlDocumentFactory _htmlDocumentFactory;

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            _htmlParser = await moduleProvider.QueryEngineCapabilityAsync<IHtmlParser>(MakabaConstants.MakabaEngineId) ?? throw new ModuleNotFoundException(typeof(IHtmlParser));
            _htmlDocumentFactory = await moduleProvider.QueryModuleAsync<IHtmlDocumentFactory>() ?? throw new ModuleNotFoundException(typeof(IHtmlDocumentFactory));
            return Nothing.Value;
        }

        /// <summary>
        /// Получить поддерживаемые типы парсеров Dto.
        /// </summary>
        /// <returns>Поддерживаемые типы парсеров Dto.</returns>
        protected override IEnumerable<Type> GetDtoParsersTypes()
        {
            yield return typeof(INetworkDtoParser<BoardPost2WithParentLink, IBoardPost>);
        }

        private const string IpIdRegexText = @"(?:.*)\s+ID:\s+<span\s+class=""postertripid"">(?<id>.*)</span>.*$";

        private const string IpIdRegexText2 = @"(?:.*)\s+ID:\s+<span\s+id=""[^""]*""\s+style=""(?<style>[^""]*)"">(?<id>.*)</span>.*$";

        private const string ColorRegexText = @"color:rgb\((?<r>\d+),(?<g>\d+),(?<b>\d+)\)\;$";

        /// <summary>
        /// Распарсить.
        /// </summary>
        /// <param name="source">Источник.</param>
        /// <returns>Результат.</returns>
        public IBoardPost Parse(BoardPost2WithParentLink source)
        {
            var data = source.Post;
            var link = source.ParentLink;
            var isPreview = source.IsPreview;

            var ipIdRegex = RegexCache.CreateRegex(IpIdRegexText);
            var ipIdRegex2 = RegexCache.CreateRegex(IpIdRegexText2);
            var colorRegex = RegexCache.CreateRegex(ColorRegexText);

            var flags = new HashSet<Guid>();

            if (data.Banned != "0" && !string.IsNullOrWhiteSpace(data.Banned))
            {
                flags.Add(BoardPostFlags.Banned);
            }
            if (data.Closed != "0" && !string.IsNullOrWhiteSpace(data.Closed))
            {
                flags.Add(BoardPostFlags.Closed);
            }
            if (data.Sticky != "0" && !string.IsNullOrWhiteSpace(data.Sticky))
            {
                flags.Add(BoardPostFlags.Sticky);
            }
            if (isPreview)
            {
                flags.Add(BoardPostFlags.ThreadPreview);
            }
            if (source.Counter == 1)
            {
                flags.Add(BoardPostFlags.ThreadOpPost);
            }
            if (data.Op != "0" && !string.IsNullOrWhiteSpace(data.Op))
            {
                flags.Add(BoardPostFlags.Op);
            }
            if ("mailto:sage".Equals((data.Email ?? "").Trim(), StringComparison.OrdinalIgnoreCase))
            {
                flags.Add(BoardPostFlags.Sage);
            }
            if (data.Edited != "0" && !string.IsNullOrWhiteSpace(data.Edited))
            {
                flags.Add(BoardPostFlags.IsEdited);
            }
            if ((data.Endless ?? 0) != 0)
            {
                flags.Add(BoardPostFlags.Endless);
            }
            string admName = null;
            if (data.Tripcode != null)
            {
                if (data.Tripcode.StartsWith("!!%") && data.Tripcode.EndsWith("%!!"))
                {
                    if ("!!%mod%!!".Equals(data.Tripcode))
                    {
                        admName = "## Mod ##";
                    }
                    else if ("!!%adm%!!".Equals(data.Tripcode))
                    {
                        admName = "## Abu ##";
                    }
                    else if ("!!%Inquisitor%!!".Equals(data.Tripcode))
                    {
                        admName = "## Applejack ##";
                    }
                    else if ("!!%coder%!!".Equals(data.Tripcode))
                    {
                        admName = "## Кодер ##";
                    }
                    else admName = data.Tripcode.Replace("!!%", "## ").Replace("%!!", " ##");
                    flags.Add(BoardPostFlags.AdminTrip);
                }
            }
            var number = data.Number.TryParseWithDefault();
            var thisLink = new PostLink()
            {
                Engine = MakabaConstants.MakabaEngineId,
                Board = link.Board,
                OpPostNum = link.OpPostNum,
                PostNum = number
            };
            var postDocument = _htmlParser.ParseHtml(data.Comment ?? "", thisLink);
            var name = admName != null && string.IsNullOrWhiteSpace(data.Name) ? admName : WebUtility.HtmlDecode(data.Name ?? string.Empty).Replace("&nbsp;", " ");
            string nameColor = null;
            Color? color = null;
            var match = ipIdRegex.Match(name);
            var match2 = ipIdRegex2.Match(name);
            if (match.Success)
            {
                name = match.Groups["id"].Captures[0].Value;
            } else if (match2.Success)
            {
                name = match2.Groups["id"].Captures[0].Value;
                nameColor = match2.Groups["style"].Captures[0].Value;
                var cmatch = colorRegex.Match(nameColor);
                if (cmatch.Success)
                {
                    try
                    {
                        var r = byte.Parse(cmatch.Groups["r"].Captures[0].Value, CultureInfo.InvariantCulture.NumberFormat);
                        var g = byte.Parse(cmatch.Groups["g"].Captures[0].Value, CultureInfo.InvariantCulture.NumberFormat);
                        var b = byte.Parse(cmatch.Groups["b"].Captures[0].Value, CultureInfo.InvariantCulture.NumberFormat);
                        color = Color.FromArgb(255, r, g, b);
                    }
                    catch (Exception)
                    {
                        color = null;
                    }
                }
            }
            else if (name.StartsWith("Аноним ID:", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Remove(0, "Аноним ID:".Length).Trim();
            }
            PosterInfo posterInfo = null;
            if (!string.IsNullOrEmpty(name) || !string.IsNullOrWhiteSpace(data.Tripcode))
            {
                posterInfo = new PosterInfo()
                {
                    Name = HtmlToText(name ?? ""),
                    NameColor = color,
                    NameColorStr = nameColor,
                    Tripcode = data.Tripcode
                };
            }
            var iconAndFlag = ParseFlagAndIcon(data.Icon);
            BoardPostTags tags = null;
            if (!string.IsNullOrWhiteSpace(data.Tags))
            {
                tags = new BoardPostTags()
                {
                    TagStr = data.Tags,
                    Tags = new List<string>() { data.Tags }
                };
            }
            BoardPostLikes likes = null;
            if (data.Likes != null || data.Dislikes != null)
            {
                likes = new BoardPostLikes()
                {
                    Likes = data.Likes ?? 0,
                    Dislikes = data.Dislikes ?? 0
                };
            }
            var result = new Core.Models.Posts.BoardPost()
            {
                Link = thisLink,
                Comment = postDocument,
                ParentLink = link,
                Subject = WebUtility.HtmlDecode(data.Subject ?? string.Empty),
                BoardSpecificDate = data.Date,
                Date = DatesHelper.FromUnixTime(data.Timestamp.TryParseWithDefault()),
                Flags = flags.ToList(),
                Quotes = new List<ILink>(),
                Hash = data.Md5,
                Email = data.Email,
                MediaFiles = new List<IPostMedia>(),
                Counter = source.Counter,
                Poster = posterInfo,
                Icon = iconAndFlag.Icon,
                Country = iconAndFlag.Country,
                Tags = tags,
                UniqueId = Guid.NewGuid().ToString(),
                Likes = likes
            };
            if (data.Files != null)
            {
                foreach (var f in data.Files)
                {
                    BoardLinkBase mediaLink, tnLink;
                    if (IsBoardLink(f.Path, link.Board))
                    {
                        mediaLink = new BoardMediaLink()
                        {
                            Engine = MakabaConstants.MakabaEngineId,
                            Board = link.Board,
                            Uri = RemoveBoardFromLink(f.Path),                            
                        };
                        tnLink = new BoardMediaLink()
                        {
                            Engine = MakabaConstants.MakabaEngineId,
                            Board = link.Board,
                            Uri = RemoveBoardFromLink(f.Thumbnail),
                        };
                    }
                    else
                    {
                        mediaLink = new EngineMediaLink()
                        {
                            Engine = MakabaConstants.MakabaEngineId,
                            Uri = f.Path,
                        };
                        tnLink = new EngineMediaLink()
                        {
                            Engine = MakabaConstants.MakabaEngineId,
                            Uri = f.Thumbnail,
                        };
                    }
                    var media = new PostMediaWithThumbnail()
                    {
                        MediaLink = mediaLink,
                        FileSize = (ulong)(f.Size * 1024),
                        Height = f.Heigth,
                        Width = f.Width,
                        Name = f.Name,
                        MediaType = f.Type == MakabaMediaTypes.Webm ? PostMediaTypes.WebmVideo : PostMediaTypes.Image,
                        DisplayName = f.DisplayName,
                        FullName = f.FullName,
                        Nsfw = f.NotSafeForWork != 0,
                        Hash = f.Md5,
                        Duration = f.Duration,
                        Thumbnail = new PostMediaWithSize()
                        {                            
                            MediaLink = tnLink,
                            Height = f.TnHeight,
                            Width = f.TnWidth,
                            FileSize = null,
                            MediaType = PostMediaTypes.Image
                        },
                    };
                    result.MediaFiles.Add(media);
                }
            }
            if (source.Counter == 1 && string.IsNullOrWhiteSpace(result.Subject))
            {
                try
                {
                    var lines = result.Comment.ToPlainText();
                    if (lines.Count > 0)
                    {
                        var s = lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
                        if (s != null)
                        {
                            if (s.Length >= 50)
                            {
                                s = s.Substring(0, 50 - 3) + "...";
                            }
                            result.Subject = s;
                        }
                    }
                }
                catch
                {
                }
            }
            return result;
        }

        /// <summary>
        /// Парсить иконку и флаг.
        /// </summary>
        /// <param name="str">Строка.</param>
        /// <returns>Иконка и флаг.</returns>
        public FlagAndIcon ParseFlagAndIcon(string str)
        {
            var emptyResult = new FlagAndIcon() { Icon = null, Country = null };
            if (string.IsNullOrWhiteSpace(str))
            {
                return emptyResult;
            }
            try
            {
                var html = _htmlDocumentFactory.Load(str);
                if (html.DocumentNode?.ChildNodes == null)
                {
                    return emptyResult;
                }
                var images = html.DocumentNode
                    .ChildNodes
                    .Where(n => n.NodeType == typeof(IHtmlNode))
                    .Where(n => n.Name.EqualsNc("img"))
                    .ToArray();
                BoardPostIcon icon = null;
                BoardPostCountryFlag country = null;

                icon = images
                    .Where(obj => obj.GetAttributeValue("src", null) != null && obj.GetAttributeValue("title", null) != null)
                    .Select(obj => new BoardPostIcon()
                    {
                        ImageLink = new EngineMediaLink() { Engine = MakabaConstants.MakabaEngineId, Uri = obj.GetAttributeValue("src", null) },
                        Description = obj.GetAttributeValue("title", null)
                    })
                    .FirstOrDefault();

                country = images
                    .Where(obj => obj.GetAttributeValue("src", null) != null)
                    .Where(obj => (obj.GetAttributeValue("src", null) ?? "").StartsWith("/flags/", StringComparison.OrdinalIgnoreCase))
                    .Select(obj => new BoardPostCountryFlag()
                    {
                        ImageLink = new EngineMediaLink() {Engine = MakabaConstants.MakabaEngineId, Uri = obj.GetAttributeValue("src", null)},
                    })
                    .FirstOrDefault();

                return new FlagAndIcon() { Icon = icon, Country = country };
            }
            catch
            {
                return emptyResult;
            }
        }

        /// <summary>
        /// Флаг и иконка.
        /// </summary>
        public struct FlagAndIcon
        {
            /// <summary>
            /// Иконка.
            /// </summary>
            public BoardPostIcon Icon;

            /// <summary>
            /// Флаг.
            /// </summary>
            public BoardPostCountryFlag Country;
        }

        private string HtmlToText(string html)
        {
            return HtmlUtil.ConvertHtmlToText(ModuleProvider, html);
        }

        private bool IsBoardLink(string uri, string board)
        {
            if (uri == null)
            {
                return false;
            }
            return uri.StartsWith($"/{board}/", StringComparison.OrdinalIgnoreCase) ||
                   uri.StartsWith($"{board}/", StringComparison.OrdinalIgnoreCase);
        }

        private string RemoveBoardFromLink(string uri)
        {
            var parts = uri?.Split('/');
            if (parts == null)
            {
                return null;
            }
            parts = parts.Where(p => p != "").ToArray();
            return parts.Skip(1).Aggregate(new StringBuilder(), (sb, s) => sb.Append("/").Append(s)).ToString();
        }
    }
}