namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Режим обновления коллекции.
    /// </summary>
    public enum BoardPostCollectionUpdateMode
    {
        /// <summary>
        /// Слить. Только для существующего треда. Информация о самом треде не обновляется.
        /// </summary>
        Merge,

        /// <summary>
        /// Полностью заменить.
        /// </summary>
        Replace,

        /// <summary>
        /// Заменить и пометить удалённые посты флагом.
        /// </summary>
        ReplaceAndHideDeleted
    }
}