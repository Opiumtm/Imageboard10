namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Действие по обновлению флага.
    /// </summary>
    public enum FlagUpdateOperation
    {
        /// <summary>
        /// Добавить.
        /// </summary>
        Add,
        /// <summary>
        /// Удалить.
        /// </summary>
        Remove,
        /// <summary>
        /// Сбросить все.
        /// </summary>
        Clear
    }
}