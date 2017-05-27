using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Сериализатор ссылок.
    /// </summary>
    public sealed class CatalogLinkSerializer : LinkSerializerBase<CatalogLink, CatalogLinkSerializer.Jo>
    {
        public class Jo
        {
            [JsonProperty("e")]
            public string Engine { get; set; }

            [JsonProperty("b")]
            public string Board { get; set; }

            [JsonProperty("s")]
            public int Sort { get; set; }
        }

        /// <summary>
        /// Идентификатор типа ссылки.
        /// </summary>
        public override string LinkTypeId => "catalog";

        /// <summary>
        /// Получить объект JSON.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Объект JSON.</returns>
        protected override Jo GetJsonObject(CatalogLink link)
        {
            return new Jo()
            {
                Engine = link.Engine,
                Board = link.Board,
                Sort = (int)link.SortMode
            };
        }

        /// <summary>
        /// Заполнить значения.
        /// </summary>
        /// <param name="result">Результат.</param>
        /// <param name="jsonObject">JSON-объект.</param>
        protected override void FillValues(CatalogLink result, Jo jsonObject)
        {
            result.Engine = jsonObject.Engine;
            result.Board = jsonObject.Board;
            result.SortMode = (BoardCatalogSort) jsonObject.Sort;
        }
    }
}