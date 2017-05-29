using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Models.Links.Serialization;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Регистрация модулей обработки ссылок.
    /// </summary>
    public static class LinkModelsRegistration
    {
        /// <summary>
        /// Регистрировать модули.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        public static void RegisterModules(IModuleCollection collection)
        {
            collection.RegisterModule<LinkSerializationService, ILinkSerializationService>();
            collection.RegisterProvider(typeof(ILinkSerializer), new StandardLinkSerializers());
        }
    }
}