namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Ссылка, имеющая URL.
    /// </summary>
    public interface IUriLink : ILink
    {
        /// <summary>
        /// Абсолютная ссылка.
        /// </summary>
        bool IsAbsolute { get; }

        /// <summary>
        /// Получить абсолютную ссылку.
        /// </summary>
        /// <returns>Абсолютная ссылка.</returns>
        string GetAbsoluteUrl();
    }
}