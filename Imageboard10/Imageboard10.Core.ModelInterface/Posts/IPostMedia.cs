﻿using System;
using Imageboard10.Core.ModelInterface.Links;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Медиафайл поста.
    /// </summary>
    public interface IPostMedia : ISerializableObject
    {
        /// <summary>
        /// Ссылка на медиа.
        /// </summary>
        ILink MediaLink { get; }

        /// <summary>
        /// Тип медиафайла.
        /// </summary>
        Guid MediaType { get; }

        /// <summary>
        /// Размер файла.
        /// </summary>
        ulong? FileSize { get; }
    }
}