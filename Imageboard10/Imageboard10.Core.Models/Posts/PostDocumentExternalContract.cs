using System.Runtime.Serialization;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Внешний контракт документа.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostDocumentExternalContract : PostDocument, IExternalContractHost
    {
        /// <summary>
        /// Внешний контракт.
        /// </summary>
        [DataMember]
        public ExternalContractData Contract { get; set; }
    }
}