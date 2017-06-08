using Imageboard10.Core.ModelInterface;
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
            collection.RegisterProvider(typeof(IObjectSerializer), new StandardPostMediaSerializers());
            collection.RegisterProvider(typeof(IObjectSerializer), new StandardPostAttributeSerializers());
        }
    }
}