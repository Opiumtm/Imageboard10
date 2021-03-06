using System.Runtime.Serialization;
using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// ������������ ������.
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
        /// ������������� ���� ������.
        /// </summary>
        public override string LinkTypeId => "boardmedia";

        /// <summary>
        /// �������� ������ JSON.
        /// </summary>
        /// <param name="link">������.</param>
        /// <returns>������ JSON.</returns>
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
        /// ��������� ��������.
        /// </summary>
        /// <param name="result">���������.</param>
        /// <param name="jsonObject">JSON-������.</param>
        protected override void FillValues(BoardMediaLink result, Jo jsonObject)
        {
            result.Engine = jsonObject.Engine;
            result.Board = jsonObject.Board;
            result.Uri = jsonObject.Uri;
        }
    }
}