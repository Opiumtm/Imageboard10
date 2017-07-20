using System.Collections.Generic;
using System.Linq;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Links;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Класс-помощник для коллекции постов.
    /// </summary>
    public static class PostCollectionHelpers
    {
        /// <summary>
        /// Получить квоты.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        /// <returns>Квоты.</returns>
        public static ILookup<ILink, ILink> GetQuotesLookup(this IBoardPostCollection collection)
        {
            if (collection?.Posts == null)
            {
                return Enumerable.Empty<KeyValuePair<ILink, ILink>>().ToLookup(l => l.Key, l => l.Value, BoardLinkEqualityComparer.Instance);
            }
            return collection.Posts
                .SelectMany(p => (p.Comment?.GetQuotes() ?? Enumerable.Empty<ILink>()).Distinct(BoardLinkEqualityComparer.Instance).Select(q => new KeyValuePair<ILink, ILink>(q, p.Link)))
                .ToLookup(l => l.Key, l => l.Value, BoardLinkEqualityComparer.Instance);
        }
    }
}