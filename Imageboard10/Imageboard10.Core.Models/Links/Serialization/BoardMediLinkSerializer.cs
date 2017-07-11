using System.Runtime.Serialization;
using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Сериализатор ссылок.
    /// </summary>
    public sealed class BoardMediLinkSerializer : LinkSerializerBase<BoardMediaLink, BoardMediLinkSerializer.Jo>
    {
        [DataContract]
        public class Jo
        {
            [DataMember(Name = "e")]
            public string Engine { get; set; }

            [DataMember(Name = "b")]
            public string Board { get; set; }

            [DataMember(Name = "u")]
            public string Uri { get; set; }
        }

        /// <summary>
        /// Идентификатор типа ссылки.
        /// </summary>
        public override string LinkTypeId => "boardmedia";

        /// <summary>
        /// Получить объект JSON.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Объект JSON.</returns>
        protected override Jo GetJsonObject(BoardMediaLink link)
        {
            return new Jo()
            {
                Engine = link.Engine,
                Board = link.Board,
                Uri = link.Uri
            };
        }

        /// <summary>
        /// Заполнить значения.
        /// </summary>
        /// <param name="result">Результат.</param>
        /// <param name="jsonObject">JSON-объект.</param>
        protected override void FillValues(BoardMediaLink result, Jo jsonObject)
        {
            result.Engine = jsonObject.Engine;
            result.Board = jsonObject.Board;
            result.Uri = jsonObject.Uri;
        }
    }
}