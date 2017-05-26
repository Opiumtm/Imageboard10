using Imageboard10.Core.Models.Links.LinkTypes;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Ссылка на тред.
    /// </summary>
    public interface IThreadLink
    {
        /// <summary>
        /// Пост находится в данном треде.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Результат проверки.</returns>
        bool IsPostFromThisThread(BoardLinkBase link);

        /// <summary>
        /// Получить ссылку на тред.
        /// </summary>
        /// <returns>Ссылка на тред.</returns>
        BoardLinkBase GetThreadLink();

        /// <summary>
        /// Получить ссылку на часть треда.
        /// </summary>
        /// <param name="fromPost">Начиная с номера поста.</param>
        /// <returns>Ссылка на часть треда.</returns>
        BoardLinkBase GetThreadPart(int fromPost);

        /// <summary>
        /// Получить ссылку на пост.
        /// </summary>
        /// <param name="postNumber">Номер поста.</param>
        /// <returns>Ссылка на пост в треде.</returns>
        BoardLinkBase GetPostLink(int postNumber);
    }
}