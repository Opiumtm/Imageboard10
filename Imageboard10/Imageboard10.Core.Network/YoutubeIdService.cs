using System;
using Imageboard10.Core.Modules;
using Imageboard10.Core.NetworkInterface;
using Imageboard10.Core.Utility;

namespace Imageboard10.Core.Network
{
    /// <summary>
    /// Сервис получения ID ютубы.
    /// </summary>
    public sealed class YoutubeIdService : ModuleBase<IYoutubeIdService>, IYoutubeIdService
    {
        private const string YoutubeRegex = @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)";

        /// <summary>
        /// Получить идентификатор ютубы.
        /// </summary>
        /// <param name="uri">URI.</param>
        /// <returns>Идентификатор.</returns>
        public string GetYoutubeIdFromUri(string uri)
        {
            try
            {
                if (uri == null) return null;
                var youtubeRegex = RegexCache.CreateRegex(YoutubeRegex);
                var match = youtubeRegex.Match(uri);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}