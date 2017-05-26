using System;

namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Сериализатор ссылок.
    /// </summary>
    public interface ILinkSerializer
    {
        /// <summary>
        /// Идентификатор типа ссылки.
        /// </summary>
        string LinkTypeId { get; }

        /// <summary>
        /// Тип ссылки.
        /// </summary>
        Type LinkType { get; }

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