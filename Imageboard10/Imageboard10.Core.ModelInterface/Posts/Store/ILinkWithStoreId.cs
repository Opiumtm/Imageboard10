using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Ссылка с идентификатором в хранилище.
    /// </summary>
    public interface ILinkWithStoreId
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        PostStoreEntityId Id { get; }

        /// <summary>
        /// Ссылка.
        /// </summary>
        ILink Link { get; }
    }
}