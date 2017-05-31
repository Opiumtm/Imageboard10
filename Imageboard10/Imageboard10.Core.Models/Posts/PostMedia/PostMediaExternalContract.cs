using System.Runtime.Serialization;

namespace Imageboard10.Core.Models.Posts.PostMedia
{
    /// <summary>
    /// Внешний объект.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostMediaExternalContract : PostMediaBase
    {
        /// <summary>
        /// Идентификатор типа.
        /// </summary>
        [DataMember]
        public string TypeId { get; set; }

        /// <summary>
        /// Бинарные данные в формате Base64.
        /// </summary>
        [DataMember]
        public string BinaryData { get; set; }
    }
}