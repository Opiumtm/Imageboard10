namespace Imageboard10.ModuleInterface
{
    /// <summary>
    /// Прогресс операции.
    /// </summary>
    public struct OperationProgress
    {
        /// <summary>
        /// Прогресс от 0 до 1.
        /// </summary>
        public double? Progress;

        /// <summary>
        /// Сообщение.
        /// </summary>
        public string Message;

        /// <summary>
        /// Идентификатор операции.
        /// </summary>
        public string OperationId;
    }
}