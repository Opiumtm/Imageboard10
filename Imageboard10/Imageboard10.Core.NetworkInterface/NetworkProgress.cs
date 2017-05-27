namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Прогресс сетевой операции.
    /// </summary>
    public struct NetworkProgress
    {
        /// <summary>
        /// Сообщение.
        /// </summary>
        public string Message;

        /// <summary>
        /// Процент выполнения.
        /// </summary>
        public double? Percent;

        /// <summary>
        /// Идентификатор операции.
        /// </summary>
        public string OperationId;
    }
}