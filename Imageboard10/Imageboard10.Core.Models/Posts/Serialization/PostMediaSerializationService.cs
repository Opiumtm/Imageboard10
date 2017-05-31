using System;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Modules;

using static Imageboard10.Core.Models.SerializationImplHelper;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Сервис сериализации медия в постах.
    /// </summary>
    public class PostMediaSerializationService : ModuleBase<IPostMediaSerializationService>, IPostMediaSerializationService
    {
        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="media">Медиа.</param>
        /// <returns>Сериализованное медиа.</returns>
        public string SerializeToString(IPostMedia media)
        {
            if (media == null)
            {
                return null;
            }
            var serializer = ModuleProvider.QueryModule<IPostMediaSerializer, Type>(media.GetTypeForSerializer()) 
                ?? throw new ModuleNotFoundException($"Не найдена логика сериализации медиа поста типа {media.GetTypeForSerializer()?.FullName}");
            return WithTypeId(serializer.SerializeToString(media), serializer.TypeId);
        }

        /// <summary>
        /// Сериализовать.
        /// </summary>
        /// <param name="media">Медиа.</param>
        /// <returns>Сериализованное медиа.</returns>
        public byte[] SerializeToBytes(IPostMedia media)
        {
            if (media == null)
            {
                return null;
            }
            var serializer = ModuleProvider.QueryModule<IPostMediaSerializer, Type>(media.GetTypeForSerializer())
                             ?? throw new ModuleNotFoundException($"Не найдена логика сериализации медиа поста типа {media.GetTypeForSerializer()?.FullName}");
            return WithTypeId(serializer.SerializeToBytes(media), serializer.TypeId);
        }

        /// <summary>
        /// Десериализовать.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Медиа поста.</returns>
        public IPostMedia Deserialize(string data)
        {
            if (data == null)
            {
                return null;
            }
            (var sdata, var typeId) = ExtractTypeId(data);
            var serializer = ModuleProvider.QueryModule<IPostMediaSerializer, string>(typeId)
                             ?? throw new ModuleNotFoundException($"Не найдена логика сериализации медиа поста типа \"{typeId}\"");
            return serializer.Deserialize(sdata);
        }

        /// <summary>
        /// Десериализовать.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Медиа поста.</returns>
        public IPostMedia Deserialize(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            (var sdata, var typeId) = ExtractTypeId(data);
            var serializer = ModuleProvider.QueryModule<IPostMediaSerializer, string>(typeId)
                             ?? throw new ModuleNotFoundException($"Не найдена логика сериализации медиа поста типа \"{typeId}\"");
            return serializer.Deserialize(sdata);
        }
    }
}