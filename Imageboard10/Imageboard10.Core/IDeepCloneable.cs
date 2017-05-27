using Imageboard10.Core.Modules;

namespace Imageboard10.Core
{
    /// <summary>
    /// Объект, поддерживающий копирование по значению.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    public interface IDeepCloneable<out T>
    {
        /// <summary>
        /// Клонировать.
        /// </summary>
        /// <param name="modules">Модули.</param>
        /// <returns>Клон.</returns>
        T DeepClone(IModuleProvider modules);
    }
}