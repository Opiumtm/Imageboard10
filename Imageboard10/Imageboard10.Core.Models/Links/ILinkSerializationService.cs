using Imageboard10.Core.ModelInterface.Links;

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
        string Serialize(ILink link);

        /// <summary>
        /// Десериализовать ссылку.
        /// </summary>
        /// <param name="linkStr">Ссылка в виде строки.</param>
        /// <returns>Ссыока.</returns>
        ILink Deserialize(string linkStr);
    }
}