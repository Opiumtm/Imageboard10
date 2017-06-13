using System;
using Imageboard10.Core.Modules;
using Imageboard10.Core.NetworkInterface;

namespace Imageboard10.Makaba
{
    /// <summary>
    /// Базовый модуль makaba.
    /// </summary>
    /// <typeparam name="TIntf">Тип интерфейса.</typeparam>
    public abstract class MakabaEngineModuleBase<TIntf> : ModuleBase<TIntf>, IStaticModuleQueryFilter, INetworkEngineCapability
        where TIntf : class, INetworkEngineCapability
    {
        /// <summary>
        /// Проверить запрос.
        /// </summary>
        /// <typeparam name="T">Тип запроса.</typeparam>
        /// <param name="query">Запрос.</param>
        /// <returns>Результат.</returns>
        protected virtual bool DoCheckQuery<T>(T query)
        {
            if (typeof(T) == typeof(EngineCapabilityQuery))
            {
                var c = (EngineCapabilityQuery) (object)query;
                if (MakabaConstants.MakabaEngineId.Equals(c.EngineId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Проверить запрос.
        /// </summary>
        /// <typeparam name="T">Тип запроса.</typeparam>
        /// <param name="query">Запрос.</param>
        /// <returns>Результат.</returns>
        bool IStaticModuleQueryFilter.CheckQuery<T>(T query)
        {
            return DoCheckQuery(query);
        }

        /// <summary>
        /// Идентификатор движка.
        /// </summary>
        public string EngineId => MakabaConstants.MakabaEngineId;
    }
}