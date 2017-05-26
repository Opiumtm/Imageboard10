using System;
using System.Collections.Generic;

namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Значения для сортировки ссылок.
    /// </summary>
    public struct LinkCompareValues
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
    }
}