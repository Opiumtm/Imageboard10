using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Пост со ссылкой на родительский тред.
    /// </summary>
    public struct BoardPost2WithParentLink
    {
        /// <summary>
        /// Пост.
        /// </summary>
        public BoardPost2 Post;

        /// <summary>
        /// Ссылка на тред.
        /// </summary>
        public IThreadLink ParentLink;

        /// <summary>
        /// Предварительный просмотр треда.
        /// </summary>
        public bool IsPreview;
    }
}