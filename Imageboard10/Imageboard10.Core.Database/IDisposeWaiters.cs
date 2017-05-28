using System.Threading.Tasks;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Регистрация использования сессий ESENT.
    /// </summary>
    internal interface IDisposeWaiters
    {
        /// <summary>
        /// Зарегистрировать использование.
        /// </summary>
        /// <param name="task">Таск.</param>
        void RegisterWaiter(Task task);

        /// <summary>
        /// Удалить использование.
        /// </summary>
        /// <param name="task">Таск.</param>
        void RemoveWaiter(Task task);
    }
}