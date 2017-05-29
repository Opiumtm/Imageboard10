using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posting;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Boards
{
    /// <summary>
    /// Ссылка на доску.
    /// </summary>
    public class BoardReference : IBoardReference
    {
        /// <summary>
        /// Ссылка на доску.
        /// </summary>
        public ILink BoardLink { get; set; }

        /// <summary>
        /// Категория.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Короткое имя.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Отображаемое имя.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Доска "только для взрослых".
        /// </summary>
        public bool IsAdult { get; set; }

        /// <summary>
        /// Поля для постинга.
        /// </summary>
        public IList<IPostingCapability> PostingCapabilities { get; set; }

        /// <summary>
        /// Иконки.
        /// </summary>
        public IList<IBoardIcon> Icons { get; set; }

        /// <summary>
        /// Бамплимит.
        /// </summary>
        public int? BumpLimit { get; set; }

        /// <summary>
        /// Имя постера по умолчанию.
        /// </summary>
        public string DefaultName { get; set; }

        /// <summary>
        /// Количество страниц.
        /// </summary>
        public int? Pages { get; set; }

        /// <summary>
        /// Разрешены лайки.
        /// </summary>
        public bool LikesEnabled { get; set; }

        /// <summary>
        /// Разрешены трипкоды.
        /// </summary>
        public bool TripCodesEnabled { get; set; }

        /// <summary>
        /// Разрешена сажа.
        /// </summary>
        public bool SageEnabled { get; set; }

        /// <summary>
        /// Разрешены тэги тредов.
        /// </summary>
        public bool ThreadTagsEnabled { get; set; }
    }
}