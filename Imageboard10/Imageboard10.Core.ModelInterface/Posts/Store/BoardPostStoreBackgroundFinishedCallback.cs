using System;

namespace Imageboard10.Core.ModelInterface.Posts.Store
{
    /// <summary>
    /// Обратный вызов по завершению фонового действия.
    /// </summary>
    /// <param name="ex">Ошибка или null, если завершено без ошибки.</param>
    public delegate void BoardPostStoreBackgroundFinishedCallback(Exception ex);
}