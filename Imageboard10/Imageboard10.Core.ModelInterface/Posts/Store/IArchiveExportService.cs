using Windows.Foundation;
using Windows.Storage;
using Imageboard10.ModuleInterface;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Сервис экспорта и импорта архивов.
    /// </summary>
    public interface IArchiveExportService
    {
        /// <summary>
        /// Экспортировать архив. 
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        /// <param name="folder">Директория.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <returns>Прогресс.</returns>
        IAsyncActionWithProgress<OperationProgress> Export(IArchiveCollection collection, StorageFolder folder, string fileName);

        /// <summary>
        /// Импортировать архив.
        /// </summary>
        /// <param name="file">Файл архива.</param>
        /// <returns>Коллекция.</returns>
        IAsyncOperationWithProgress<IArchiveCollection, OperationProgress> Import(StorageFile file);
    }
}