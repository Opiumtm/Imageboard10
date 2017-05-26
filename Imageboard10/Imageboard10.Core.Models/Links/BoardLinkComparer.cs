using System.Collections.Generic;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Средство сравнения ссылок.
    /// </summary>
    public class BoardLinkComparer : IComparer<BoardLinkBase>
    {
        /// <summary>
        /// Средство сравнения.
        /// </summary>
        public static readonly IComparer<BoardLinkBase> Instance = new BoardLinkComparer();

        /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
        /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.</returns>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        public int Compare(BoardLinkBase x, BoardLinkBase y)
        {
            var x1 = GetValue(x);
            var y1 = GetValue(y);
            return x1.CompareTo(y1);
        }

        private LinkCompareValues GetValue(BoardLinkBase link)
        {
            if (link != null)
            {
                return link.GetCompareValues();
            }
            return LinkCompareValues.Empty;
        }
    }
}