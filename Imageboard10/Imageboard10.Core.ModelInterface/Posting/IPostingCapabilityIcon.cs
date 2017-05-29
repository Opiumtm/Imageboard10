namespace Imageboard10.Core.ModelInterface.Posting
{
    /// <summary>
    /// Иконка.
    /// </summary>
    public interface IPostingCapabilityIcon
    {
        /// <summary>
        /// Идентификатор иконки.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Имя.
        /// </summary>
        string Name { get; }
    }
}