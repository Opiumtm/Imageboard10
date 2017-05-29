using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posting;

namespace Imageboard10.Core.ModelInterface.Boards
{
    /// <summary>
    /// Ссылка на доску.
    /// </summary>
    public interface IBoardReference : IBoardShortInfo
    {
        /// <summary>
        /// Поля для постинга.
        /// </summary>
        IList<IPostingCapability> PostingCapabilities { get; }

        /// <summary>
        /// Иконки.
        /// </summary>
        IList<IBoardIcon> Icons { get; }

        /// <summary>
        /// Бамплимит.
        /// </summary>
        int? BumpLimit { get; }

        /// <summary>
        /// Имя постера по умолчанию.
        /// </summary>
        string DefaultName { get; }

        /// <summary>
        /// Количество страниц.
        /// </summary>
        int? Pages { get; }

        /// <summary>
        /// Разрешены лайки.
        /// </summary>
        bool LikesEnabled { get; }
    }
}