using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Сериализатор ссылок.
    /// </summary>
    public sealed class ThreadPartLinkSerializer : LinkSerializerBase<ThreadPartLink, ThreadPartLinkSerializer.Jo>
    {
        public class Jo
        {
            [JsonProperty("e")]
            public string Engine { get; set; }

            [JsonProperty("b")]
            public string Board { get; set; }

            [JsonProperty("t")]
            public int OpPostNum { get; set; }

            [JsonProperty("p")]
            public int FromPost { get; set; }
        }

        /// <summary>
        /// Идентификатор типа ссылки.
        /// </summary>
        public override string LinkTypeId => "threadpart";

        /// <summary>
        /// Получить объект JSON.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Объект JSON.</returns>
        protected override Jo GetJsonObject(ThreadPartLink link)
        {
            return new Jo()
            {
                Engine = link.Engine,
                Board = link.Board,
                OpPostNum = link.OpPostNum,
                FromPost = link.FromPost
            };
        }

        /// <summary>
        /// Заполнить значения.
        /// </summary>
        /// <param name="result">Результат.</param>
        /// <param name="jsonObject">JSON-объект.</param>
        protected override void FillValues(ThreadPartLink result, Jo jsonObject)
        {
            result.Engine = jsonObject.Engine;
            result.Board = jsonObject.Board;
            result.OpPostNum = jsonObject.OpPostNum;
            result.FromPost = jsonObject.FromPost;
        }
    }
}