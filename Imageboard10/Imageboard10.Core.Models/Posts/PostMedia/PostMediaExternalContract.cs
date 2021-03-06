﻿using System.Runtime.Serialization;
using Imageboard10.Core.Models.Posts.Serialization;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts.PostMedia
{
    /// <summary>
    /// Внешний объект.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostMediaExternalContract : PostMediaBase, IExternalContractHost
    {
        /// <summary>
        /// Внешний контракт.
        /// </summary>
        [DataMember]
        public ExternalContractData Contract { get; set; }
    }
}