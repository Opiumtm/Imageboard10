namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Ссылка с движком.
    /// </summary>
    public interface IEngineLink : ILink
    {
        /// <summary>
        /// Движок.
        /// </summary>
        string Engine { get; }

        /// <summary>
        /// Получить корневую ссылку.
        /// </summary>
        /// <returns>Корневая ссылка.</returns>
        ILink GetRootLink();
    }
}