﻿using System.IO;
using Windows.Storage.Streams;
using HtmlAgilityPack;
using Imageboard10.Core.Modules;
using Imageboard10.Core.NetworkInterface.Html;

namespace Imageboard10.Core.Network.Html
{
    /// <summary>
    /// Фабрика документов HTML.
    /// </summary>
    public sealed class AgilityHtmlDocumentFactory : ModuleBase<IHtmlDocumentFactory>, IHtmlDocumentFactory
    {
        /// <summary>
        /// Загрузить документ.
        /// </summary>
        /// <param name="content">Содержимое.</param>
        /// <returns>Документ.</returns>
        public IHtmlDocument Load(string content)
        {
            var result = new HtmlDocument();
            result.LoadHtml(content);
            return new AgilityHtmlDocument(result);
        }

        /// <summary>
        /// Загрузить документ.
        /// </summary>
        /// <param name="stream">Поток.</param>
        /// <returns>Документ.</returns>
        public IHtmlDocument Load(IInputStream stream)
        {
            var result = new HtmlDocument();
            using (var s = stream.AsStreamForRead())
            {
                result.Load(s);
            }
            return new AgilityHtmlDocument(result);
        }

        /// <summary>
        /// Конвертировать все html entity в unicode.
        /// </summary>
        /// <param name="src">Исходная строка.</param>
        /// <returns>Результат.</returns>
        public string DeEntitize(string src) => HtmlEntity.DeEntitize(src);

        /// <summary>
        /// Проверка на пересечение элементов.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <returns>Результат.</returns>
        public bool IsOverlappedClosingElement(string text) => HtmlNode.IsOverlappedClosingElement(text);
    }
}