using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Models.Posts;
using Imageboard10.Core.Models.Serialization;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models
{
    /// <summary>
    /// Регистрация модулей для моделей.
    /// </summary>
    public static class ModelsRegistration
    {
        /// <summary>
        /// Регистрировать модули.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        public static void RegisterModules(IModuleCollection collection)
        {
            CommonSerializationRegistration.RegisterModules(collection);
            LinkModelsRegistration.RegisterModules(collection);
            PostModelsRegistration.RegisterModules(collection);
        }
    }
}