using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Таблица ESENT.
    /// </summary>
    public struct EsentTable : IDisposable
    {
        /// <summary>
        /// Сессия.
        /// </summary>
        public readonly Session Session;

        /// <summary>
        /// Таблица.
        /// </summary>
        public readonly JET_TABLEID Table;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="session">Сессия.</param>
        /// <param name="table">Таблица.</param>
        public EsentTable(Session session, JET_TABLEID table)
        {
            Session = session;
            Table = table;
        }

        /// <summary>
        /// Получить идентификатор столбца.
        /// </summary>
        /// <param name="columnName">Имя столбца.</param>
        /// <returns>Идентификатор.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JET_COLUMNID GetColumnid(string columnName)
        {
            return Api.GetTableColumnid(Session, Table, columnName);
        }

        /// <summary>
        /// Получить словарь столбцов.
        /// </summary>
        /// <returns>Словарь столбцов.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDictionary<string, JET_COLUMNID> GetColumnDictionary()
        {
            return Api.GetColumnDictionary(Session, Table);
        }

        /// <summary>
        /// Создать обновление.
        /// </summary>
        /// <param name="prep">Тип обновления.</param>
        /// <returns>Обновление.</returns>
        public Update Update(JET_prep prep)
        {
            return new Update(Session, Table, prep);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Api.JetCloseTable(Session, Table);
        }

        public static implicit operator JET_TABLEID(EsentTable src)
        {
            return src.Table;
        }
    }
}