using System;
using System.Collections.Generic;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Политика удаления старых данных.
    /// </summary>
    public interface IPostStoreStaleDataClearPolicy
    {
        /// <summary>
        /// Максимальное время нахождение к базе, в секундах.
        /// Если null, то не проверять максимальное время нахождения.
        /// </summary>
        double? MaxAgeSec { get; }

        /// <summary>
        /// Максимальное время с последнего доступа, в секундах.
        /// Если null, то не проверять максимальное время с последнего доступа.
        /// </summary>
        double? MaxAccessAgeSec { get; }

        /// <summary>
        /// Максимальный размер лога доступа по типам сущностей (только для независимых сущностей, т.е. не имеющих родительской сущности).
        /// </summary>
        IDictionary<PostStoreEntityType, int> MaxAccessLogSize { get; }

        /// <summary>
        /// Минимальное время (в секундах) между запуском очистки. Если прошлый запуск не был ранее, чем указанное количество секунд, то операция не производится.
        /// Если null, то произвести очистку в любом случае.
        /// </summary>
        double? MinCleanupPeriod { get; }

        /// <summary>
        /// Не очищать находящееся в "избранном".
        /// </summary>
        bool DontCleanFavorites { get; }
    }
}