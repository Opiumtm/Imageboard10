using System;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Ошибка работы с хранилищем бинарных данных.
    /// </summary>
    public class BlobException : Exception
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        public BlobException(string message)
            :base(message)
        {            
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="innerException">Внутренняя ошибка.</param>
        public BlobException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}