using System.Collections.Generic;
using Windows.Storage;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Архив.
    /// </summary>
    public interface IArchiveCollection : IBoardPostCollection
    {
        /// <summary>
        /// Файлы архива (ключ - строковое представление ссылки на медиа).
        /// </summary>
        IDictionary<string, StorageFile> ArchiveFiles { get; }
    }
}