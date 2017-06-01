namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Файл не найден в хранилище бинарных данных.
    /// </summary>
    public class BlobNotFoundException : BlobException
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="blobId">Идентификатор файла.</param>
        public BlobNotFoundException(BlobId blobId)
            :base($"Файл не найден в базе данных, blobId = {blobId.Id}")
        {            
        }
    }
}