using System;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Действие по обновлению флага.
    /// </summary>
    public interface IFlagUpdateAction
    {
        /// <summary>
        /// Действие.
        /// </summary>
        FlagUpdateAction Action { get; }

        /// <summary>
        /// Флаг.
        /// </summary>
        Guid Flag { get; }
    }


    /// <summary>
    /// Действие по обновлению флага.
    /// </summary>
    public enum FlagUpdateAction
    {
        /// <summary>
        /// Добавить.
        /// </summary>
        Add,
        /// <summary>
        /// Удалить.
        /// </summary>
        Remove,
        /// <summary>
        /// Сбросить все.
        /// </summary>
        Clear
    }
}