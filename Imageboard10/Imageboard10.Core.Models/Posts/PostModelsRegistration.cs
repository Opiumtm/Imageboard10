using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Posts.Serialization;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// Регистрация модулей обработки моделей постов.
    /// </summary>
    public static class PostModelsRegistration
    {
        /// <summary>
        /// Регистрировать модули.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        public static void RegisterModules(IModuleCollection collection)
        {
            collection.RegisterModule<PostMediaSerializationService, IPostMediaSerializationService>();
            collection.RegisterProvider(typeof(IPostMediaSerializer), new StandardPostMediaSerializers());
        }
    }
}