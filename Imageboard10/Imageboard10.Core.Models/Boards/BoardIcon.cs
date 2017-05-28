using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Boards
{
    /// <summary>
    /// Иконка борды.
    /// </summary>
    public class BoardIcon : IBoardIcon, IDeepCloneable<BoardIcon>
    {
        /// <summary>
        /// Имя.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ссылка на медиа.
        /// </summary>
        public ILink MediaLink { get; set; }

        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <param name="modules">Модули.</param>
        /// <returns>Клон.</returns>
        public BoardIcon DeepClone(IModuleProvider modules)
        {
            return new BoardIcon()
            {
                Name = Name,
                MediaLink = MediaLink.CloneLink(modules)
            };
        }
    }
}