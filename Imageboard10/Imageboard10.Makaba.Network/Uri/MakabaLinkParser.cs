using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Imageboard10.Core;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Modules;
using Imageboard10.Core.NetworkInterface;
using Imageboard10.Core.Utility;

namespace Imageboard10.Makaba.Network.Uri
{
    /// <summary>
    /// Парсер ссылок makaba.
    /// </summary>
    public sealed class MakabaLinkParser : MakabaEngineModuleBase<IEngineLinkParser>, IEngineLinkParser
    {
        private const string PostLinkRegexText = @"http[s]?://(?:2ch\.(?:[^/]+)|2-ch.so)/(?<board>[^/]+)/res/(?<parent>\d+).html(?:#(?<post>\d+))?$";
        private const string PostLinkRegex2Text = @"/?(?<board>[^/]+)/res/(?<parent>\d+).html(?:#(?<post>\d+))?$";

        private Regex _postLinkRegex, _postLinkRegex2;

        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            _postLinkRegex = RegexCache.CreateRegex(PostLinkRegexText);
            _postLinkRegex2 = RegexCache.CreateRegex(PostLinkRegex2Text);
            return Nothing.Value;
        }

        /// <summary>
        /// Попробовать распарсить строку.
        /// </summary>
        /// <param name="uri">URI.</param>
        /// <param name="parseRelative">Парсить также относительные ссылки.</param>
        /// <returns>Результат или null, если не определён.</returns>
        public ILink TryParseLink(string uri, bool parseRelative)
        {
            return TryParsePostLink(uri, parseRelative);
        }

        private ILink TryParsePostLink(string uri, bool parseRelative)
        {
            try
            {
                var regexes = GetRegexesForPostCheck(parseRelative);
                var match = regexes.Select(r => r.Match(uri)).FirstOrDefault(r => r.Success);
                if (match != null)
                {
                    if (match.Groups["post"].Captures.Count > 0)
                    {
                        return new PostLink()
                        {
                            Engine = MakabaConstants.MakabaEngineId,
                            Board = match.Groups["board"].Captures[0].Value,
                            OpPostNum = int.Parse(match.Groups["parent"].Captures[0].Value),
                            PostNum = int.Parse(match.Groups["post"].Captures[0].Value)
                        };
                    }
                    return new ThreadLink()
                    {
                        Engine = MakabaConstants.MakabaEngineId,
                        Board = match.Groups["board"].Captures[0].Value,
                        OpPostNum = int.Parse(match.Groups["parent"].Captures[0].Value),
                    };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// true, если ссылка подходит к данному движку.
        /// </summary>
        /// <param name="uri">URI.</param>
        /// <param name="parseRelative">Парсить также относительные ссылки.</param>
        /// <returns>Результат.</returns>
        public bool IsLinkForEngine(string uri, bool parseRelative)
        {
            try
            {
                var regexes = GetRegexesForPostCheck(parseRelative);
                return regexes.Select(r => r.Match(uri)).Any(r => r.Success);
            }
            catch
            {
                return false;
            }
        }

        private Regex[] GetRegexesForPostCheck(bool parseRelative)
        {
            if (parseRelative)
            {
                return new[] {_postLinkRegex, _postLinkRegex2};
            }
            return new[] {_postLinkRegex};
        }
    }
}