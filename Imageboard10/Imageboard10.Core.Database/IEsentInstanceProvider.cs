﻿using System.Threading.Tasks;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Провайдер экземпляров ESENT.
    /// </summary>
    public interface IEsentInstanceProvider
    {
        /// <summary>
        /// Основная сессия. Не вызывать Dispose(), т.к. временем жизни основной сессии управляет провайдер.
        /// </summary>
        IEsentSession MainSession { get; }

        /// <summary>
        /// Получить сессию только для чтения.
        /// </summary>
        /// <returns>Экземпляр.</returns>
        Task<IEsentSession> CreateReadOnlySession();

        /// <summary>
        /// Получить сессию только для чтения, вызовы к которой строго должны производиться из одного потока.
        /// </summary>
        /// <returns></returns>
        IEsentSession CreateThreadUnsafeReadOnlySession();

        /// <summary>
        /// Путь к базе данных.
        /// </summary>
        string DatabasePath { get; }
    }
}