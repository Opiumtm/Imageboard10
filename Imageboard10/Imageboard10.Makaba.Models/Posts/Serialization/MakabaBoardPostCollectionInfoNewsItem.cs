using System.Runtime.Serialization;
using Imageboard10.Core;

namespace Imageboard10.Makaba.Models.Posts.Serialization
{
    /// <summary>
    /// ������� ��������.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class MakabaBoardPostCollectionInfoNewsItemContract
    {
        /// <summary>
        /// ����.
        /// </summary>
        [DataMember]
        public string Date { get; set; }

        /// <summary>
        /// ������ �� �������.
        /// </summary>
        [DataMember]
        public string NewsLink { get; set; }

        /// <summary>
        /// ���������.
        /// </summary>
        [DataMember]
        public string Title { get; set; }
    }
}