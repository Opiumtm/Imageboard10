using System.Runtime.Serialization;
using Imageboard10.Core;

namespace Imageboard10.Makaba.Models.Posts.Serialization
{
    /// <summary>
    /// Элемент новостей.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class MakabaBoardPostCollectionInfoNewsItemContract
    {
        /// <summary>
        /// Дата.
        /// </summary>
        [DataMember]
        public string Date { get; set; }

        /// <summary>
        /// Ссылка на новость.
        /// </summary>
        [DataMember]
        public string NewsLink { get; set; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        [DataMember]
        public string Title { get; set; }
    }
}