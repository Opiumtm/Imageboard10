using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Boards
{
    /// <summary>
    /// Короткая информация о доске.
    /// </summary>
    public interface IBoardShortInfo
    {
        /// <summary>
        /// Ссылка на доску.
        /// </summary>
        ILink BoardLink { get; }

        /// <summary>
        /// Категория.
        /// </summary>
        string Category { get; }

        /// <summary>
        /// Короткое имя.
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Отображаемое имя.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Доска "только для взрослых".
        /// </summary>
        bool IsAdult { get; }
    }
}