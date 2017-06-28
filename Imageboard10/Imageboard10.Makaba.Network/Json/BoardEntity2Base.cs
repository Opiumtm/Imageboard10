using Newtonsoft.Json;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// Сущность "борда или тред".
    /// </summary>
    public class BoardEntity2Base
    {
        /// <summary>
        /// Борда.
        /// </summary>
        [JsonProperty("Board")]
        public string Board { get; set; }

        /// <summary>
        /// Информация о борде.
        /// </summary>
        [JsonProperty("BoardInfo")]
        public string BoardInfo { get; set; }

        /// <summary>
        /// Дополнительная информация о борде.
        /// </summary>
        [JsonProperty("BoardInfoOuter")]
        public string BoardInfoOuter { get; set; }

        /// <summary>
        /// Имя борды.
        /// </summary>
        [JsonProperty("BoardName")]
        public string BoardName { get; set; }

        /// <summary>
        /// Реклама.
        /// </summary>
        [JsonProperty("advert_bottom_image")]
        public string AdvertBottomImage { get; set; }

        /// <summary>
        /// Реклама.
        /// </summary>
        [JsonProperty("advert_bottom_link")]
        public string AdvertBottomLink { get; set; }

        /// <summary>
        /// Баннер.
        /// </summary>
        [JsonProperty("board_banner_image")]
        public string BoardBannerImage { get; set; }

        /// <summary>
        /// Ссылка на борду (короткое имя).
        /// </summary>
        [JsonProperty("board_banner_link")]
        public string BoardBannerLink { get; set; }

        /// <summary>
        /// Скорость борды.
        /// </summary>
        [JsonProperty("board_speed")]
        public int? BoardSpeed { get; set; }

        /// <summary>
        /// Бамп лимит.
        /// </summary>
        [JsonProperty("bump_limit")]
        public int? BumpLimit { get; set; }

        /// <summary>
        /// Текущая страница.
        /// </summary>
        [JsonProperty("current_page")]
        public int? CurrentPage { get; set; }

        /// <summary>
        /// Текущий тред.
        /// </summary>
        [JsonProperty("current_thread")]
        public int? CurrentThread { get; set; }

        /// <summary>
        /// Имя по умолчанию.
        /// </summary>
        [JsonProperty("default_name")]
        public string DefaultName { get; set; }

        /// <summary>
        /// Разрешить аудио.
        /// </summary>
        [JsonProperty("enable_audio")]
        public int? EnableAudio { get; set; }

        /// <summary>
        /// Разрешить кубики (?)
        /// </summary>
        [JsonProperty("enable_dices")]
        public int? EnableDices { get; set; }

        /// <summary>
        /// Разрешить иконки.
        /// </summary>
        [JsonProperty("enable_flags")]
        public int? EnableFlags { get; set; }

        /// <summary>
        /// Разрешить иконки.
        /// </summary>
        [JsonProperty("enable_icons")]
        public int? EnableIcons { get; set; }

        /// <summary>
        /// Разрешить изображения.
        /// </summary>
        [JsonProperty("enable_images")]
        public int? EnableImages { get; set; }

        /// <summary>
        /// Разрешить лайки.
        /// </summary>
        [JsonProperty("enable_likes")]
        public int? EnableLikes { get; set; }

        /// <summary>
        /// Разрешить имена.
        /// </summary>
        [JsonProperty("enable_names")]
        public int? EnableNames { get; set; }

        /// <summary>
        /// Разрешить Oekaki (?).
        /// </summary>
        [JsonProperty("enable_oekaki")]
        public int? EnableOekaki { get; set; }

        /// <summary>
        /// Разрешить постинг.
        /// </summary>
        [JsonProperty("enable_posting")]
        public int? EnablePosting { get; set; }

        /// <summary>
        /// Разрешить сажу.
        /// </summary>
        [JsonProperty("enable_sage")]
        public int? EnableSage { get; set; }

        /// <summary>
        /// Разрешить щит (?).
        /// </summary>
        [JsonProperty("enable_shield")]
        public int? EnableShield { get; set; }

        /// <summary>
        /// Разрешить заголовок.
        /// </summary>
        [JsonProperty("enable_subject")]
        public int? EnableSubject { get; set; }

        /// <summary>
        /// Разрешить тэги тредов.
        /// </summary>
        [JsonProperty("enable_thread_tags")]
        public int? EnableThreadTags { get; set; }

        /// <summary>
        /// Разрешить трипкоды.
        /// </summary>
        [JsonProperty("enable_trips")]
        public int? EnableTrips { get; set; }

        /// <summary>
        /// Разрешить видео.
        /// </summary>
        [JsonProperty("enable_video")]
        public int? EnableVideo { get; set; }

        /// <summary>
        /// Иконки.
        /// </summary>
        [JsonProperty("icons")]
        public BoardIcon2[] Icons { get; set; }

        /// <summary>
        /// Объект является бордой.
        /// </summary>
        [JsonProperty("is_board")]
        public int? IsBoard { get; set; }

        /// <summary>
        /// Объект является индексом.
        /// </summary>
        [JsonProperty("is_index")]
        public int? IsIndex { get; set; }

        /// <summary>
        /// Максимальный комментарий.
        /// </summary>
        [JsonProperty("max_comment")]
        public int? MaxComment { get; set; }

        /// <summary>
        /// Максимальный размер файла.
        /// </summary>
        [JsonProperty("max_files_size")]
        public int? MaxFilesSize { get; set; }

        /// <summary>
        /// Новости.
        /// </summary>
        [JsonProperty("news_abu")]
        public BoardEntityNewsReference[] NewsAbu { get; set; }

        /// <summary>
        /// Страницы.
        /// </summary>
        [JsonProperty("pages")]
        public int[] Pages { get; set; }

        /// <summary>
        /// Тэги.
        /// </summary>
        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Верхняя реклама досок.
        /// </summary>
        [JsonProperty("top")]
        public BoardEntity2TopAdvert[] TopAdvert { get; set; }

        /// <summary>
        /// Тред закрыт.
        /// </summary>
        [JsonProperty("is_closed")]
        public int? IsClosed { get; set; }

        /// <summary>
        /// Максимальный номер.
        /// </summary>
        [JsonProperty("max_num")]
        public int? MaxNum { get; set; }

        /// <summary>
        /// Уникальных постеров.
        /// </summary>
        [JsonProperty("unique_posters")]
        public string UniquePosters { get; set; }

        /// <summary>
        /// Фильтр.
        /// </summary>
        [JsonProperty("filter")]
        public string Filter { get; set; }
    }
}