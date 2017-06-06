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
        /// Читать категорию.
        /// </summary>
        /// <param name="category">Категория.</param>
        /// <returns>Результат.</returns>
        Task<BlobInfo[]> ReadCategory(string category);

        /// <summary>
        /// Читать все файлы по ссылке.
        /// </summary>
        /// <param name="referenceId">Идентификатор ссылки.</param>
        /// <returns>Результат.</returns>
        Task<BlobInfo[]> ReadReferencedBlobs(Guid referenceId);

        /// <summary>
        /// Получить размер категории.
        /// </summary>
        /// <param name="category">Категория.</param>
        /// <returns>Размер категории.</returns>
        Task<long> GetCategorySize(string category);

        /// <summary>
        /// Получить количество файлов в категории.
        /// </summary>
        /// <param name="category">Категория.</param>
        /// <returns>Количество файлов.</returns>
        Task<int> GetCategoryBlobsCount(string category);

        /// <summary>
        /// Получить размер элементов со ссылкой.
        /// </summary>
        /// <param name="referenceId">Идентификатор ссылки.</param>
        /// <returns>Размер категории.</returns>
        Task<long> GetReferencedSize(Guid referenceId);

        /// <summary>
        /// Получить количество файлов со ссылкой.
        /// </summary>
        /// <param name="referenceId">Идентификатор ссылки.</param>
        /// <returns>Количество файлов.</returns>
        Task<int> GetReferencedBlobsCount(Guid referenceId);

        /// <summary>
        /// Добавить постоянную ссылку.
        /// </summary>
        /// <param name="referenceId">Идентификатор ссылки.</param>
        Task AddPermanentReference(Guid referenceId);

        /// <summary>
        /// Удалить постоянную ссылку.
        /// </summary>
        /// <param name="referenceId">Идентификатор ссылки.</param>
        Task RemovePermanentReference(Guid referenceId);

        /// <summary>
        /// Проверить являются ли ссылки постояными.
        /// </summary>
        /// <param name="references">Массив ссылок.</param>
        /// <returns>Массив постоянных ссылок.</returns>
        Task<Guid[]> CheckIfReferencesPermanent(Guid[] references);

        /// <summary>
        /// Удалить все файлы.
        /// </summary>
        Task DeleteAllBlobs();

        /// <summary>
        /// Удалить все ссылки.
        /// </summary>
        Task DeleteAllReferences();

        /// <summary>
        /// Удалить все не завершённые файлы.
        /// </summary>
        Task DeleteAllUncompletedBlobs();

        /// <summary>
        /// Получить количество всех файлов.
        /// </summary>
        /// <returns>Все файлы.</returns>
        Task<int> GetBlobsCount();

        /// <summary>
        /// Получить общий размер.
        /// </summary>
        /// <returns></returns>
        Task<long> GetTotalSize();

        /// <summary>
        /// Получить количество всех незавершённых файлов.
        /// </summary>
        /// <returns>Все файлы.</returns>
        Task<int> GetUncompletedBlobsCount();

        /// <summary>
        /// Получить общий размер незавершённых файлов.
        /// </summary>
        /// <returns></returns>
        Task<long> GetUncompletedTotalSize();

        /// <summary>
        /// Найти незавершённые файлы.
        /// </summary>
        /// <returns>Список файлов.</returns>
        Task<BlobId[]> FindUncompletedBlobs();

        /// <summary>
        /// Для юнит-тестов. Пометить файл как незавершённый.
        /// </summary>
        /// <param name="id">Идентификатор файла.</param>
        /// <returns>true, если файл найден и помечен.</returns>
        Task<bool> MarkUncompleted(BlobId id);

        /// <summary>
        /// Для юнит тестов. Проверка на наличие Файла.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Результат.</returns>
        Task<bool> IsFilePresent(BlobId id);

        /// <summary>
        /// Для юнит-тестов.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Получить путь к временному файлу.</returns>
        string GetTempFilePath(BlobId id);
    }
}