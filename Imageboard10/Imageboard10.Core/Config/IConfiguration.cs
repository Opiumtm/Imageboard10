using System;
using System.Threading.Tasks;
using Imageboard10.Core.Tasks;

namespace Imageboard10.Core.Config
{
    /// <summary>
    /// Конфигурация.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Сохранить конфигурацию.
        /// </summary>
        ValueTask<Nothing> Save();

        /// <summary>
        /// Конфигурация сохранена.
        /// </summary>
        AsyncLanguageEvent<EventArgs> Saved { get; }
    }
}