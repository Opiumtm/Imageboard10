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
        /// <param name="message">Сообщение.</param>
        public ModuleNotFoundException(string message)
            :base(message)
        {            
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="type">Тип модуля.</param>
        public ModuleNotFoundException(Type type)
            : base($"Запрошенный модуль не найден. Тип: {type.FullName}")
        {
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public ModuleNotFoundException()
            :base("Запрошенный модуль не найден")
        {            
        }
    }
}