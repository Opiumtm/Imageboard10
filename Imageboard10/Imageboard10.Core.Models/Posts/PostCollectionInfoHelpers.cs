using System;
using System.Linq;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Помощники для получения информации о коллекции посто.
    /// </summary>
    public static class PostCollectionInfoHelpers
    {
        /// <summary>
        /// Получить информацию.
        /// </summary>
        /// <typeparam name="T">Тип информационного интерфейса.</typeparam>
        /// <param name="infoSet">Набор информации.</param>
        /// <returns>Инофрмация.</returns>
        public static T GetCollectionInfo<T>(this IBoardPostCollectionInfoSet infoSet)
            where T : class, IBoardPostCollectionInfo
        {
            if (infoSet?.Items == null)
            {
                return null;
            }
            foreach (var item in infoSet.Items)
            {
                foreach (var it in item.GetInfoInterfaceTypes() ?? Enumerable.Empty<Type>())
                {
                    if (it == typeof(T) && item is T i)
                    {
                        return i;
                    }
                }
            }
            return null;
        }
    }
}