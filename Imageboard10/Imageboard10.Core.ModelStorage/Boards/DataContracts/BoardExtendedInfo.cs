using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posting;
using Imageboard10.Core.Models.Boards;

namespace Imageboard10.Core.ModelStorage.Boards.DataContracts
{
    /// <summary>
    /// Контракт расширенных данных по доске.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class BoardExtendedInfo
    {
        /// <summary>
        /// Поля для постинга.
        /// </summary>
        [DataMember]
        public List<BoardPostingCapability> PostingCapabilities { get; set; }

        /// <summary>
        /// Иконки.
        /// </summary>
        [DataMember]
        public List<BoardIconContract> Icons { get; set; }

        /// <summary>
        /// Разрешены лайки.
        /// </summary>
        [DataMember]
        public bool LikesEnabled { get; set; }

        /// <summary>
        /// Разрешены трипкоды.
        /// </summary>
        [DataMember]
        public bool TripCodesEnabled { get; set; }

        /// <summary>
        /// Разрешена сажа.
        /// </summary>
        [DataMember]
        public bool SageEnabled { get; set; }

        /// <summary>
        /// Разрешены тэги тредов.
        /// </summary>
        [DataMember]
        public bool ThreadTagsEnabled { get; set; }

        /// <summary>
        /// Привести к контракту.
        /// </summary>
        /// <param name="reference">Ссылка на доску.</param>
        /// <param name="serializationService">Сервис сериализации ссылок.</param>
        /// <returns>Контракт расширенных данных.</returns>
        public static BoardExtendedInfo ToContract(IBoardReference reference, ILinkSerializationService serializationService)
        {
            if (serializationService == null) throw new ArgumentNullException(nameof(serializationService));
            if (reference == null)
            {
                return null;
            }
            return new BoardExtendedInfo()
            {
                LikesEnabled = reference.LikesEnabled,
                SageEnabled = reference.SageEnabled,
                ThreadTagsEnabled = reference.ThreadTagsEnabled,
                TripCodesEnabled = reference.TripCodesEnabled,                
                Icons = reference.Icons?.Select(i => new BoardIconContract()
                {
                    Id = i?.Id,
                    Name = i?.Name,
                    MediaLink = i?.MediaLink != null ? serializationService.Serialize(i.MediaLink) : null,
                })?.ToList(),
                PostingCapabilities = reference.PostingCapabilities?.Select(BoardPostingCapability.ToContract)?.ToList()
            };
        }

        /// <summary>
        /// Установить дополнительную информацию.
        /// </summary>
        /// <param name="extended">Дополнительная информация.</param>
        /// <param name="reference">Ссылка.</param>
        /// <param name="serializationService">Сервис сериализации ссылок.</param>
        public static void SetExtendedInfoFor(BoardExtendedInfo extended, BoardReference reference, ILinkSerializationService serializationService)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (serializationService == null) throw new ArgumentNullException(nameof(serializationService));
            if (extended == null)
            {
                reference.LikesEnabled = false;
                reference.Icons = null;
                reference.PostingCapabilities = null;
            }
            else
            {
                reference.LikesEnabled = extended.LikesEnabled;
                reference.Icons = extended.Icons?.Select(i => new BoardIcon()
                {
                    Id = i?.Id,
                    Name = i?.Name,
                    MediaLink = i?.MediaLink != null ? serializationService.Deserialize(i.MediaLink) : null
                })?.OfType<IBoardIcon>()?.ToList();
                if (extended.PostingCapabilities != null)
                {
                    foreach (var c in extended.PostingCapabilities.OfType<BoardPostingIconCapability>())
                    {
                        c.UpdateInterface();
                    }
                }
                reference.PostingCapabilities = extended.PostingCapabilities?.OfType<IPostingCapability>()?.ToList();
            }
        }
    }

    /// <summary>
    /// Контракт иконки.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class BoardIconContract
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Имя.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Ссылка на медиа в виде сериализованной строки.
        /// </summary>
        [DataMember]
        public string MediaLink { get; set; }
    }

    /// <summary>
    /// Поле постинга.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    [KnownType(typeof(BoardPostingCaptchaCapability))]
    [KnownType(typeof(BoardPostingCommentCapability))]
    [KnownType(typeof(BoardPostingIconCapability))]
    [KnownType(typeof(BoardPostingMediaFileCapability))]
    public class BoardPostingCapability : IPostingCapability
    {
        /// <summary>
        /// Роль поля.
        /// </summary>
        [DataMember]
        public Guid Role { get; set; }

        /// <summary>
        /// Привести к контракту.
        /// </summary>
        /// <param name="capability">Свойство постинга.</param>
        /// <returns>Контракт.</returns>
        public static BoardPostingCapability ToContract(IPostingCapability capability)
        {
            switch (capability)
            {
                case IPostingCaptchaCapability c:
                    return new BoardPostingCaptchaCapability
                    {
                        Role = c.Role,
                        CaptchaTypes = c.CaptchaTypes?.ToList()
                    };
                case IPostingCommentCapability c:
                    return new BoardPostingCommentCapability()
                    {
                        Role = c.Role,
                        MarkupType = c.MarkupType
                    };
                case IPostingIconCapability c:
                    return new BoardPostingIconCapability()
                    {
                        Role = c.Role,
                        Icons = c.Icons?.Select(i => new BoardPostingCapabilityIcon()
                        {
                            Name = i?.Name,
                            Id = i?.Id
                        })?.ToList(),
                    }.UpdateInterface();
                case IPostingMediaFileCapability c:
                    return new BoardPostingMediaFileCapability()
                    {
                        Role = c.Role,
                        MaxFileCount = c.MaxFileCount
                    };
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Поле капчи.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class BoardPostingCaptchaCapability : BoardPostingCapability, IPostingCaptchaCapability
    {
        /// <summary>
        /// Типы капчи.
        /// </summary>
        IList<Guid> IPostingCaptchaCapability.CaptchaTypes => CaptchaTypes;

        /// <summary>
        /// Типы капчи.
        /// </summary>
        [DataMember]
        public List<Guid> CaptchaTypes { get; set; }
    }

    /// <summary>
    /// Поле комментария.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class BoardPostingCommentCapability : BoardPostingCapability, IPostingCommentCapability
    {
        /// <summary>
        /// Тип разметки.
        /// </summary>
        [DataMember]
        public Guid MarkupType { get; set; }

        /// <summary>
        /// Максимальный размер.
        /// </summary>
        [DataMember]
        public int? MaxLength { get; set; }
    }

    /// <summary>
    /// Поле с иконкой.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class BoardPostingIconCapability : BoardPostingCapability, IPostingIconCapability
    {
        /// <summary>
        /// Доступные для выбора иконки.
        /// </summary>
        IList<IPostingCapabilityIcon> IPostingIconCapability.Icons => _icons;

        [IgnoreDataMember]
        private IList<IPostingCapabilityIcon> _icons;

        /// <summary>
        /// Доступные для выбора иконки.
        /// </summary>
        [DataMember]
        public List<BoardPostingCapabilityIcon> Icons { get; set; }

        /// <summary>
        /// Обновить данные в интерфейсе.
        /// </summary>
        public BoardPostingIconCapability UpdateInterface()
        {
            if (Icons == null)
            {
                _icons = null;
            }
            else
            {
                _icons = Icons.OfType<IPostingCapabilityIcon>().ToList();
            }
            return this;
        }
    }

    /// <summary>
    /// Поле медиафайлов.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class BoardPostingMediaFileCapability : BoardPostingCapability, IPostingMediaFileCapability
    {
        /// <summary>
        /// Максимальное количество файлов.
        /// </summary>
        [DataMember]
        public int? MaxFileCount { get; set; }
    }

    /// <summary>
    /// Иконка.
    /// </summary>
    [DataContract(Namespace = CoreConstants.DvachBrowserNamespace)]
    public class BoardPostingCapabilityIcon : IPostingCapabilityIcon
    {
        /// <summary>
        /// Идентификатор иконки.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Имя.
        /// </summary>
        [DataMember]
        public string Name { get; set; }
    }
}