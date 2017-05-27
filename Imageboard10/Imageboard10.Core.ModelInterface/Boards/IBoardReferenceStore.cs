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
        /// <param name="category">Категория (null - по всем категориям).</param>
        /// <returns>Количество досок.</returns>
        IAsyncOperation<int> GetCount(string category);

        /// <summary>
        /// Получить количество категорий.
        /// </summary>
        /// <returns>Количество категорий.</returns>
        IAsyncOperation<int> GetCategoryCount();

        /// <summary>
        /// Получить все ссылки на доски.
        /// </summary>
        /// <param name="category">Категория (null - по всем категориям).</param>
        /// <returns>Ссылки на доски.</returns>
        IAsyncOperation<IList<ILink>> GetBoardLiks(string category);

        /// <summary>
        /// Получить все категории.
        /// </summary>
        /// <returns>Все категории.</returns>
        IAsyncOperation<IList<string>> GetAllCategories();

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
        /// <param name="category">Категория.</param>
        /// <returns>Ссылки.</returns>
        IAsyncOperation<IList<IBoardReference>> LoadReferences(int start, int count, string category);

        /// <summary>
        /// Загрузить ссылки.
        /// </summary>
        /// <param name="links">Список ссылок.</param>
        /// <returns>Ссылки на доски.</returns>
        IAsyncOperation<IList<IBoardReference>> LoadReferences(IList<ILink> links);

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