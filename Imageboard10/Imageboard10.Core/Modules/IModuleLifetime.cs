using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
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
        ValueTask<Nothing> InitializeModule(IModuleProvider provider);

        /// <summary>
        /// Завершить работу модуля.
        /// </summary>
        ValueTask<Nothing> DisposeModule();

        /// <summary>
        /// Приостановить работу модуля.
        /// </summary>
        ValueTask<Nothing> SuspendModule();

        /// <summary>
        /// Возобновить работу модуля.
        /// </summary>
        ValueTask<Nothing> ResumeModule();

        /// <summary>
        /// Все модули возобновлены.
        /// </summary>
        ValueTask<Nothing> AllModulesResumed();

        /// <summary>
        /// Все модули инициализированы.
        /// </summary>
        ValueTask<Nothing> AllModulesInitialized();

        /// <summary>
        /// Поддерживает приостановку и восстановление.
        /// </summary>
        bool IsSuspendAware { get; }
    }
}