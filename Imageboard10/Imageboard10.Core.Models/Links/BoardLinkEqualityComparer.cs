using System;
using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Средство сравнения ссылок на идентичность.
    /// </summary>
    public sealed class BoardLinkEqualityComparer : IEqualityComparer<ILink>
    {
        /// <summary>
        /// Средство сравнения.
        /// </summary>
        public static readonly IEqualityComparer<ILink> Instance = new BoardLinkEqualityComparer();

        /// <summary>Determines whether the specified objects are equal.</summary>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        public bool Equals(ILink x, ILink y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(x?.GetLinkHash() ?? "", y?.GetLinkHash() ?? "");
        }

        /// <summary>Returns a hash code for the specified object.</summary>
        /// <returns>A hash code for the specified object.</returns>
        /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.</exception>
        public int GetHashCode(ILink obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj?.GetLinkHash() ?? "");
        }
    }
}