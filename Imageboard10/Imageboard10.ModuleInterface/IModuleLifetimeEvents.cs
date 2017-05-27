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

        /// <summary>
        /// Работа приостановлена.
        /// </summary>
        event ModuleLifetimeEventHandler Suspended;

        /// <summary>
        /// Работа возобновлена.
        /// </summary>
        event ModuleLifetimeEventHandler Resumed;

        /// <summary>
        /// Работа возобновлена для всех модулей.
        /// </summary>
        event ModuleLifetimeEventHandler AllResumed;

        /// <summary>
        /// Все модули инициализированы.
        /// </summary>
        event ModuleLifetimeEventHandler AllInitialized;
    }
}