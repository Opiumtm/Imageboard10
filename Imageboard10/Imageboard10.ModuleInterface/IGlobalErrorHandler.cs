﻿using System;

namespace Imageboard10.ModuleInterface
{
    /// <summary>
    /// Глобальный обработчик ошибок.
    /// </summary>
    public interface IGlobalErrorHandler
    {
        /// <summary>
        /// Сигнализировать об ошибке. Ни в коем случае не должен кидать исключения сам.
        /// </summary>
        /// <param name="error">Ошибка.</param>
        void SignalError(Exception error);
    }
}