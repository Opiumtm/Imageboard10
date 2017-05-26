namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Ссылка на каталог.
    /// </summary>
    public interface ICatalogLink : IBoardLink
    {
        /// <summary>
        /// Режим сортировки.
        /// </summary>
        BoardCatalogSort SortMode { get; }
    }
}