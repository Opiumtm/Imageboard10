using System;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Класс помощник для отладки.
    /// </summary>
    public static class DebugHelper
    {
        /// <summary>
        /// Остановиться при ошибке.
        /// </summary>
        /// <param name="ex">Ошибка.</param>
        public static void BreakOnError(Exception ex)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
#endif
        }
    }
}