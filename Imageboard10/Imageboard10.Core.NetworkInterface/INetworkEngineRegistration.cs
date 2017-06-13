using System.Collections.Generic;

namespace Imageboard10.Core.NetworkInterface
{
    /// <summary>
    /// Регистрация сетевых движков.
    /// </summary>
    public interface INetworkEngineRegistration
    {
        /// <summary>
        /// Регистрировать движок.
        /// </summary>
        /// <param name="engineId">Идентификатор движка.</param>
        void RegisterEngine(string engineId);

        /// <summary>
        /// Получить доступные движки.
        /// </summary>
        /// <returns>Доступные движки.</returns>
        IList<string> GetAvailableEngines();
    }
}