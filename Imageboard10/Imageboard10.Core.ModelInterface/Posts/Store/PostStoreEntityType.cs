namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Тип элемента.
    /// </summary>
    public enum PostStoreEntityType : int
    {
        /// <summary>
        /// Пост.
        /// </summary>
        Post = 0,

        /// <summary>
        /// Пост из превью треда.
        /// </summary>
        ThreadPreviewPost = 1,

        /// <summary>
        /// Пост из каталога.
        /// </summary>
        CatalogPost = 2,

        /// <summary>
        /// Коллекция постов.
        /// </summary>
        Thread = 3,

        /// <summary>
        /// Каталог.
        /// </summary>
        Catalog = 4,

        /// <summary>
        /// Превью треда.
        /// </summary>
        ThreadPreview = 5,

        /// <summary>
        /// Страница борды.
        /// </summary>
        BoardPage = 6,
    }
}