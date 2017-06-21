using System.Runtime.Serialization;
using Imageboard10.Core;

namespace Imageboard10.Makaba.Models.Posts.Serialization
{
    /// <summary>
    /// Реклама доски.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class MakabaBoardPostCollectionInfoBoardsAdvertisementItemContract
    {
        /// <summary>
        /// Ссылка на доску.
        /// </summary>
        [DataMember]
        public string BoardLink { get; set; }

        /// <summary>
        /// Имя.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Информация.
        /// </summary>
        [DataMember]
        public string Info { get; set; }
    }
}