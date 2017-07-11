using System.Runtime.Serialization;
using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Сериализатор ссылок.
    /// </summary>
    public sealed class PostLinkSerializer : LinkSerializerBase<PostLink, PostLinkSerializer.Jo>
    {
        [DataContract]
        public class Jo
        {
            [DataMember(Name = "e")]
            public string Engine { get; set; }

            [DataMember(Name = "b")]
            public string Board { get; set; }

            [DataMember(Name = "t")]
            public int OpPostNum { get; set; }

            [DataMember(Name = "p")]
            public int PostNum { get; set; }
        }

        /// <summary>
        /// Идентификатор типа ссылки.
        /// </summary>
        public override string LinkTypeId => "post";

        /// <summary>
        /// Получить объект JSON.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Объект JSON.</returns>
        protected override Jo GetJsonObject(PostLink link)
        {
            return new Jo()
            {
                Engine = link.Engine,
                Board = link.Board,
                OpPostNum = link.OpPostNum,
                PostNum = link.PostNum
            };
        }

        /// <summary>
        /// Заполнить значения.
        /// </summary>
        /// <param name="result">Результат.</param>
        /// <param name="jsonObject">JSON-объект.</param>
        protected override void FillValues(PostLink result, Jo jsonObject)
        {
            result.Engine = jsonObject.Engine;
            result.Board = jsonObject.Board;
            result.OpPostNum = jsonObject.OpPostNum;
            result.PostNum = jsonObject.PostNum;
        }
    }
}