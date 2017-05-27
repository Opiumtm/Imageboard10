namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Базовый атрибут.
    /// </summary>
    public interface IPostBasicAttribute : IPostAttribute
    {
        /// <summary>
        /// Атрибут.
        /// </summary>
        string Attribute { get; }
    }
}