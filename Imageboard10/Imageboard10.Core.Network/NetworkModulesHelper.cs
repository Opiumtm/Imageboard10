﻿using System;
using System.Threading.Tasks;
using Imageboard10.Core.Modules;
using Imageboard10.Core.NetworkInterface;

namespace Imageboard10.Core.Network
{
    /// <summary>
    /// Класс-помощник для поиска модулей.
    /// </summary>
    public static class NetworkModulesHelper
    {
        /// <summary>
        /// Найти парсер сетевого Dto.
        /// </summary>
        /// <typeparam name="TIn">Тип входных данных.</typeparam>
        /// <typeparam name="TOut">Тип результата.</typeparam>
        /// <param name="provider">Провайдер модулей.</param>
        /// <returns>Парсер.</returns>
        public static INetworkDtoParser<TIn, TOut> FindNetworkDtoParser<TIn, TOut>(this IModuleProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            var parserType = typeof(INetworkDtoParser<TIn, TOut>);
            var parsers = provider.QueryModule<INetworkDtoParsers, Type>(parserType);
            return parsers as INetworkDtoParser<TIn, TOut> ?? parsers?.QueryView<INetworkDtoParser<TIn, TOut>>();
        }

        /// <summary>
        /// Найти парсер сетевого Dto.
        /// </summary>
        /// <typeparam name="TIn">Тип входных данных.</typeparam>
        /// <typeparam name="TOut">Тип результата.</typeparam>
        /// <param name="provider">Провайдер модулей.</param>
        /// <returns>Парсер.</returns>
        public static async ValueTask<INetworkDtoParser<TIn, TOut>> FindNetworkDtoParserAsync<TIn, TOut>(this IModuleProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            var parserType = typeof(INetworkDtoParser<TIn, TOut>);
            var parsers = await provider.QueryModuleAsync<INetworkDtoParsers, Type>(parserType);
            return parsers as INetworkDtoParser<TIn, TOut> ?? parsers?.QueryView<INetworkDtoParser<TIn, TOut>>();
        }

        /// <summary>
        /// Запросить операцию сетевого движка.
        /// </summary>
        /// <typeparam name="TIntf">Интерфейс операции.</typeparam>
        /// <param name="provider">Провайдер.</param>
        /// <param name="engineId">Движок.</param>
        /// <returns>Операция (если найдена).</returns>
        public static TIntf QueryEngineCapability<TIntf>(this IModuleProvider provider, string engineId)
            where TIntf : class , INetworkEngineCapability
        {
            return provider.QueryModule<TIntf, EngineCapabilityQuery>(new EngineCapabilityQuery() {EngineId = engineId});
        }

        /// <summary>
        /// Запросить операцию сетевого движка.
        /// </summary>
        /// <typeparam name="TIntf">Интерфейс операции.</typeparam>
        /// <param name="provider">Провайдер.</param>
        /// <param name="engineId">Движок.</param>
        /// <returns>Операция (если найдена).</returns>
        public static ValueTask<TIntf> QueryEngineCapabilityAsync<TIntf>(this IModuleProvider provider, string engineId)
            where TIntf : class, INetworkEngineCapability
        {
            return provider.QueryModuleAsync<TIntf, EngineCapabilityQuery>(new EngineCapabilityQuery() { EngineId = engineId });
        }
    }
}