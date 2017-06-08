using System.Runtime.Serialization;
using Imageboard10.Core.Models.Posts.Serialization;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts.PostNodes
{
    /// <summary>
    /// Внешний контракт.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostAttributeExternalContract : PostAttributeBase, IExternalContractHost
    {
        /// <summary>
        /// Внешний контракт.
        /// </summary>
        [DataMember]
        public ExternalContractData Contract { get; set; }
    }
}