using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.PostNodes
{
    /// <summary>
    /// Текстовый узел.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class TextPostNode : PostNodeBase, ITextPostNode
    {
        /// <summary>
        /// Текст.
        /// </summary>
        [DataMember]
        public string Text { get; set; }
    }
}