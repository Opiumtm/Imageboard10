using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Хранилище бинарных данных.
    /// </summary>
    public interface IBlobsModelStore
    {
        /// <summary>
        /// Сохранить файл.
        /// </summary>
        /// <param name="blob">Файл.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>GUID файла.</returns>
        Task<BlobId> SaveBlob(InputBlob blob, CancellationToken token);

        /// <summary>
        /// Получить GUID файла.
        /// </summary>
        /// <param name="uniqueName">Имя файла.</param>
        /// <returns>GUID файла.</returns>
        Task<BlobId?> FindBlob(string uniqueName);

        /// <summary>
        /// Загрузить файл.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Результат.</returns>
        Task<Stream> LoadBlob(BlobId id);

        /// <summary>
        /// Удалить файл.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>true, если файл найден и удалён. false, если нет такого файла или файл заблокирован на удаление.</returns>
        Task<bool> DeleteBlob(BlobId id);

        /// <summary>
        /// Удалить блобы.
        /// </summary>
        /// <param name="idArray">Массив идентификаторов.</param>
        /// <returns>Массив идентификаторов тех файлов, которые получилось удалить.</returns>
        Task<BlobId[]> DeleteBlobs(BlobId[] idArray);

        /// <summary>
        /// Получить размер файла.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Информация о файле.</returns>
        Task<BlobInfo?> GetBlobInfo(BlobId id);

        /// <summary>
        /// Проверка, заблокирован ли файл.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Результат.</returns>
        Task<bool?> IsLocked(BlobId id);

        /// <summary>
        /// Читать категорию.
        /// </summary>
        /// <param name="category">Категория.</param>
        /// <returns>Результат.</returns>
        Task<BlobInfo[]> ReadCategory(string category);

        /// <summary>
        /// Получить размер категории.
        /// </summary>
        /// <param name="category">Категория.</param>
        /// <returns>Размер категории.</returns>
        Task<long> GetCategorySize(string category);
    }
}