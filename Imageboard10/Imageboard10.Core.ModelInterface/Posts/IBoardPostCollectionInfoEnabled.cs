namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Есть информация о коллекции.
    /// </summary>
    public interface IBoardPostCollectionInfoEnabled
    {
        /// <summary>
        /// Дополнительная информация.
        /// </summary>
        IBoardPostCollectionInfoSet Info { get; }
    }
}