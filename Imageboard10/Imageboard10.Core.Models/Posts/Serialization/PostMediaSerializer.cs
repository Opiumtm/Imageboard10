using Imageboard10.Core.Models.Posts.PostMedia;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// Сериализатор медиа.
    /// </summary>
    public sealed class PostMediaSerializer : PostMediaSerializerBase<PostMedia.PostMedia>
    {
        /// <summary>
        /// Идентификатор типа.
        /// </summary>
        public override string TypeId => "std.base";
    }

    /// <summary>
    /// Сериализатор медиа.
    /// </summary>
    public sealed class PostMediaWithSizeSerializer : PostMediaSerializerBase<PostMediaWithSize>
    {
        /// <summary>
        /// Идентификатор типа.
        /// </summary>
        public override string TypeId => "std.w/size";
    }

    /// <summary>
    /// Сериализатор медиа.
    /// </summary>
    public sealed class PostMediaWithThumbnailSerializer : PostMediaSerializerBase<PostMediaWithThumbnail>
    {
        /// <summary>
        /// Идентификатор типа.
        /// </summary>
        public override string TypeId => "std.w/thumbnail";
    }

    /// <summary>
    /// Сериализатор медиа.
    /// </summary>
    public sealed class PostMediaExternalSerializer : PostMediaSerializerBase<PostMediaExternalContract>
    {
        /// <summary>
        /// Идентификатор типа.
        /// </summary>
        public override string TypeId => "std.external";
    }

}