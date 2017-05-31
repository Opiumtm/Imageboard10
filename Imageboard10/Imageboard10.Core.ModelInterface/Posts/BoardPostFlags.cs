using System;

namespace Imageboard10.Core.ModelInterface.Posts
{
    /// <summary>
    /// Флаги поста.
    /// </summary>
    public static class BoardPostFlags
    {
        /// <summary>
        /// Сажа.
        /// </summary>
        public static Guid Sage { get; } = new Guid("{4B6319F1-449E-46D2-B48E-3BF7C25F9FD4}");

        /// <summary>
        /// Постер забанен.
        /// </summary>
        public static Guid Banned { get; } = new Guid("{A1E4317C-B33C-4D91-81FA-2291FF13AAB4}");

        /// <summary>
        /// Прикреплённый пост.
        /// </summary>
        public static Guid Sticky { get; } = new Guid("{BAE46432-5AD5-4C80-8EFD-E7EAA76D9B71}");

        /// <summary>
        /// Тема закрыта.
        /// </summary>
        public static Guid Closed { get; } = new Guid("{D1498103-34DB-4A26-9D90-F42E7AB89889}");

        /// <summary>
        /// Предварительный просмотр треда.
        /// </summary>
        public static Guid ThreadPreview { get; } = new Guid("{145B822F-D035-45EA-99F1-46DA68F64580}");

        /// <summary>
        /// Пост редактирован.
        /// </summary>
        public static Guid IsEdited { get; } = new Guid("{6CD9433A-BCAA-4003-9409-7F684CD2C24E}");

        /// <summary>
        /// ОП.
        /// </summary>
        public static Guid Op { get; } = new Guid("{5DF2C3FD-958C-4D25-9D69-42CB971E33D4}");

        /// <summary>
        /// Трипкод администратора.
        /// </summary>
        public static Guid AdminTrip { get; } = new Guid("{D8C106E6-F45B-42BC-967B-ED4FA96771AC}");

        /// <summary>
        /// ОП-пост превью треда.
        /// </summary>
        public static Guid ThreadPreviewOpPost { get; } = new Guid("{E80DD784-CABD-48A8-BF6A-A2321E3ECC27}");

    }
}