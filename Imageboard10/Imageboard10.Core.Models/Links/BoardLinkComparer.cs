﻿using System.Collections;
using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Средство сравнения ссылок.
    /// </summary>
    public class BoardLinkComparer : IComparer<ILink>, IComparer
    {
        /// <summary>
        /// Средство сравнения.
        /// </summary>
        public static readonly IComparer<ILink> Instance = new BoardLinkComparer();

        /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
        /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.</returns>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        public int Compare(ILink x, ILink y)
        {
            if (x?.GetLinkHash() == y?.GetLinkHash())
            {
                return 0;
            }
            var x1 = GetValue(x);
            var y1 = GetValue(y);
            return LinkCompareValuesComparer.Instance.Compare(x1, y1);
        }

        private LinkCompareValues GetValue(ILink link)
        {
            if (link != null)
            {
                return link.GetCompareValues();
            }
            return new LinkCompareValues()
            {
                Engine = "",
                Board = "",
                Page = 0,
                Post = 0,
                Thread = 0,
                Other = ""
            };
        }

        /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
        /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero <paramref name="x" /> is less than <paramref name="y" />. Zero <paramref name="x" /> equals <paramref name="y" />. Greater than zero <paramref name="x" /> is greater than <paramref name="y" />. </returns>
        /// <param name="x">The first object to compare. </param>
        /// <param name="y">The second object to compare. </param>
        /// <exception cref="T:System.ArgumentException">Neither <paramref name="x" /> nor <paramref name="y" /> implements the <see cref="T:System.IComparable" /> interface.-or- <paramref name="x" /> and <paramref name="y" /> are of different types and neither one can handle comparisons with the other. </exception>
        /// <filterpriority>2</filterpriority>
        public int Compare(object x, object y)
        {
            return Compare(x as ILink, y as ILink);
        }
    }
}