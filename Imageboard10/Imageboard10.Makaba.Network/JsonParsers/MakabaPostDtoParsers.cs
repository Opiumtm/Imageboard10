using System;
using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Network;
using Imageboard10.Makaba.Network.Json;

namespace Imageboard10.Makaba.Network.JsonParsers
{
    /// <summary>
    /// Парсер данных поста.
    /// </summary>
    public class MakabaPostDtoParsers : NetworkDtoParsersBase, 
        INetworkDtoParser<BoardPost2WithParentLink, IBoardPost>
    {
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
            throw new NotImplementedException();
        }
    }
}