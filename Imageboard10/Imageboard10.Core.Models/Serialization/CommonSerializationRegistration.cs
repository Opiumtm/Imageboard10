using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Serialization
{
    /// <summary>
    /// Регистрация общих модулей сериализации.
    /// </summary>
    public static class CommonSerializationRegistration
    {
        /// <summary>
        /// Регистрировать модули.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        public static void RegisterModules(IModuleCollection collection)
        {
            collection.RegisterModule<ObjectSerializationService, IObjectSerializationService>();
        }
    }
}