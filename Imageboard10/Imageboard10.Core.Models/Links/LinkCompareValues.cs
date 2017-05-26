using System;
using System.Collections.Generic;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Значения для сортировки ссылок.
    /// </summary>
    public struct LinkCompareValues : IComparable<LinkCompareValues>
    {
        /// <summary>
        /// Движок.
        /// </summary>
        public string Engine;

        /// <summary>
        /// Борда.
        /// </summary>
        public string Board;

        /// <summary>
        /// Страница.
        /// </summary>
        public int Page;

        /// <summary>
        /// Тред.
        /// </summary>
        public int Thread;

        /// <summary>
        /// Пост.
        /// </summary>
        public int Post;

        /// <summary>
        /// Другая информация.
        /// </summary>
        public string Other;

        /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object. </summary>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  Zero This instance occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order. </returns>
        /// <param name="other">An object to compare with this instance. </param>
        public int CompareTo(LinkCompareValues other)
        {
            var er = StringComparer.OrdinalIgnoreCase.Compare(Engine, other.Engine);
            if (er != 0)
            {
                return er;
            }
            var br = StringComparer.OrdinalIgnoreCase.Compare(Board, other.Board);
            if (br != 0)
            {
                return br;
            }
            var bpr = Comparer<int>.Default.Compare(Page, other.Page);
            if (bpr != 0)
            {
                return bpr;
            }
            var tr = Comparer<int>.Default.Compare(Thread, other.Thread);
            if (tr != 0)
            {
                return tr;
            }
            var pr = Comparer<int>.Default.Compare(Post, other.Post);
            if (pr != 0)
            {
                return pr;
            }
            var or = StringComparer.Ordinal.Compare(Other, other.Other);
            if (or != 0)
            {
                return or;
            }
            return 0;
        }

        /// <summary>
        /// Пустой набор значений.
        /// </summary>
        public static readonly LinkCompareValues Empty = new LinkCompareValues()
        {
            Board = "",
            Engine = "",
            Other = "",
            Post = 0,
            Thread = 0,
            Page = 0
        };
    }
}