using System;

namespace Imageboard10.Core.ModelInterface.Links
{
    /// <summary>
    /// Ссылка на капчу.
    /// </summary>
    public interface ICaptchaLink
    {
        /// <summary>
        /// Тип капчи.
        /// </summary>
        Guid CaptchaType { get; }

        /// <summary>
        /// Контекст капчи.
        /// </summary>
        Guid CaptchaContext { get; }

        /// <summary>
        /// Идентификатор капчи.
        /// </summary>
        string CaptchaId { get; }
    }
}