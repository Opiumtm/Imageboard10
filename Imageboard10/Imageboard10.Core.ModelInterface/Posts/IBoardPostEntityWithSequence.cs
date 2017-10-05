namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Поддержка оригинального порядка.
    /// </summary>
    public interface IBoardPostEntityWithSequence : IBoardPostEntity
    {
        /// <summary>
        /// Порядок на странице.
        /// </summary>
        int OnPageSequence { get; }
    }
}