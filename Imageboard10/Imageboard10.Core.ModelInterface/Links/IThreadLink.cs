namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Ссылка на тред.
    /// </summary>
    public interface IThreadLink : IBoardLink
    {
        /// <summary>
        /// Пост находится в данном треде.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Результат проверки.</returns>
        bool IsPostFromThisThread(ILink link);

        /// <summary>
        /// Получить ссылку на тред.
        /// </summary>
        /// <returns>Ссылка на тред.</returns>
        ILink GetThreadLink();

        /// <summary>
        /// Получить ссылку на часть треда.
        /// </summary>
        /// <param name="fromPost">Начиная с номера поста.</param>
        /// <returns>Ссылка на часть треда.</returns>
        ILink GetThreadPart(int fromPost);

        /// <summary>
        /// Получить ссылку на пост.
        /// </summary>
        /// <param name="postNumber">Номер поста.</param>
        /// <returns>Ссылка на пост в треде.</returns>
        ILink GetPostLink(int postNumber);
    }
}