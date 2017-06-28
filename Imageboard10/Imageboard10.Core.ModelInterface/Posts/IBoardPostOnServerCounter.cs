namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Номер поста согласно сервера.
    /// </summary>
    public interface IBoardPostOnServerCounter
    {
        /// <summary>
        /// Нумерация на сервере в треде.
        /// </summary>
        int? OnServerCounter { get; }
    }
}