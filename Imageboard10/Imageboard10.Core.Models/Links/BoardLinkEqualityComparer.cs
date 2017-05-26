using System;
using System.Collections.Generic;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Средство сравнения ссылок на идентичность.
    /// </summary>
    public sealed class BoardLinkEqualityComparer : IEqualityComparer<BoardLinkBase>
    {
        /// <summary>
        /// Средство сравнения.
        /// </summary>
        public static readonly IEqualityComparer<BoardLinkBase> Instance = new BoardLinkEqualityComparer();

        /// <summary>Determines whether the specified objects are equal.</summary>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        public bool Equals(BoardLinkBase x, BoardLinkBase y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(x?.GetLinkHash() ?? "", y?.GetLinkHash() ?? "");
        }

        /// <summary>Returns a hash code for the specified object.</summary>
        /// <returns>A hash code for the specified object.</returns>
        /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.</exception>
        public int GetHashCode(BoardLinkBase obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj?.GetLinkHash() ?? "");
        }
    }
}