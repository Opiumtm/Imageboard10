using System;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Экземпляр ESENT.
    /// </summary>
    public interface IEsentInstance : IDisposable
    {
        /// <summary>
        /// Экземпляр.
        /// </summary>
        Instance Instance { get; }
    }
}