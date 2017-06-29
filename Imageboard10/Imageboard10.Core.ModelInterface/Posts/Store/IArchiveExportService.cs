using Windows.Foundation;
using Windows.Storage;

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
        /// <returns>Прогресс (0.0..1.0).</returns>
        IAsyncActionWithProgress<double> Export(IArchiveCollection collection, StorageFolder folder, string fileName);

        /// <summary>
        /// Импортировать архив.
        /// </summary>
        /// <param name="file">Файл архива.</param>
        /// <returns>Прогресс (0.0..1.0)</returns>
        IAsyncOperationWithProgress<IArchiveCollection, double> Import(StorageFile file);
    }
}