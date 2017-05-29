using System;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Ошибка: модуль не готов к использованию.
    /// </summary>
    public class ModuleNotReadyException : Exception
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        public ModuleNotReadyException()
            :base("Модуль не готов к использованию - не инициализирован, завершён или приостановлен")
        {            
        }
    }
}