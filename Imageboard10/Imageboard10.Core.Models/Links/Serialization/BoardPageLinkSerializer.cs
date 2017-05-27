using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// ������������ ������.
    /// </summary>
    public sealed class BoardPageLinkSerializer : LinkSerializerBase<BoardPageLink, BoardPageLinkSerializer.Jo>
    {
        public class Jo
        {
            [JsonProperty("e")]
            public string Engine { get; set; }

            [JsonProperty("b")]
            public string Board { get; set; }

            [JsonProperty("p")]
            public int Page { get; set; }
        }

        /// <summary>
        /// ������������� ���� ������.
        /// </summary>
        public override string LinkTypeId => "boardpage";

        /// <summary>
        /// �������� ������ JSON.
        /// </summary>
        /// <param name="link">������.</param>
        /// <returns>������ JSON.</returns>
        protected override Jo GetJsonObject(BoardPageLink link)
        {
            return new Jo()
            {
                Engine = link.Engine,
                Board = link.Board,
                Page = link.Page
            };
        }

        /// <summary>
        /// ��������� ��������.
        /// </summary>
        /// <param name="result">���������.</param>
        /// <param name="jsonObject">JSON-������.</param>
        protected override void FillValues(BoardPageLink result, Jo jsonObject)
        {
            result.Engine = jsonObject.Engine;
            result.Board = jsonObject.Board;
            result.Page = jsonObject.Page;
        }
    }
}