using System.Threading.Tasks;
using Imageboard10.Core.Modules.Wrappers;

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
        public static ModuleInterface.IModuleProvider AsWinRTProvider<T>(this T provider)
            where T : IModuleProvider
        {
            if (provider == null)
            {
                return null;
            }
            return new ModuleProviderWrapperToWinRT<T>(provider);
        }

        /// <summary>
        /// Получить интерфейс провайдера в виде .NET.
        /// </summary>
        /// <typeparam name="T">Тип провайдера.</typeparam>
        /// <param name="provider">Провайдер.</param>
        /// <returns>Совместимый с .NET-библиотекой провайдер.</returns>
        public static IModuleProvider AsDotnetProvider<T>(this T provider)
            where T : ModuleInterface.IModuleProvider
        {
            if (provider == null)
            {
                return null;
            }
            return new ModuleProviderWrapperToDotnet<T>(provider);
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
            return new ModuleCollectionWrapperToWinRT<T>(collection);
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
            return new ModuleCollectionWrapperToDotnet<T>(collection);
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
            return new ModuleWrapperToWinRT<T>(module);
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
            return new ModuleWrapperToDontnet<T>(module);
        }

        /// <summary>
        /// Получить интерфейс времени жизни модуля в виде WinRT.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="lifetime">Исходный объект.</param>
        /// <returns>Совместимый с WinRT объект.</returns>
        // ReSharper disable once InconsistentNaming
        public static ModuleInterface.IModuleLifetime AsWinRTModuleLifetime<T>(this T lifetime)
            where T : IModuleLifetime
        {
            if (lifetime == null)
            {
                return null;
            }
            return new ModuleLifetimeWrapperToWinRt<T>(lifetime);
        }

        /// <summary>
        /// Получить интерфейс времени жизни модуля в виде .NET.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="lifetime">Исходный объект.</param>
        /// <returns>Совместимый с .NET-библиотекой объект.</returns>
        public static IModuleLifetime AsDotnetModuleLifetime<T>(this T lifetime)
            where T : ModuleInterface.IModuleLifetime
        {
            if (lifetime == null)
            {
                return null;
            }
            return new ModuleLifetimeWrapperToDotnet<T>(lifetime);
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

        /// <summary>
        /// Зарегистрировать статический модуль.
        /// </summary>
        /// <typeparam name="T">Тип модуля.</typeparam>
        /// <typeparam name="TIntf">Тип интерфейса, который реализует модуль.</typeparam>
        /// <param name="collection">Коллекция.</param>
        /// <param name="module">Модуль.</param>
        /// <param name="filter">Фильтр модуля.</param>
        public static void RegisterModule<T, TIntf>(this IModuleCollection collection, T module, IStaticModuleQueryFilter filter = null)
            where T : IModule, TIntf
        {
            collection.RegisterProvider(typeof(TIntf), new StaticModuleProvider<T, TIntf>(module, filter));
        }
    }
}