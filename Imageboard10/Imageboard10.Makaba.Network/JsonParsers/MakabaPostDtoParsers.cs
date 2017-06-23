using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.UI;
using Imageboard10.Core;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Boards;
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
using Imageboard10.Makaba.Models.Posts;
using Imageboard10.Makaba.Network.Json;

namespace Imageboard10.Makaba.Network.JsonParsers
{
    /// <summary>
    /// Парсер данных поста.
    /// </summary>
    public class MakabaPostDtoParsers : NetworkDtoParsersBase, 
        INetworkDtoParser<BoardPost2WithParentLink, IBoardPost>,
        INetworkDtoParser<PartialThreadData, IBoardPostCollectionEtag>,
        INetworkDtoParser<ThreadData, IBoardPostCollectionEtag>
    {
        private IHtmlParser _htmlParser;
        private IHtmlDocumentFactory _htmlDocumentFactory;
        private INetworkDtoParser<BoardPost2WithParentLink, IBoardPost> _postsParser;

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            _htmlParser = await moduleProvider.QueryEngineCapabilityAsync<IHtmlParser>(MakabaConstants.MakabaEngineId) ?? throw new ModuleNotFoundException(typeof(IHtmlParser));
            _htmlDocumentFactory = await moduleProvider.QueryModuleAsync<IHtmlDocumentFactory>() ?? throw new ModuleNotFoundException(typeof(IHtmlDocumentFactory));
            _postsParser = await moduleProvider.FindNetworkDtoParserAsync<BoardPost2WithParentLink, IBoardPost>() ?? throw new ModuleNotFoundException(typeof(INetworkDtoParser<BoardPost2WithParentLink, IBoardPost>));
            return Nothing.Value;
        }

