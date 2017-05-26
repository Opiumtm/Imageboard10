namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Сервис сериализации ссылок.
    /// </summary>
    public interface ILinkSerializationService
    {
        /// <summary>
        /// Сериализовать ссылку.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Ссылка в виде строки.</returns>
        string Serialize(BoardLinkBase link);

        /// <summary>
        /// Десериализовать ссылку.
        /// </summary>
        /// <param name="linkStr">Ссылка в виде строки.</param>
        /// <returns>Ссыока.</returns>
        BoardLinkBase Deserialize(string linkStr);
    }
}