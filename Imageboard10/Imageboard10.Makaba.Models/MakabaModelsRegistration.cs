using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Modules;
using Imageboard10.Makaba.Models.Posts.Serialization;

namespace Imageboard10.Makaba.Models
{
    /// <summary>
    /// Регистрация моделей Makaba.
    /// </summary>
    public static class MakabaModelsRegistration
    {
        /// <summary>
        /// Регистрировать модули.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        public static void RegisterModules(IModuleCollection collection)
        {
            collection.RegisterProvider(typeof(IObjectSerializer), new MakabaModelsSerializers());
        }
    }
}