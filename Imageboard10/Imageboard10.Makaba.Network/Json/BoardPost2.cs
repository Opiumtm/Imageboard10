﻿using Newtonsoft.Json;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Пост на борде (makaba).
    /// </summary>
    public class BoardPost2 : BoardPost
    {
        /// <summary>
        /// MD5-хэш.
        /// </summary>
        [JsonProperty("md5")]
        public string Md5 { get; set; }

        /// <summary>
        /// Трипкод.
        /// </summary>
        [JsonProperty("trip")]
        public string Tripcode { get; set; }

        /// <summary>
        /// Адрес почты.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Редактировался.
        /// </summary>
        [JsonProperty("edited")]
        public string Edited { get; set; }

        /// <summary>
        /// Имя изображения.
        /// </summary>
        [JsonProperty("image_name")]
        public string ImageName { get; set; }

        /// <summary>
        /// Количество изображений.
        /// </summary>
        [JsonProperty("files_count")]
        public string FilesCount { get; set; }

        /// <summary>
        /// Иконка.
        /// </summary>
        [JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Файлы.
        /// </summary>
        [JsonProperty("files")]
        public BoardPostFile2[] Files { get; set; }

        /// <summary>
        /// Тэги.
        /// </summary>
        [JsonProperty("tags")]
        public string Tags { get; set; }

        /// <summary>
        /// Лайки.
        /// </summary>
        [JsonProperty("likes")]
        public int? Likes { get; set; }

        /// <summary>
        /// Дизлайки.
        /// </summary>
        [JsonProperty("dislikes")]
        public int? Dislikes { get; set; }

        /// <summary>
        /// Бесконечный тред.
        /// </summary>
        [JsonProperty("endless")]
        public int? Endless { get; set; }

        /// <summary>
        /// Номер поста в треде.
        /// </summary>
        [JsonProperty("number")]
        public int? CountNumber { get; set; }
    }
}