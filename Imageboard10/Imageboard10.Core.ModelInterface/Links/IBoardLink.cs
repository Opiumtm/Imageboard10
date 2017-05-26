namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Ссылка на борду.
    /// </summary>
    public interface IBoardLink : ILink
    {
        /// <summary>
        /// Получить ссылку на борду.
        /// </summary>
        /// <returns>Ссылка на борду.</returns>
        ILink GetBoardLink();

        /// <summary>
        /// Получить ссылку на страницу доски.
        /// </summary>
        /// <param name="page">Страница.</param>
        /// <returns></returns>
        ILink GetBoardPageLink(int page);
    }
}