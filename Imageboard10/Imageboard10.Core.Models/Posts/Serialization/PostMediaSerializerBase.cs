using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Posts.PostMedia;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Utility;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Средство сериализации.
    /// </summary>
    /// <typeparam name="T">Тип медиа-контракта.</typeparam>
    public abstract class PostMediaSerializerBase<T> : ModuleBase<IPostMediaSerializer>, IPostMediaSerializer
        where T : PostMediaBase
    {
        /// <summary>
        /// Идентификатор типа.
        /// </summary>
        public abstract string TypeId { get; }

        /// <summary>
        /// Тип.
        /// </summary>
        public Type Type => typeof(T);

        /// <summary>
        /// Сериализаци ссылок.
        /// </summary>
        protected ILinkSerializationService LinkSerialization { get; private set; }

        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            LinkSerialization = await moduleProvider.QueryModuleAsync<ILinkSerializationService>() ?? throw new ModuleNotFoundException(typeof(ILinkSerializationService));
            return Nothing.Value;
        }

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
            var serializer = DataContractSerializerCache.GetJsonSerializer<PostMediaBase>();
            using (var str = new MemoryStream())
            {
                serializer.WriteObject(str, ValidateContract(media));
                return Encoding.UTF8.GetString(str.ToArray());
            }
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
            var serializer = DataContractSerializerCache.GetSerializer<PostMediaBase>();
            using (var str = new MemoryStream())
            {
                using (var wr = XmlDictionaryWriter.CreateBinaryWriter(str))
                {
                    serializer.WriteObject(wr, ValidateContract(media));
                    wr.Flush();
                }
                return str.ToArray();
            }
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
            var serializer = DataContractSerializerCache.GetJsonSerializer<PostMediaBase>();
            using (var str = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                return ValidateAfterDeserialize(serializer.ReadObject(str) as PostMediaBase);
            }
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
            var serializer = DataContractSerializerCache.GetSerializer<PostMediaBase>();
            using (var str = new MemoryStream(data))
            {
                using (var rd = XmlDictionaryReader.CreateBinaryReader(str, XmlDictionaryReaderQuotas.Max))
                {
                    return ValidateAfterDeserialize(serializer.ReadObject(rd) as PostMediaBase);
                }
            }
        }

        private PostMediaBase ValidateContract(IPostMedia media)
        {
            if (media == null)
            {
                return null;
            }
            if (media is PostMediaWithThumbnail p)
            {
                p.MediaLinkContract = p.MediaLink != null ? LinkSerialization.Serialize(p.MediaLink) : null;
                p.ThumbnailContract = ValidateContract(p.Thumbnail as PostMedia.PostMedia) ?? SerializeToExternalContract(p.Thumbnail);
                return p;
            }
            if (media is PostMedia.PostMedia pm)
            {
                pm.MediaLinkContract = pm.MediaLink != null ? LinkSerialization.Serialize(pm.MediaLink) : null;
                return pm;
            }
            if (media is PostMediaBase pb)
            {
                return pb;
            }
            return SerializeToExternalContract(media);
        }

        private IPostMedia ValidateAfterDeserialize(PostMediaBase media)
        {
            if (media == null)
            {
                return null;
            }
            if (media is PostMediaExternalContract e)
            {
                return DeserializeExternalContract(e);
            }
            if (media is PostMediaWithThumbnail t)
            {
                t.MediaLink = t.MediaLinkContract != null ? LinkSerialization.Deserialize(t.MediaLinkContract) : null;
                t.Thumbnail = ValidateAfterDeserialize(t.ThumbnailContract) as IPostMediaWithSize;
                t.MediaLinkContract = null;
                t.ThumbnailContract = null;
                return t;
            }
            if (media is PostMedia.PostMedia pm)
            {
                pm.MediaLink = pm.MediaLinkContract != null ? LinkSerialization.Deserialize(pm.MediaLinkContract) : null;
                pm.MediaLinkContract = null;
                return pm;
            }
            return media as IPostMedia;
        }

        private PostMediaExternalContract SerializeToExternalContract(IPostMedia media)
        {
            if (media == null)
            {
                return null;
            }
            var t = media.GetTypeForSerializer();
            var serializer = ModuleProvider.QueryModule<IPostMediaSerializer, Type>(t)
                             ?? throw new ModuleNotFoundException($"Неизвестный тип медиа-объекта поста для сериализации {t.FullName}");
            var bytes = serializer.SerializeToBytes(media);
            return new PostMediaExternalContract()
            {
                BinaryData = bytes != null ? Convert.ToBase64String(bytes) : null,
                TypeId = serializer.TypeId,
            };
        }

        private IPostMedia DeserializeExternalContract(PostMediaExternalContract contract)
        {
            if (contract?.BinaryData == null)
            {
                return null;
            }
            var serializer = ModuleProvider.QueryModule<IPostMediaSerializer, string>(contract.TypeId) 
                ?? throw new ModuleNotFoundException($"Неизвестный тип сериализованного медиа-объекта поста {contract.TypeId}");
            var bytes = contract.BinaryData != null ? Convert.FromBase64String(contract.BinaryData) : null;
            return serializer.Deserialize(bytes);
        }
    }
}