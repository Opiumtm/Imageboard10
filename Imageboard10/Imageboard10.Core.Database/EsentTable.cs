using System;
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