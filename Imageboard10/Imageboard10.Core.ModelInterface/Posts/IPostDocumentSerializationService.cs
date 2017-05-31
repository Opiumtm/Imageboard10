using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Metadata;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Сервис сериализации ноды документа поста.
    /// </summary>
    public interface IPostDocumentSerializationService
    {
        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="document">Документ.</param>
        /// <returns>Сериализованный документ.</returns>
        string SerializeToString(IPostDocument document);

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="document">Документ.</param>
        /// <returns>Сериализованный документ.</returns>
        byte[] SerializeToBytes(IPostDocument document);

        /// <summary>
        /// Десериализовать.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Документ.</returns>
        [DefaultOverload]
        IPostDocument Deserialize(string data);

        /// <summary>
        /// Десериализовать.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Документ.</returns>
        IPostDocument Deserialize([ReadOnlyArray] byte[] data);
    }
}