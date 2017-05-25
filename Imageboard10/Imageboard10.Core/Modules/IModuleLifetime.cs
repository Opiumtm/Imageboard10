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
    }
}