using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.ModelStorage.Boards.DataContracts
{
    /// <summary>
    /// Коротка информация о доске.
    /// </summary>
    public class BoardShortInfo : IBoardShortInfo, IDeepCloneable<BoardShortInfo>
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
        /// Клонировать.
        /// </summary>
        /// <param name="modules">Модули.</param>
        /// <returns>Клон.</returns>
        public BoardShortInfo DeepClone(IModuleProvider modules)
        {
            return new BoardShortInfo()
            {
                Category = Category,
                IsAdult = IsAdult,
                ShortName = ShortName,
                DisplayName = DisplayName,
                BoardLink = BoardLink
            };
        }
    }
}