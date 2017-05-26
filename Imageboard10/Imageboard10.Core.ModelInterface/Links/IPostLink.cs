namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Ссылка на пост.
    /// </summary>
    public interface IPostLink : IThreadLink
    {
        /// <summary>
        /// Получить строку с номером поста.
        /// </summary>
        /// <returns>Строка с номером поста.</returns>
        string GetPostNumberString();
    }
}