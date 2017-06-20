using Windows.UI;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Информация о постере.
    /// </summary>
    public class PosterInfo : IPosterInfo
    {
        /// <summary>
        /// Имя.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Трипкод.
        /// </summary>
        public string Tripcode { get; set; }

        /// <summary>
        /// Цвет имени.
        /// </summary>
        public string NameColorStr { get; set; }

        /// <summary>
        /// Цвет имени.
        /// </summary>
        public Color? NameColor { get; set; }
    }
}