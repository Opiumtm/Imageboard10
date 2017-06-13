namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Возможность прервать обработку.
    /// </summary>
    public interface ITreeWalkContextBreak
    {
        /// <summary>
        /// Прервать обработку.
        /// </summary>
        bool IsBreak { get; set; }
    }
}