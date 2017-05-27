using Windows.Graphics;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// ����� � �������� �����������.
    /// </summary>
    public interface IPostMediaWithSize : IPostMedia
    {
        /// <summary>
        /// ������.
        /// </summary>
        SizeInt32 Size { get; }
    }
}