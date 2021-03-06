using System.Runtime.Serialization;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// ������������ ������.
    /// </summary>
    public sealed class CatalogLinkSerializer : LinkSerializerBase<CatalogLink, CatalogLinkSerializer.Jo>
    {
        [DataContract]
        public class Jo
        {
            [DataMember(Name = "e")]
            public string Engine { get; set; }

            [DataMember(Name = "b")]
            public string Board { get; set; }

            [DataMember(Name = "s")]
            public int Sort { get; set; }
        }

        /// <summary>
        /// ������������� ���� ������.
        /// </summary>
        public override string LinkTypeId => "catalog";

        /// <summary>
        /// �������� ������ JSON.
        /// </summary>
        /// <param name="link">������.</param>
        /// <returns>������ JSON.</returns>
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
        /// ��������� ��������.
        /// </summary>
        /// <param name="result">���������.</param>
        /// <param name="jsonObject">JSON-������.</param>
        protected override void FillValues(CatalogLink result, Jo jsonObject)
        {
            result.Engine = jsonObject.Engine;
            result.Board = jsonObject.Board;
            result.SortMode = (BoardCatalogSort) jsonObject.Sort;
        }
    }
}