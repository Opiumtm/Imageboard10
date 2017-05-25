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
        /// <returns></returns>
        IAsyncAction DisposeModule();
    }
}