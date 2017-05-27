using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Links;

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
        void RegisterEngine(ILink engineId);

        /// <summary>
        /// Получить доступные движки.
        /// </summary>
        /// <returns>Доступные движки.</returns>
        IList<ILink> GetAvailableEngines();
    }
}