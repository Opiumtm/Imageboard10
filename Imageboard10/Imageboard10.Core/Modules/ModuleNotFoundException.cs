using System;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Ошибка вида "модуль не найден".
    /// </summary>
    public class ModuleNotFoundException : Exception
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        public ModuleNotFoundException()
            :base("Запрошенный модуль не найден")
        {            
        }
    }
}