        /// <summary>
        /// Получить поддерживаемые типы парсеров Dto.
        /// </summary>
        /// <returns>Поддерживаемые типы парсеров Dto.</returns>
        protected override IEnumerable<Type> GetDtoParsersTypes()
        {
            yield return typeof(INetworkDtoParser<BoardPost2WithParentLink, IBoardPost>);
            yield return typeof(INetworkDtoParser<PartialThreadData, IBoardPostCollectionEtag>);
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
                Likes = likes,
                LoadedTime = source.LoadedTime
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

        /// <summary>
        /// Распарсить.
        /// </summary>
        /// <param name="source">Источник.</param>
        /// <returns>Результат.</returns>
        public IBoardPostCollectionEtag Parse(PartialThreadData source)
        {
            var posts = source.Posts.OrderBy(p => p.Number.TryParseWithDefault());
            var result = new BoardPostCollection()
            {
                Etag = source.Etag,
                Info = null,
                Link = source.Link,
                ParentLink = source.Link?.GetBoardLink(),
                Posts = posts.WithCounter(1).Select(p => _postsParser.Parse(new BoardPost2WithParentLink()
                {
                    Counter = p.Key,
                    ParentLink = new ThreadLink() { Board = source.Link.Board, Engine = source.Link.Engine, OpPostNum = source.Link.OpPostNum },
                    Post = p.Value,
                    IsPreview = false,
                    LoadedTime = source.LoadedTime,                    
                })).ToList()
            };
            return result;
        }

        /// <summary>
        /// Распарсить.
        /// </summary>
        /// <param name="source">Источник.</param>
        /// <returns>Результат.</returns>
        public IBoardPostCollectionEtag Parse(ThreadData source)
        {
            var entity = GetEntityModel(source.Entity, source.Link);
            var posts = source.Entity.Threads.SelectMany(p => p.Posts).OrderBy(p => p.Number.TryParseWithDefault());
            var parsedPosts = posts.WithCounter(1).Select(c => _postsParser.Parse(new BoardPost2WithParentLink()
            {
                ParentLink = source.Link,
                Counter = c.Key,
                Post = c.Value,
                IsPreview = false,
                LoadedTime = source.LoadedTime
            })).ToArray();
            return new BoardPostCollection()
            {
                Link = source.Link,
                ParentLink = source.Link.GetBoardLink(),
                Etag = source.Etag,
                Info = entity,
                Posts = parsedPosts
            };
        }

        /// <summary>
        /// Получить информацию о makaba entity.
        /// </summary>
        /// <param name="entity2">Makaba entity.</param>
        /// <param name="baseLink">Базовая ссылка.</param>
        /// <returns>Информация.</returns>
        private MakabaEntityInfoModel GetEntityModel(BoardEntity2 entity2, ILink baseLink)
        {
            var flags = new HashSet<Guid>();

            void AddFlag(int? flag, Guid flagId)
            {
                if (flag != null && flag != 0)
                {
                    flags.Add(flagId);
                }
            }

            AddFlag(entity2.EnableAudio, PostCollectionFlags.EnableAudio);
            AddFlag(entity2.EnableDices, PostCollectionFlags.EnableDices);
            AddFlag(entity2.EnableFlags, PostCollectionFlags.EnableCountryFlags);
            AddFlag(entity2.EnableIcons, PostCollectionFlags.EnableIcons);
            AddFlag(entity2.EnableImages, PostCollectionFlags.EnableImages);
            AddFlag(entity2.EnableLikes, PostCollectionFlags.EnableLikes);
            AddFlag(entity2.EnableNames, PostCollectionFlags.EnableNames);
            AddFlag(entity2.EnableOekaki, PostCollectionFlags.EnableOekaki);
            AddFlag(entity2.EnablePosting, PostCollectionFlags.EnablePosting);
            AddFlag(entity2.EnableSage, PostCollectionFlags.EnableSage);
            AddFlag(entity2.EnableShield, PostCollectionFlags.EnableShield);
            AddFlag(entity2.EnableSubject, PostCollectionFlags.EnableSubject);
            AddFlag(entity2.EnableThreadTags, PostCollectionFlags.EnableThreadTags);
            AddFlag(entity2.EnableTrips, PostCollectionFlags.EnableTripcodes);
            AddFlag(entity2.EnableVideo, PostCollectionFlags.EnableVideo);
            AddFlag(entity2.IsIndex, PostCollectionFlags.IsIndex);
            AddFlag(entity2.IsBoard, PostCollectionFlags.IsBoard);

            return new MakabaEntityInfoModel()
            {
                AdvertisementBannerLink = !string.IsNullOrWhiteSpace(entity2.AdvertBottomImage) ? 
                    new EngineMediaLink() { Engine = MakabaConstants.MakabaEngineId, Uri = entity2.AdvertBottomImage} : null,
                AdvertisementClickLink = !string.IsNullOrWhiteSpace(entity2.AdvertBottomLink) ?
                    new EngineUriLink() { Engine = MakabaConstants.MakabaEngineId, Uri = entity2.AdvertBottomLink } : null,
                AdvertisementItems = entity2.TopAdvert?.Select(a => new MakabaBoardPostCollectionInfoBoardsAdvertisementItem()
                {
                    Name = a.Name,
                    BoardLink = new BoardLink() {  Engine = MakabaConstants.MakabaEngineId, Board = a.Board },
                    Info = a.Info
                })?.OfType<IBoardPostCollectionInfoBoardsAdvertisementItem>()?.ToList(),
                Board = entity2.Board,
                BannerImageLink = !string.IsNullOrWhiteSpace(entity2.BoardBannerImage) ?
                    new EngineMediaLink() { Engine = MakabaConstants.MakabaEngineId, Uri = entity2.BoardBannerImage } : null,
                BannerBoardLink = !string.IsNullOrWhiteSpace(entity2.BoardBannerLink) ?
                    new BoardLink() { Engine = MakabaConstants.MakabaEngineId, Board = entity2.BoardBannerLink} : null,
                BannerSize = new SizeInt32() {  Width = 300, Height = 100 },
                BoardInfo = entity2.BoardInfo != null ? _htmlParser.ParseHtml(entity2.BoardInfo, baseLink) : null,
                BoardInfoOuter = entity2.BoardInfoOuter,
                BoardName = entity2.BoardName,
                CurrentPage = entity2.CurrentPage,
                CurrentThread = entity2.CurrentThread,
                DefaultName = entity2.DefaultName,
                MaxComment = entity2.MaxComment,
                MaxFilesSize = entity2.MaxFilesSize != null ? new ulong?(((ulong)entity2.MaxFilesSize) * 1024) : null,
                Pages = entity2.Pages?.ToList(),
                Speed = entity2.BoardSpeed ?? 0,
                NewsItems = entity2.NewsAbu?.Select(n => new MakabaBoardPostCollectionInfoNewsItem()
                {
                    Date = n.Date,
                    Title = n.Subject,
                    NewsLink = new ThreadLink()
                    {
                        Engine = MakabaConstants.MakabaEngineId,
                        Board = "abu",
                        OpPostNum = n.Number
                    }
                })?.OfType<IBoardPostCollectionInfoNewsItem>()?.ToList(),
                Flags = flags.ToList(),
                Icons = entity2.Icons?.Select(i => new BoardIcon()
                {
                    Id = i.Number,
                    Name = i.Name,
                    MediaLink = i.Url != null ? new EngineMediaLink() { Engine = MakabaConstants.MakabaEngineId, Uri = i.Url } : null
                })?.OfType<IBoardIcon>()?.ToList()
            };
        }
    }
}