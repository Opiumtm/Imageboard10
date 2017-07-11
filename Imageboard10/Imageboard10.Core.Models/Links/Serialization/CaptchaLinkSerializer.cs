using System;
using System.Runtime.Serialization;
using Imageboard10.Core.Models.Links.LinkTypes;
using Newtonsoft.Json;

namespace Imageboard10.Core.Models.Links.Serialization
{
    /// <summary>
    /// Сериализатор ссылки на капчу.
    /// </summary>
    public class CaptchaLinkSerializer : LinkSerializerBase<CaptchaLink, CaptchaLinkSerializer.Jo>
    {
        [DataContract]
        public class Jo
        {
            [DataMember(Name = "e")]
            public string Engine { get; set; }

            [DataMember(Name = "t")]
            public Guid CaptchaType { get; set; }

            [DataMember(Name = "c")]
            public Guid CaptchaContext { get; set; }

            [DataMember(Name = "id")]
            public string CaptchaId { get; set; }

            [DataMember(Name = "b")]
            public string Board { get; set; }

            [DataMember(Name = "thd")]
            public int ThreadId { get; set; }
        }

        /// <summary>
        /// Идентификатор типа ссылки.
        /// </summary>
        public override string LinkTypeId => "captcha";

        /// <summary>
        /// Получить объект JSON.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <returns>Объект JSON.</returns>
        protected override Jo GetJsonObject(CaptchaLink link)
        {
            return new Jo()
            {
                Engine = link.Engine,
                CaptchaType = link.CaptchaType,
                CaptchaContext = link.CaptchaContext,
                CaptchaId = link.CaptchaId,
                Board = link.Board,
                ThreadId = link.ThreadId
            };
        }

        /// <summary>
        /// Заполнить значения.
        /// </summary>
        /// <param name="result">Результат.</param>
        /// <param name="jsonObject">JSON-объект.</param>
        protected override void FillValues(CaptchaLink result, Jo jsonObject)
        {
            result.Engine = jsonObject.Engine;
            result.CaptchaType = jsonObject.CaptchaType;
            result.CaptchaContext = jsonObject.CaptchaContext;
            result.CaptchaId = jsonObject.CaptchaId;
            result.Board = jsonObject.Board;
            result.ThreadId = jsonObject.ThreadId;
        }
    }
}