using System;
using System.Collections.Generic;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Политика удаления старых данных.
    /// </summary>
    public sealed class PostStoreStaleDataClearPolicy
    {
        /// <summary>
        /// Максимальное время нахождение к базе, в секундах.
        /// Если null, то не проверять максимальное время нахождения.
        /// </summary>
        public double? MaxAgeSec { get; set; }

        /// <summary>
        /// Максимальное время с последнего доступа, в секундах.
        /// Если null, то не проверять максимальное время с последнего доступа.
        /// </summary>
        public double? MaxAccessAgeSec { get; set; }

        /// <summary>
        /// Минимальное время (в секундах) между запуском очистки. Если прошлый запуск не был ранее, чем указанное количество секунд, то операция не производится.
        /// Если null, то произвести очистку в любом случае.
        /// </summary>
        public double? MinCleanupPeriod { get; set; }

        /// <summary>
        /// Обратный вызов.
        /// </summary>
        public event PostStoreStaleDataClearFinishedCallback Callback;

        /// <summary>
        /// Вызвать обратный вызов.
        /// </summary>
        /// <param name="ex">Ошибка.</param>
        public void TriggerCallback(Exception ex)
        {
            Callback?.Invoke(ex);
        }
    }

    /// <summary>
    /// Обратный вызов.
    /// </summary>
    /// <param name="error">null, если операция завершена успешно.</param>
    public delegate void PostStoreStaleDataClearFinishedCallback(Exception error);
}