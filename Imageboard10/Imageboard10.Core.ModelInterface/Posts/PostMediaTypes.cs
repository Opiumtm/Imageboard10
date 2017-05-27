using System;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Типы медиа постов.
    /// </summary>
    public static class PostMediaTypes
    {
        /// <summary>
        /// Статическое изображение.
        /// </summary>
        public static Guid Image { get; } = new Guid("{28AEE534-6C57-41E6-A2BB-31C5489A5F02}");

        /// <summary>
        /// Видео в формате webm.
        /// </summary>
        public static Guid WebmVideo { get; } = new Guid("{A7A24757-5DB1-4B2A-9166-421C882F0684}");

        /// <summary>
        /// Видео YouTube.
        /// </summary>
        public static Guid YoutubeVideo { get; } = new Guid("{CEABDC54-479F-4D5A-A8C1-D6106A88B3B2}");
    }
}