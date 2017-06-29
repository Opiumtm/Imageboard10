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
        /// ОП-пост.
        /// </summary>
        public static Guid ThreadOpPost { get; } = new Guid("{E80DD784-CABD-48A8-BF6A-A2321E3ECC27}");

        /// <summary>
        /// Бесконечный тред.
        /// </summary>
        public static Guid Endless { get; } = new Guid("{91062F04-6A63-428E-AFF7-4E40F9C074E1}");

        /// <summary>
        /// Мой пост.
        /// </summary>
        public static Guid MyPost { get; } = new Guid("{EA6A4811-C5E0-45ED-9F1D-BB474FA5CD91}");
    }
}