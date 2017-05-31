using System;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Modules;
using static Imageboard10.Core.Models.SerializationImplHelper;


namespace Imageboard10.Core.Models.Links
{
    /// <summary>
    /// Сервис сериализации ссылок.
    /// </summary>
    public class LinkSerializationService : ModuleBase<ILinkSerializationService>, ILinkSerializationService
    {
        /// <summary>
        /// Сериализовать ссылку.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Ссылка в виде строки.</returns>
        public string Serialize(ILink link)
        {
            if (link == null)
            {
                return null;
            }
            var serializer = ModuleProvider?.QueryModule<ILinkSerializer, Type>(link.GetTypeForSerializer());
            if (serializer == null)
            {
                throw new ModuleNotFoundException($"Не найдена логика сериализации для ссылки типа {link.GetTypeForSerializer()?.FullName}");
            }
            return WithTypeId(serializer.Serialize(link), serializer.LinkTypeId);
        }

        /// <summary>
        /// Десериализовать ссылку.
        /// </summary>
        /// <param name="linkStr">Ссылка в виде строки.</param>
        /// <returns>Ссыока.</returns>
        public ILink Deserialize(string linkStr)
        {
            if (linkStr == null)
            {
                return null;
            }
            (var data, var typeId) = ExtractTypeId(linkStr);
            var serializer = ModuleProvider?.QueryModule<ILinkSerializer, string>(typeId);
            if (serializer == null)
            {
                throw new ModuleNotFoundException($"Не найдена логика сериализации для ссылки типа \"{typeId}\"");
            }
            return serializer.Deserialize(data);
        }
    }
}