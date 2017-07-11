using System.Runtime.Serialization;
using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Сериализатор ссылок.
    /// </summary>
    public sealed class RootLinkSerializer : LinkSerializerBase<RootLink, RootLinkSerializer.Jo>
    {
        [DataContract]
        public class Jo
        {
            [DataMember(Name = "e")]
            public string Engine { get; set; }
        }

        /// <summary>
        /// Идентификатор типа ссылки.
        /// </summary>
        public override string LinkTypeId => "root";

        /// <summary>
        /// Получить объект JSON.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Объект JSON.</returns>
        protected override Jo GetJsonObject(RootLink link)
        {
            return new Jo()
            {
                Engine = link.Engine
            };
        }

        /// <summary>
        /// Заполнить значения.
        /// </summary>
        /// <param name="result">Результат.</param>
        /// <param name="jsonObject">JSON-объект.</param>
        protected override void FillValues(RootLink result, Jo jsonObject)
        {
            result.Engine = jsonObject.Engine;
        }
    }
}