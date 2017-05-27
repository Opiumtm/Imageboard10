using System;
using System.Collections.Generic;

namespace Imageboard10.Core.ModelInterface.Posting
{
    /// <summary>
    /// Поле капчи.
    /// </summary>
    public interface IPostingCaptchaCapability : IPostingCapability
    {
        /// <summary>
        /// Типы капчи.
        /// </summary>
        IList<Guid> CaptchaTypes { get; }
    }
}