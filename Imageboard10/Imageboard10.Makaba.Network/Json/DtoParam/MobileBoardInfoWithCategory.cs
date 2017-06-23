namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Информация о мобильной доске с категорией.
    /// </summary>
    public struct MobileBoardInfoWithCategory
    {
        /// <summary>
        /// Доска.
        /// </summary>
        public MobileBoardInfo Board;

        /// <summary>
        /// Категория.
        /// </summary>
        public string Category;
    }
}