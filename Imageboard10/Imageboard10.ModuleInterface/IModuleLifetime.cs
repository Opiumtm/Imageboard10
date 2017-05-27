using Windows.Foundation;

namespace Imageboard10.ModuleInterface
{
    /// <summary>
    /// Управление временем жизни модуля.
    /// </summary>
    public interface IModuleLifetime
    {
        /// <summary>
        /// Инициализировать модуль.
        /// </summary>
        /// <param name="provider">Провайдер модулей.</param>
        IAsyncAction InitializeModule(IModuleProvider provider);

        /// <summary>
        /// Завершить работу модуля.
        /// </summary>
        IAsyncAction DisposeModule();

        /// <summary>
        /// Приостановить работу модуля.
        /// </summary>
        IAsyncAction SuspendModule();

        /// <summary>
        /// Возобновить работу модуля.
        /// </summary>
        IAsyncAction ResumeModule();

        /// <summary>
        /// Все модули возобновлены.
        /// </summary>
        IAsyncAction AllModulesResumed();

        /// <summary>
        /// Все модули инициализированы.
        /// </summary>
        IAsyncAction AllModulesInitialized();

        /// <summary>
        /// Поддерживает приостановку и восстановление.
        /// </summary>
        bool IsSuspendAware { get; }
    }
}