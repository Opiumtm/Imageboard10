using Imageboard10.Core.Models.Links.LinkTypes;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Ссылка на борду.
    /// </summary>
    public interface IBoardLink
    {
        /// <summary>
        /// Получить ссылку на борду.
        /// </summary>
        /// <returns>Ссылка на борду.</returns>
        BoardLinkBase GetBoardLink();

        /// <summary>
        /// Получить ссылку на страницу доски.
        /// </summary>
        /// <param name="page">Страница.</param>
        /// <returns></returns>
        BoardLinkBase GetBoardPageLink(int page);
    }
}