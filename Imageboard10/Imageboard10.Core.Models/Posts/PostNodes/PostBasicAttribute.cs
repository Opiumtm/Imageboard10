using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.PostNodes
{
    /// <summary>
    /// ������� �������.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostBasicAttribute : PostNodeAttribute, IPostBasicAttribute
    {
        /// <summary>
        /// �������.
        /// </summary>
        [DataMember]
        public string Attribute { get; set; }
    }
}