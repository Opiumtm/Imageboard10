using System;
using System.Runtime.Serialization;

namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// Данные внешнего объекта.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class ExternalContractData
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