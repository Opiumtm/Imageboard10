﻿using System;
using Windows.Foundation;

namespace Imageboard10.ModuleInterface
{
    /// <summary>
    /// Модуль.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        object QueryView(Type viewType);

        /// <summary>
        /// Модуль готов к использованию.
        /// </summary>
        bool IsModuleReady { get; }
    }
}