using System.Runtime.Serialization;
using Imageboard10.Core;

namespace Imageboard10.Makaba.Models.Posts.Serialization
{
    /// <summary>
    /// Контракт иконки.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class BoardIconContract
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Имя.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Ссылка.
        /// </summary>
        [DataMember]
        public string MediaLink { get; set; }
    }
}