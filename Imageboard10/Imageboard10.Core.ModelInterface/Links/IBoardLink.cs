namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Ссылка на борду.
    /// </summary>
    public interface IBoardLink : IEngineLink
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
        /// <returns>Ссылка на страницу.</returns>
        ILink GetBoardPageLink(int page);

        /// <summary>
        /// Создать ссылку на каталог.
        /// </summary>
        /// <param name="sortMode">Режим сортировки.</param>
        /// <returns>Ссылка на каталог.</returns>
        ILink GetCatalogLink(BoardCatalogSort sortMode);

        /// <summary>
        /// Получить ссылку на тэг тредов.
        /// </summary>
        /// <param name="tag">Тэг.</param>
        /// <returns>Ссылка на тэг тредов.</returns>
        ILink GetTagLink(string tag);
    }
}