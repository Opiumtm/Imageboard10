namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Режим загрузки сущностей.
    /// </summary>
    public enum PostStoreLoadMode
    {
        /// <summary>
        /// Загрузка полных данных с рекурсией (загрузка также всех дочерних сущностей).
        /// </summary>
        FullRecursive,

        /// <summary>
        /// Загрузка данных с дочерними сущностями (один уровень рекурсии).
        /// </summary>
        FullWithChildren,

        /// <summary>
        /// Загрузка полных данных.
        /// </summary>
        Full,

        /// <summary>
        /// Облегчённые данные.
        /// </summary>
        Light,

        /// <summary>
        /// Только общую для всех типов информацию сущности.
        /// </summary>
        EntityOnly,

        /// <summary>
        /// Только ссылки.
        /// </summary>
        LinksOnly
    }
}