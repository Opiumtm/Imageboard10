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
        /// Коллекция постов.
        /// </summary>
        Thread = 1,

        /// <summary>
        /// Каталог.
        /// </summary>
        Catalog = 2,

        /// <summary>
        /// Превью треда.
        /// </summary>
        ThreadPreview = 3,

        /// <summary>
        /// Страница борды.
        /// </summary>
        BoardPage = 4,
    }
}