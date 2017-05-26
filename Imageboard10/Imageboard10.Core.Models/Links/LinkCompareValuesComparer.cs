using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Средство сравнения значений для сравнения.
    /// </summary>
    public class LinkCompareValuesComparer : IComparer<LinkCompareValues>
    {
        /// <summary>
        /// Средство сравнения.
        /// </summary>
        public static readonly IComparer<LinkCompareValues> Instance = new LinkCompareValuesComparer();

        /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
        /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.</returns>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        public int Compare(LinkCompareValues x, LinkCompareValues y)
        {
            return CompareTo(x, y);
        }

        private static int CompareTo(LinkCompareValues @this, LinkCompareValues other)
        {
            var er = StringComparer.OrdinalIgnoreCase.Compare(@this.Engine, other.Engine);
            if (er != 0)
            {
                return er;
            }
            var br = StringComparer.OrdinalIgnoreCase.Compare(@this.Board, other.Board);
            if (br != 0)
            {
                return br;
            }
            var bpr = Comparer<int>.Default.Compare(@this.Page, other.Page);
            if (bpr != 0)
            {
                return bpr;
            }
            var tr = Comparer<int>.Default.Compare(@this.Thread, other.Thread);
            if (tr != 0)
            {
                return tr;
            }
            var pr = Comparer<int>.Default.Compare(@this.Post, other.Post);
            if (pr != 0)
            {
                return pr;
            }
            var or = StringComparer.Ordinal.Compare(@this.Other, other.Other);
            if (or != 0)
            {
                return or;
            }
            return 0;
        }
    }
}