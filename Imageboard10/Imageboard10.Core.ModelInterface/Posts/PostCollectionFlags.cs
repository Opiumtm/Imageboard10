using System;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Флаги коллекций постов.
    /// </summary>
    public static class PostCollectionFlags
    {
        /// <summary>
        /// Разрешить аудио.
        /// </summary>
        public static Guid EnableAudio { get; } = new Guid("{6EB8C97A-D87F-4A39-892C-683931D23C6D}");

        /// <summary>
        /// Разрешить dices.
        /// </summary>
        public static Guid EnableDices { get; } = new Guid("{E217378E-D667-4F4F-B3C5-38CD90531D4E}");

        /// <summary>
        /// Разрешить флаги стран.
        /// </summary>
        public static Guid EnableCountryFlags { get; } = new Guid("{49C70387-78AE-4C3F-B812-C15E9D7F6729}");

        /// <summary>
        /// Разрешить иконки.
        /// </summary>
        public static Guid EnableIcons { get; } = new Guid("{B03E527A-52DE-4B28-B517-D4312815F24B}");

        /// <summary>
        /// Разрешить изображения.
        /// </summary>
        public static Guid EnableImages { get; } = new Guid("{7FA40448-998A-44A3-9643-71D381A88689}");

        /// <summary>
        /// Разрешить лайки.
        /// </summary>
        public static Guid EnableLikes { get; } = new Guid("{FD9D39DD-4BE0-47EC-AE1E-51E12B0D9842}");

        /// <summary>
        /// Разрешить имена.
        /// </summary>
        public static Guid EnableNames { get; } = new Guid("{207397CE-5E1C-4342-9B6E-78542CC32C85}");

        /// <summary>
        /// Разрешить Oekaki (?).
        /// </summary>
        public static Guid EnableOekaki { get; } = new Guid("{3D1FE529-AB99-4ACB-B782-68454EE6C2D3}");

        /// <summary>
        /// Разрешить постинг.
        /// </summary>
        public static Guid EnablePosting { get; } = new Guid("{DD0A97D7-99B2-4CA6-91A6-3D7525196BA4}");

        /// <summary>
        /// Разрешить сажу.
        /// </summary>
        public static Guid EnableSage { get; } = new Guid("{6A86D7B8-BADC-42F8-9C23-2DAA23839CFF}");

        /// <summary>
        /// Разрешить Shield (?).
        /// </summary>
        public static Guid EnableShield { get; } = new Guid("{298E43BD-1500-41D1-B942-16074A92E5EA}");

        /// <summary>
        /// Разрешить заголовок.
        /// </summary>
        public static Guid EnableSubject { get; } = new Guid("{08F56D6F-623D-4227-8595-512638770C49}");

        /// <summary>
        /// Разрешить тэги тредов.
        /// </summary>
        public static Guid EnableThreadTags { get; } = new Guid("{F9D37683-3EA8-486A-A7BD-545856024F55}");

        /// <summary>
        /// Разрешить трипкоды.
        /// </summary>
        public static Guid EnableTripcodes { get; } = new Guid("{F162ED02-F0D5-45AB-99C3-7BE76B521114}");

        /// <summary>
        /// Разрешить видео.
        /// </summary>
        public static Guid EnableVideo { get; } = new Guid("{FDF3301A-B635-49EF-B44E-CDB938844D9A}");

        /// <summary>
        /// Борда.
        /// </summary>
        public static Guid IsBoard { get; } = new Guid("{6066E9F1-28C8-4B7B-AB94-9BC9663FA5EB}");

        /// <summary>
        /// Индекс.
        /// </summary>
        public static Guid IsIndex { get; } = new Guid("{D5421C60-77C6-4938-AFF3-C3FCC76E0BAC}");
    }
}