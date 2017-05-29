using System;
using System.Collections.Generic;
using System.Linq;
using Imageboard10.Core.ModelInterface.Boards;
using Imageboard10.Core.ModelInterface.Posting;
using Imageboard10.Core.Models.Boards;
using Imageboard10.Core.Models.Links.LinkTypes;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Network;
using Imageboard10.Makaba.Network.Json;

using static Imageboard10.Makaba.MakabaConstants;

namespace Imageboard10.Makaba.Network.JsonParsers
{
    /// <summary>
    /// Парсеры DTO макабы.
    /// </summary>
    public class MakabaBoardReferenceDtoParsers : ModuleBase<INetworkDtoParsers>, INetworkDtoParsers,
        INetworkDtoParser<MobileBoardInfoCollection, IList<IBoardReference>>,
        INetworkDtoParser<MobileBoardInfoWithCategory, IBoardReference>
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        public MakabaBoardReferenceDtoParsers()
            :base(false, false)
        {            
        }

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        public override object QueryView(Type viewType)
        {
            if (viewType == typeof(INetworkDtoParser<MobileBoardInfoCollection, IList<IBoardReference>>))
            {
                return this;
            }
            if (viewType == typeof(INetworkDtoParser<MobileBoardInfoWithCategory, IBoardReference>))
            {
                return this;
            }
            return base.QueryView(viewType);
        }

        private static readonly Guid[] CommonRoles = new Guid[]
        {
            PostingFieldSemanticRole.Captcha,
            PostingFieldSemanticRole.Comment,
            PostingFieldSemanticRole.Email,
            PostingFieldSemanticRole.Icon,
            PostingFieldSemanticRole.MediaFile,
            PostingFieldSemanticRole.OpFlag,
            PostingFieldSemanticRole.PosterName,
            PostingFieldSemanticRole.PosterTrip,
            PostingFieldSemanticRole.PosterTrip2,
            PostingFieldSemanticRole.SageFlag,
            PostingFieldSemanticRole.Title,
            PostingFieldSemanticRole.WatermarkFlag,
            PostingFieldSemanticRole.ThreadTag,
        };

        private BoardReference Parse(string category, MobileBoardInfo b)
        {
            var board = new BoardReference()
            {
                Category = category ?? "",
                DisplayName = b.Name,
                BoardLink = new BoardLink() { Engine = MakabaEngineId, Board = b.Id },
                ShortName = b.Id ?? "",
                IsAdult = "Взрослым".Equals(category) || AdultBoards.Contains(b.Id),                
                BumpLimit = b.BumpLimit,
                DefaultName = b.DefaultName,
                Icons = (b.Icons ?? new BoardIcon2[0]).Where(i => i != null).Select(i => new BoardIcon()
                {
                    Name = i.Name,
                    Id = i.Number,
                    MediaLink = new EngineMediaLink() { Engine = MakabaEngineId, Uri = i.Url }
                }).OfType<IBoardIcon>().ToList(),
                Pages = b.Pages,
                SageEnabled = b.Sage != 0,
                TripCodesEnabled = b.Tripcodes != 0,
                LikesEnabled = b.EnableLikes != 0,
                ThreadTagsEnabled = b.EnableThreadTags != 0,
                PostingCapabilities = new List<IPostingCapability>()
            };
            foreach (var cr in CommonRoles)
            {
                if (cr == PostingFieldSemanticRole.Comment)
                {
                    board.PostingCapabilities.Add(new PostingCommentCapability()
                    {
                        MaxLength = null,
                        MarkupType = MarkupTypes.Makaba,
                        Role = PostingFieldSemanticRole.Comment
                    });
                } else if (cr == PostingFieldSemanticRole.Captcha)
                {
                    board.PostingCapabilities.Add(new PostingCaptchaCapability()
                    {
                        Role = PostingFieldSemanticRole.Captcha,
                        CaptchaTypes = new List<Guid>()
                        {
                            CaptchaTypes.DvachCaptcha
                        }
                    });
                } else if (cr == PostingFieldSemanticRole.Icon)
                {
                    if (board.Icons.Count > 0)
                    {
                        board.PostingCapabilities.Add(new PostingIconCapability()
                        {
                            Role = PostingFieldSemanticRole.Icon,
                            Icons = board.Icons.Select(i => new PostingCapabilityIcon()
                            {
                                Name = i.Name,
                                Id = i.Id
                            }).OfType<IPostingCapabilityIcon>().ToList()
                        });
                    }
                } else if (cr == PostingFieldSemanticRole.MediaFile)
                {
                    if (!BoardsWithoutMedia.Contains(b.Id))
                    {
                        board.PostingCapabilities.Add(new PostingMediaFileCapability()
                        {
                            Role = PostingFieldSemanticRole.MediaFile,
                            MaxFileCount = 4
                        });
                    }
                } else if (cr == PostingFieldSemanticRole.Title)
                {
                    if (!BoardsWithoutTitle.Contains(b.Id))
                    {
                        board.PostingCapabilities.Add(new PostingCapability()
                        {
                            Role = PostingFieldSemanticRole.Title,
                        });
                    }
                } else if (cr == PostingFieldSemanticRole.ThreadTag)
                {
                    if (b.EnableThreadTags != 0)
                    {
                        board.PostingCapabilities.Add(new PostingCapability()
                        {
                            Role = PostingFieldSemanticRole.ThreadTag,
                        });
                    }
                }
                else
                {
                    board.PostingCapabilities.Add(new PostingCapability()
                    {
                        Role = cr,
                    });
                }
            }
            return board;
        }

        public BoardReference Default(string category, string boardId)
        {
            return Parse(category, new MobileBoardInfo()
            {
                BumpLimit = 500,
                Category = category,
                DefaultName = "Аноним",
                EnablePosting = 1,
                Icons = new BoardIcon2[0],
                Id = boardId,
                Name = "/" + boardId + "/",
                Pages = 5,
                Sage = 1,
                Tripcodes = 1,
                EnableLikes = 0,
                EnableThreadTags = 0
            });
        }

        /// <summary>
        /// Распарсить.
        /// </summary>
        /// <param name="source">Источник.</param>
        /// <returns>Результат.</returns>
        public IList<IBoardReference> Parse(MobileBoardInfoCollection source)
        {
            var result = new List<IBoardReference>();
            if (source.Boards != null)
            {
                result.AddRange(
                    from kv in source.Boards
                    where kv.Value != null
                    from b in kv.Value
                    select Parse(kv.Key, b)
                );
            }
            return result;
        }

        /// <summary>
        /// Распарсить.
        /// </summary>
        /// <param name="source">Источник.</param>
        /// <returns>Результат.</returns>
        public IBoardReference Parse(MobileBoardInfoWithCategory source)
        {
            return Parse(source.Category, source.Board);
        }
    }
}