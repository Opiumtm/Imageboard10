namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Коллекция постов со штампом изменения.
    /// </summary>
    public interface IBoardPostCollectionEtag : IBoardPostCollection, IBoardPostCollectionEtagEnabled
    {
    }
}