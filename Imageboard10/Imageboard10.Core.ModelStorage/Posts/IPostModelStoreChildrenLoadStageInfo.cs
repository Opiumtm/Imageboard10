namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Состояние загрузки дочерних элементов.
    /// </summary>
    internal interface IPostModelStoreChildrenLoadStageInfo
    {
        byte Stage { get; }
    }
}