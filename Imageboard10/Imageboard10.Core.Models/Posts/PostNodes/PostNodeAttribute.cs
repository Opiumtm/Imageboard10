using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts.PostNodes
{
    /// <summary>
    /// Атрибут поста.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    [KnownType(typeof(PostBasicAttribute))]
    [KnownType(typeof(PostLinkAttribute))]
    [KnownType(typeof(PostAttributeExternalContract))]
    public abstract class PostNodeAttribute : IPostAttribute
    {
    }
}