using System.Collections.Generic;

namespace Imageboard10.Core.ModelInterface.Boards
{
    /// <summary>
    /// Категория досок.
    /// </summary>
    public interface IBoardCategory
    {
        /// <summary>
        /// Имя категории.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Доски.
        /// </summary>
        IList<IBoardReference> Boards { get; }
    }
}