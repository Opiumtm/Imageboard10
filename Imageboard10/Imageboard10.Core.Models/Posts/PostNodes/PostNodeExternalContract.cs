using System.Runtime.Serialization;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts.PostNodes
{
    /// <summary>
    /// Внешний контракт узла.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostNodeExternalContract : PostNodeBase, IExternalContractHost
    {
        /// <summary>
        /// Внешний контракт.
        /// </summary>
        [DataMember]
        public ExternalContractData Contract { get; set; }
    }
}