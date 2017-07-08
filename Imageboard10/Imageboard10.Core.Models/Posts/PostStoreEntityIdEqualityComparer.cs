using System.Collections.Generic;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Средство сравнения идентификаторов сущностей.
    /// </summary>
    public class PostStoreEntityIdEqualityComparer : IEqualityComparer<PostStoreEntityId>
    {
        /// <summary>
        /// Экземпляр.
        /// </summary>
        public static readonly IEqualityComparer<PostStoreEntityId> Instance = new PostStoreEntityIdEqualityComparer();

        /// <summary>Determines whether the specified objects are equal.</summary>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        public bool Equals(PostStoreEntityId x, PostStoreEntityId y)
        {
            return x.Id == y.Id;
        }

        /// <summary>Returns a hash code for the specified object.</summary>
        /// <returns>A hash code for the specified object.</returns>
        /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.</exception>
        public int GetHashCode(PostStoreEntityId obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}