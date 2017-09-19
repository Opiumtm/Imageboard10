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
        SizeOfInt32 Size { get; }
    }
}