using System;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Режим загрузки сущностей.
    /// </summary>
    public sealed class PostStoreLoadMode
    {
        /// <summary>
        /// Выяснить порядковый номер поста в треде.
        /// </summary>
        public bool RetrieveCounterNumber { get; set; }

        /// <summary>
        /// Режим загрузки постов.
        /// </summary>
        public PostStorePostsLoadMode PostsLoadMode { get; set; }

        /// <summary>
        /// Режим загрузки коллекуий.
        /// </summary>
        public PostStoreCollectionLoadMode CollectionLoadMode { get; set; }

        /// <summary>
        /// Режим рекурсии загрузки.
        /// </summary>
        public PostStoreCollectionLoadRecursionMode RecursionMode { get; set; }
    }

    /// <summary>
    /// Реэтм загрузки постов.
    /// </summary>
    public enum PostStorePostsLoadMode
    {
        /// <summary>
        /// Загрузка полных данных.
        /// </summary>
        Full,

        /// <summary>
        /// Облегчённые данные.
        /// </summary>
        Light,

        /// <summary>
        /// Только общую для всех типов информацию сущности.
        /// </summary>
        EntityOnly,

        /// <summary>
        /// Только ссылку.
        /// </summary>
        LinkOnly,
    }

    /// <summary>
    /// Реэтм загрузки постов.
    /// </summary>
    public enum PostStoreCollectionLoadMode
    {
        /// <summary>
        /// Загрузка полных данных.
        /// </summary>
        Full,

        /// <summary>
        /// Только общую для всех типов информацию сущности.
        /// </summary>
        EntityOnly,

        /// <summary>
        /// Только ссылку.
        /// </summary>
        LinkOnly,
    }

    /// <summary>
    /// Режим рекурсии для загрузки постов.
    /// </summary>
    public enum PostStoreCollectionLoadRecursionMode
    {
        /// <summary>
        /// Нет загрузки дочерних сущностей.
        /// </summary>
        None,

        /// <summary>
        /// Один уровень рекурсии.
        /// </summary>
        DirectChildOnly,

        /// <summary>
        /// Только ОП-пост. Для треда и каталога - то же самое, что и <see cref="DirectChildOnly"/>. Для страницы доски, все ОП-посты дочерних тредов.
        /// </summary>
        OpPostOnly,

        /// <summary>
        /// Полная загрузка дочерних постов.
        /// </summary>
        Full
    }
}