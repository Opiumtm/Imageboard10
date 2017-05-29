namespace Imageboard10.Core.ModelInterface.Boards
{
    /// <summary>
    /// Запрос на доски в хранилище.
    /// </summary>
    public struct BoardReferenceStoreQuery
    {
        /// <summary>
        /// Категория. Если null - то любая.
        /// </summary>
        public string Category;

        /// <summary>
        /// Запрос по доскам "для взрослых". Если null - не имеет значения.
        /// </summary>
        public bool? IsAdult;
    }
}