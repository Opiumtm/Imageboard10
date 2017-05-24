using System.Threading.Tasks;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Класс-помощник для модулей.
    /// </summary>
    public static class ModuleHelper
    {
        /// <summary>
        /// Получить интерфейс провайдера, пригодный для передачи через WinRT.
        /// </summary>
        /// <typeparam name="T">Тип провайдера.</typeparam>
        /// <param name="provider">Провайдер.</param>
        /// <returns>Совместимый с WinRT провайдер.</returns>
        // ReSharper disable once InconsistentNaming
        public static ModuleInterface.IModuleProvider AsWinRT<T>(this T provider)
            where T : IModuleProvider
        {
            if (provider == null)
            {
                return null;
            }
            return new ModuleProviderWrapper<T>(provider);
        }

        /// <summary>
        /// Получить интерфейс провайдера в виде .NET.
        /// </summary>
        /// <typeparam name="T">Тип провайдера.</typeparam>
        /// <param name="provider">Провайдер.</param>
        /// <returns>Совместимый с .NET-библиотекой провайдер.</returns>
        public static IModuleProvider AsDotnet<T>(this T provider)
            where T : ModuleInterface.IModuleProvider
        {
            if (provider == null)
            {
                return null;
            }
            return new ModuleProviderBackwardWrapper<T>(provider);
        }

        /// <summary>
        /// Получить интерфейс коллекции модулей, пригодный для передачи через WinRT.
        /// </summary>
        /// <typeparam name="T">Тип коллекции модулей.</typeparam>
        /// <param name="collection">Коллекция модулей.</param>
        /// <returns>Совместимая с WinRT коллекция модулей.</returns>
        // ReSharper disable once InconsistentNaming
        public static ModuleInterface.IModuleCollection AsWinRTCollection<T>(this T collection)
            where T : IModuleCollection
        {
            if (collection == null)
            {
                return null;
            }
            return new ModuleCollectionWrapper<T>(collection);
        }

        /// <summary>
        /// Получить интерфейс коллекции модулей в виде .NET.
        /// </summary>
        /// <typeparam name="T">Тип коллекции модулей.</typeparam>
        /// <param name="collection">Коллекция модулей.</param>
        /// <returns>Совместимая с .NET-библиотекой коллекция модулей.</returns>
        public static IModuleCollection AsDotnetCollection<T>(this T collection)
            where T : ModuleInterface.IModuleCollection
        {
            if (collection == null)
            {
                return null;
            }
            return new ModuleCollectionBackwardWrapper<T>(collection);
        }

        /// <summary>
        /// Получить интерфейс модуля, пригодный для передачи через WinRT.
        /// </summary>
        /// <typeparam name="T">Тип модуля.</typeparam>
        /// <param name="module">Модуль.</param>
        /// <returns>Совместимый с WinRT модуль.</returns>
        // ReSharper disable once InconsistentNaming
        public static ModuleInterface.IModule AsWinRTModule<T>(this T module)
            where T : IModule
        {
            if (module == null)
            {
                return null;
            }
            return new ModuleBackwardWrapper<T>(module);
        }

        /// <summary>
        /// Получить интерфейс модуля в виде .NET.
        /// </summary>
        /// <typeparam name="T">Тип модуля.</typeparam>
        /// <param name="module">Модуль.</param>
        /// <returns>Совместимый с .NET-библиотекой модуль.</returns>
        public static IModule AsDotnetModule<T>(this T module)
            where T : ModuleInterface.IModule
        {
            if (module == null)
            {
                return null;
            }
            return new ModuleWrapper<T>(module);
        }

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <typeparam name="T">Тип представления.</typeparam>
        /// <param name="module">Модуль.</param>
        /// <returns>Представление.</returns>
        public static T QueryView<T>(this IModule module)
        {
            if (module.QueryView(typeof(T)) is T r)
            {
                return r;
            }
            return default(T);
        }

        /// <summary>
        /// Запросить модуль.
        /// </summary>
        /// <typeparam name="T">Тип интерфейса.</typeparam>
        /// <param name="provider">Провайдер.</param>
        /// <returns>Модуль.</returns>
        public static T QueryModule<T>(this IModuleProvider provider)
            where T : class
        {
            var obj = provider.QueryModule<object>(typeof(T), null);
            return obj as T ?? obj?.QueryView<T>();
        }

        /// <summary>
        /// Запросить модуль асинхронно.
        /// </summary>
        /// <typeparam name="T">Тип интерфейса.</typeparam>
        /// <param name="provider">Провайдер.</param>
        /// <returns>Модуль.</returns>
        public static async Task<T> QueryModuleAsync<T>(this IModuleProvider provider)
            where T : class
        {
            var obj = await provider.QueryModuleAsync<object>(typeof(T), null);
            return obj as T ?? obj?.QueryView<T>();
        }

        /// <summary>
        /// Запросить модуль.
        /// </summary>
        /// <typeparam name="T">Тип интерфейса.</typeparam>
        /// <typeparam name="TQuery">Тип запроса.</typeparam>
        /// <param name="provider">Провайдер.</param>
        /// <param name="query">Запрос.</param>
        /// <returns>Модуль.</returns>
        public static T QueryModule<T, TQuery>(this IModuleProvider provider, TQuery query)
            where T : class
        {
            var obj = provider.QueryModule(typeof(T), query);
            return obj as T ?? obj?.QueryView<T>();
        }

        /// <summary>
        /// Запросить модуль асинхронно.
        /// </summary>
        /// <typeparam name="T">Тип интерфейса.</typeparam>
        /// <typeparam name="TQuery">Тип запроса.</typeparam>
        /// <param name="provider">Провайдер.</param>
        /// <param name="query">Запрос.</param>
        /// <returns>Модуль.</returns>
        public static async Task<T> QueryModuleAsync<T, TQuery>(this IModuleProvider provider, TQuery query)
            where T : class
        {
            var obj = await provider.QueryModuleAsync(typeof(T), query);
            return obj as T ?? obj?.QueryView<T>();
        }
    }
}