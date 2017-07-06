using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Windows.UI;

namespace Imageboard10.Core.ModelStorage.Posts
{
    /// <summary>
    /// Прочие данные.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostOtherData
    {
        /// <summary>
        /// MD5-хэш, если есть.
        /// </summary>
        [DataMember]
        public string Hash { get; set; }

        /// <summary>
        /// Адрес почты, если есть.
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        [DataMember]
        public string UniqueId { get; set; }

        /// <summary>
        /// Автор.
        /// </summary>
        [DataMember]
        public PostOtherDataPoster Poster { get; set; }

        /// <summary>
        /// Иконка.
        /// </summary>
        [DataMember]
        public PostOtherDataIcon Icon { get; set; }

        /// <summary>
        /// Страна.
        /// </summary>
        [DataMember]
        public PostOtherDataCountry Country { get; set; }
    }

    /// <summary>
    /// Автор.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostOtherDataPoster
    {
        /// <summary>
        /// Трипкод.
        /// </summary>
        [DataMember]
        public string Tripcode { get; set; }

        /// <summary>
        /// Цвет имени.
        /// </summary>
        [DataMember]
        public string NameColorStr { get; set; }

        /// <summary>
        /// Цвет имени.
        /// </summary>
        [DataMember]
        public PostOtherDataColor NameColor { get; set; }
    }

    /// <summary>
    /// Цвет.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostOtherDataColor
    {
        [DataMember]
        public byte A { get; set; }

        [DataMember]
        public byte R { get; set; }

        [DataMember]
        public byte G { get; set; }

        [DataMember]
        public byte B { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Color?(PostOtherDataColor color)
        {
            if (color == null)
            {
                return null;
            }
            return new Color() { A = color.A, R = color.R, G = color.G, B = color.B };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PostOtherDataColor(Color? color)
        {
            if (color == null)
            {
                return null;
            }
            var cv = color.Value;
            return new PostOtherDataColor() { A = cv.A, R = cv.R, G = cv.G, B = cv.B };
        }
    }

    /// <summary>
    /// Иконка.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostOtherDataIcon
    {
        /// <summary>
        /// Ссылка на иконку.
        /// </summary>
        [DataMember]
        public string ImageLink { get; set; }

        /// <summary>
        /// Описание.
        /// </summary>
        [DataMember]
        public string Description { get; set; }
    }

    /// <summary>
    /// Иконка.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class PostOtherDataCountry
    {
        /// <summary>
        /// Ссылка на иконку.
        /// </summary>
        [DataMember]
        public string ImageLink { get; set; }
    }
}