namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Текстовый узел.
    /// </summary>
    public interface ITextPostNode : IPostNode
    {
        /// <summary>
        /// Текст.
        /// </summary>
        string Text { get; }
    }
}