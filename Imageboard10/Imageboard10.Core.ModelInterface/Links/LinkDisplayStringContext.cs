namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Контекст изображения ссылки.
    /// </summary>
    public enum LinkDisplayStringContext
    {
        /// <summary>
        /// Без контекста.
        /// </summary>
        None,
        /// <summary>
        /// Один движок.
        /// </summary>
        Engine,
        /// <summary>
        /// Доска.
        /// </summary>
        Board,
        /// <summary>
        /// Тред.
        /// </summary>
        Thread
    }
}