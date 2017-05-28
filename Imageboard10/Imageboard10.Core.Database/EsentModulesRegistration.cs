using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Регистрация модулей для работы с ESENT.
    /// </summary>
    public static class EsentModulesRegistration
    {
        /// <summary>
        /// Зарегистрировать модули.
        /// </summary>
        /// <param name="collection">Коллекция.</param>
        /// <param name="clearDbOnStart">Удалять содержимое базы данных при старте (для юнит-тестов).</param>
        public static void RegisterModules(IModuleCollection collection, bool clearDbOnStart = false)
        {
            collection.RegisterModule<EsentInstanceProvider, IEsentInstanceProvider>(new EsentInstanceProvider(clearDbOnStart));
        }    
    }
}