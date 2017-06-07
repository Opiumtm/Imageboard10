using System;

namespace Imageboard10.Core.ModelInterface
{
    /// <summary>
    /// Провайдер типа для сериализации.
    /// </summary>
    public interface ITypeProviderForSerializer
    {
        /// <summary>
        /// Получить тип для сериализации.
        /// </summary>
        /// <returns>Тип для сериализации.</returns>
        Type GetTypeForSerializer();
    }
}