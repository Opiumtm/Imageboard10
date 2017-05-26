namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Ссылка, имеющая URL.
    /// </summary>
    public interface IUriLink
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