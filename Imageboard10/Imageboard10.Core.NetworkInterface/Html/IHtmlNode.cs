using System;
using System.Collections.Generic;
using Windows.Foundation.Metadata;

namespace Imageboard10.Core.NetworkInterface.Html
{
    /// <summary>
    /// Нода HTML.
    /// </summary>
    public interface IHtmlNode
    {
        /// <summary>
        /// Имя.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Получить значение атрибута.
        /// </summary>
        /// <param name="name">Имя.</param>
        /// <param name="def">Значение по умолчанию.</param>
        /// <returns>Значение.</returns>
        [DefaultOverload]
        string GetAttributeValue(string name, string def);

        /// <summary>
        /// Получить значение атрибута.
        /// </summary>
        /// <param name="name">Имя.</param>
        /// <param name="def">Значение по умолчанию.</param>
        /// <returns>Значение.</returns>
        int GetAttributeValue(string name, int def);

        /// <summary>
        /// Получить значение атрибута.
        /// </summary>
        /// <param name="name">Имя.</param>
        /// <param name="def">Значение по умолчанию.</param>
        /// <returns>Значение.</returns>
        bool GetAttributeValue(string name, bool def);

        /// <summary>
        /// Дочерние ноды.
        /// </summary>
        IList<IHtmlNode> ChildNodes { get; }

        /// <summary>
        /// Есть дочерние ноды.
        /// </summary>
        bool HasChildNodes { get; }

        /// <summary>
        /// Тип ноды (один из интерфейсов, унаследованных от <see cref="IHtmlNode"/>).
        /// </summary>
        Type NodeType { get; }

        /// <summary>
        /// Исходный объект.
        /// </summary>
        object SourceObject { get; }

        /// <summary>
        /// Родительская нода.
        /// </summary>
        IHtmlNode ParentNode { get; }
    }
}