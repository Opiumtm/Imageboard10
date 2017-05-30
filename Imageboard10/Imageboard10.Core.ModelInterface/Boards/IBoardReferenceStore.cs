using System.Collections.Generic;
using Windows.Foundation;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Boards
{
    /// <summary>
    /// Хранилище ссылок на доски.
    /// </summary>
    public interface IBoardReferenceStore
    {
        /// <summary>
        /// Получить количество досок.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Количество досок.</returns>
        IAsyncOperation<int> GetCount(BoardReferenceStoreQuery query);

        /// <summary>
        /// Получить количество категорий.
        /// </summary>
        /// <returns>Количество категорий.</returns>
        IAsyncOperation<int> GetCategoryCount();

        /// <summary>
        /// Получить количество категорий.
        /// </summary>
        /// <param name="isAdult">Только для взрослых. null = не имеет значения.</param>
        /// <returns>Количество категорий.</returns>
        IAsyncOperation<int> GetCategoryCount(bool? isAdult);

        /// <summary>
        /// Получить все ссылки на доски.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Ссылки на доски.</returns>
        IAsyncOperation<IList<ILink>> GetBoardLinks(BoardReferenceStoreQuery query);

        /// <summary>
        /// Получить все категории.
        /// </summary>
        /// <returns>Все категории.</returns>
        IAsyncOperation<IList<string>> GetAllCategories();

        /// <summary>
        /// Получить все категории.
        /// </summary>
        /// <param name="isAdult">Только для взрослых. null = не имеет значения.</param>
        /// <returns>Все категории.</returns>
        IAsyncOperation<IList<string>> GetAllCategories(bool? isAdult);

        /// <summary>
        /// Загрузить ссылку на доску.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Ссылка на доску.</returns>
        IAsyncOperation<IBoardReference> LoadReference(ILink link);

        /// <summary>
        /// Загрузить ссылки.
        /// </summary>
        /// <param name="start">Начало.</param>
        /// <param name="count">Количество.</param>
        /// <param name="query">Запрос.</param>
        /// <returns>Ссылки.</returns>
        IAsyncOperation<IList<IBoardShortInfo>> LoadShortReferences(int start, int count, BoardReferenceStoreQuery query);

        /// <summary>
        /// Загрузить ссылки.
        /// </summary>
        /// <param name="links">Список ссылок.</param>
        /// <returns>Ссылки на доски.</returns>
        IAsyncOperation<IList<IBoardShortInfo>> LoadShortReferences(IList<ILink> links);

        /// <summary>
        /// Очистить всю информацию.
        /// </summary>
        IAsyncAction Clear();

        /// <summary>
        /// Обновить ссылку.
        /// </summary>
        /// <param name="reference">Ссылка.</param>
        IAsyncAction UpdateReference(IBoardReference reference);

        /// <summary>
        /// Обновить ссылки.
        /// </summary>
        /// <param name="references">Ссылки.</param>
        /// <param name="clearPrevious">Очистить предыдущие.</param>
        IAsyncAction UpdateReferences(IList<IBoardReference> references, bool clearPrevious);
    }
}