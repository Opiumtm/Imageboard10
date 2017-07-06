namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Есть информация о ETAG.
    /// </summary>
    public interface IBoardPostCollectionEtagEnabled
    {
        /// <summary>
        /// Штамп изменений.
        /// </summary>
        string Etag { get; }
    }
}