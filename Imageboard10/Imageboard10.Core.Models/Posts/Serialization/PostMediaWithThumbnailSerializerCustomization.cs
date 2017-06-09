using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Posts.PostMedia;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Кастомизация сериализации объекта <see cref="PostMediaWithThumbnail"/>.
    /// </summary>
    public class PostMediaWithThumbnailSerializerCustomization<T> : PostMediaSerializerCustomization<T>
        where T : PostMediaWithThumbnail, new()
    {
        /// <summary>
        /// Проверить контракт перед сериализацией.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override T ValidateContract(T obj)
        {
            obj = base.ValidateContract(obj);
            if (obj != null)
            {
                obj.ThumbnailContract = ModuleProvider.ValidateBeforeSerialize<IPostMediaWithSize, PostMediaBase, PostMediaExternalContract>(obj.Thumbnail);
            }
            return obj;
        }

        /// <summary>
        /// Проверить контракт после сериализации.
        /// </summary>
        /// <param name="obj">Исходный объект.</param>
        /// <returns>Проверенный объект.</returns>
        public override T ValidateAfterDeserialize(T obj)
        {
            obj = base.ValidateAfterDeserialize(obj);
            if (obj != null)
            {
                obj.Thumbnail = ModuleProvider.ValidateAfterDeserialize<PostMediaBase, IPostMediaWithSize, PostMediaExternalContract>(obj.ThumbnailContract);
                obj.ThumbnailContract = null;
            }
            return obj;
        }
    }
}