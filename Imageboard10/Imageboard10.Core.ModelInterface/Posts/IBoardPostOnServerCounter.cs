namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// ����� ����� �������� �������.
    /// </summary>
    public interface IBoardPostOnServerCounter
    {
        /// <summary>
        /// ��������� �� ������� � �����.
        /// </summary>
        int? OnServerCounter { get; }
    }
}