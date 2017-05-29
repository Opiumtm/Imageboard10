using System;
using System.Collections.Generic;

namespace Imageboard10.Core.Models.Boards
{
    /// <summary>
    /// Постинг капчи.
    /// </summary>
    public class PostingCaptchaCapability : PostingCapability
    {
        /// <summary>
        /// Типы капчи.
        /// </summary>
        public IList<Guid> CaptchaTypes { get; set; }
    }
}