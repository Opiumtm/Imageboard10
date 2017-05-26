namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Ссылка на ютуб.
    /// </summary>
    public interface IYoutubeLink : IUriLink
    {
        /// <summary>
        /// Идентификатор ютуба.
        /// </summary>
        string YoutubeId { get; }

        /// <summary>
        /// Получить URI предпросмотра.
        /// </summary>
        /// <returns>URI картинки предпросмотра.</returns>
        string GetThumbnailUri();
    }
}