namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Тэг тредов на доске.
    /// </summary>
    public interface IThreadTagLink : IBoardLink
    {
        /// <summary>
        /// Тэг.
        /// </summary>
        string Tag { get; }
    }
}