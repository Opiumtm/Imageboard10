using System;
using System.Threading.Tasks;

namespace Imageboard10.Core.Tasks
{
    /// <summary>
    /// Асинхронный обработчик событий.
    /// </summary>
    /// <typeparam name="T">Тип события.</typeparam>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Событие.</param>
    /// <returns></returns>
    public delegate Task AsyncEventHandler<in T>(object sender, T e) where T : EventArgs;
}