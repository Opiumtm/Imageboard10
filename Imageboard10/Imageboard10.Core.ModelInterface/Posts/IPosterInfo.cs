using Windows.UI;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Информация о постере.
    /// </summary>
    public interface IPosterInfo
    {
        /// <summary>
        /// Имя.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Трипкод.
        /// </summary>
        string Tripcode { get; }

        /// <summary>
        /// Цвет имени.
        /// </summary>
        string NameColorStr { get; }

        /// <summary>
        /// Цвет имени.
        /// </summary>
        Color? NameColor { get; }
    }
}