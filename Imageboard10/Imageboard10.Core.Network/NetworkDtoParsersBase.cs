using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Network
{
    /// <summary>
    /// Базовый класс для набора парсеров Dto.
    /// </summary>
    public abstract class NetworkDtoParsersBase : ModuleBase<INetworkDtoParsers>, INetworkDtoParsers, IStaticModuleQueryFilter
    {
        private readonly HashSet<Type> _dtoParserTypes;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        protected NetworkDtoParsersBase()
            :base(false, false)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            _dtoParserTypes = new HashSet<Type>(GetDtoParsersTypes());
        }

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        public override object QueryView(Type viewType)
        {
            if (_dtoParserTypes.Contains(viewType))
            {
                return this;
            }
            return base.QueryView(viewType);
        }

        /// <summary>
        /// Получить поддерживаемые типы парсеров Dto.
        /// </summary>
        /// <returns>Поддерживаемые типы парсеров Dto.</returns>
        protected abstract IEnumerable<Type> GetDtoParsersTypes();

        /// <summary>
        /// Проверить запрос.
        /// </summary>
        /// <typeparam name="T">Тип запроса.</typeparam>
        /// <param name="query">Запрос.</param>
        /// <returns>Результат.</returns>
        public bool CheckQuery<T>(T query)
        {
            var t = query as Type;
            if (t != null)
            {
                return _dtoParserTypes.Contains(t);
            }
            return false;
        }
    }
}