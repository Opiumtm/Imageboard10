namespace Imageboard10.ModuleInterface
{
    /// <summary>
    /// События времени жизни модуля.
    /// </summary>
    public interface IModuleLifetimeEvents
    {
        /// <summary>
        /// Работа модуля завершена.
        /// </summary>
        event ModuleLifetimeEventHandler Disposed;
    }
}