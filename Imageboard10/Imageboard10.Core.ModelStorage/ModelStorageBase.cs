using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.Models.Serialization;
using Imageboard10.Core.ModelStorage.Boards;
using Imageboard10.Core.ModelStorage.UnitTests;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Tasks;
using Imageboard10.Core.Utility;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.ModelStorage
{
    /// <summary>
    /// Базовый класс хранилища данных модели.
    /// </summary>
    /// <typeparam name="TIntf">Тип интерфейса.</typeparam>
    public abstract class ModelStorageBase<TIntf> : ModuleBase<TIntf>, IModelStorageForTests where TIntf : class
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        protected ModelStorageBase()
            :base(true, false)
        {
            _upgradeTcs = new TaskCompletionSource<Nothing>();
            _upgradeWaitTask = _upgradeTcs.Task;
        }

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        public override object QueryView(Type viewType)
        {
            if (viewType == typeof(IModelStorageForTests))
            {
                return this;
            }
            return base.QueryView(viewType);
        }

        /// <summary>
        /// Провайдер ESENT.
        /// </summary>
        protected IEsentInstanceProvider EsentProvider { get; private set; }

        /// <summary>
        /// Сериализация ссылок.
        /// </summary>
        protected ILinkSerializationService LinkSerialization { get; private set; }

        /// <summary>
        /// Сериализация объектов.
        /// </summary>
        protected IObjectSerializationService ObjectSerializationService { get; private set; }

        /// <summary>
        /// Глобальная сигнализация об ошибках.
        /// </summary>
        protected ModuleInterface.IGlobalErrorHandler GlobalErrorHandler { get; private set; }

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            EsentProvider = await moduleProvider.QueryModuleAsync<IEsentInstanceProvider>() ?? throw new ModuleNotFoundException(typeof(IEsentInstanceProvider));
            LinkSerialization = await moduleProvider.QueryModuleAsync<ILinkSerializationService>() ?? throw new ModuleNotFoundException(typeof(ILinkSerializationService));
            ObjectSerializationService = await moduleProvider.QueryModuleAsync<IObjectSerializationService>() ?? throw new ModuleNotFoundException(typeof(IObjectSerializationService));
            GlobalErrorHandler = await moduleProvider.QueryModuleAsync<ModuleInterface.IGlobalErrorHandler>();
            return Nothing.Value;
        }

        private readonly TaskCompletionSource<Nothing> _upgradeTcs;
        private readonly Task _upgradeWaitTask;

        /// <summary>
        /// Все модули инициализированы.
        /// </summary>
        protected override async ValueTask<Nothing> OnAllInitialized()
        {
            await base.OnAllInitialized();
            try
            {
                ValueTask<Nothing> DoEnsureTableversion()
                {
                    return InMainSessionAsync(EnsureTableversion);
                }

                async ValueTask<Nothing> DoUpdate()
                {
                    void SignalUpdateComplete()
                    {
                        _upgradeTcs.SetResult(Nothing.Value);
                    }

                    void SignalUpdateError(Exception ex)
                    {
                        _upgradeTcs.SetException(ex);
                    }

                    try
                    {
                        await TableVersionStatus.Instance.InitializeTableVersionOnce(DoEnsureTableversion);
                        await CreateOrUpgradeTables();
                        CoreTaskHelper.RunUnawaitedTask(SignalUpdateComplete);
                    }
                    catch (Exception ex)
                    {
                        CoreTaskHelper.RunUnawaitedTask(() => SignalUpdateError(ex));
                    }

                    return Nothing.Value;
                }

                CoreTaskHelper.RunUnawaitedTaskAsync2(DoUpdate);
            }
            catch (Exception ex)
            {
                GlobalErrorHandler?.SignalError(ex);
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Ждать завершения инициализации таблиц.
        /// </summary>
        protected Task WaitForTablesInitialize()
        {
            return _upgradeWaitTask;
        }

        /// <summary>
        /// Создать или обновить таблицы.
        /// </summary>
        protected virtual ValueTask<Nothing> CreateOrUpgradeTables()
        {
            return new ValueTask<Nothing>(Nothing.Value);
        }

        /// <summary>
        /// Запросить данные базы.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="logic">Логика.</param>
        /// <returns>Результат.</returns>
        protected async ValueTask<T> OpenSessionAsync<T>(Func<IEsentSession, ValueTask<T>> logic)
        {
            if (logic == null)
            {
                return default(T);
            }
            CheckModuleReady();
            var readonlySession = await EsentProvider.GetSecondarySessionAndUse();
            using (readonlySession.usage)
            {
                return await logic(readonlySession.session);
            }
        }

        /// <summary>
        /// Асинхронно запросить данные базы. Обращения к базе должны производиться из одного потока.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="logic">Логика.</param>
        /// <returns>Результат.</returns>
        protected async ValueTask<T> OpenSession<T>(Func<IEsentSession, T> logic)
        {
            if (logic == null)
            {
                return default(T);
            }
            CheckModuleReady();
            var readonlySession = await EsentProvider.GetSecondarySessionAndUse();
            using (readonlySession.usage)
            {
                T result = default(T);
                await readonlySession.session.Run(() =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    result = logic(readonlySession.session);
                });
                return result;
            }
        }

        /// <summary>
        /// Обновить данные в базе.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="logic">Логика.</param>
        /// <returns>Результат.</returns>
        protected ValueTask<T> InMainSessionAsync<T>(Func<IEsentSession, ValueTask<T>> logic)
        {
            if (logic == null)
            {
                return new ValueTask<T>();
            }
            CheckModuleReady();
            var mainSession = EsentProvider.MainSession;
            using (mainSession.UseSession())
            {
                return logic(mainSession);
            }
        }

        /// <summary>
        /// Обновить данные в базе.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="logic">Логика.</param>
        /// <returns>Результат.</returns>
        protected ValueTask<T> InMainSession<T>(Func<IEsentSession, T> logic)
        {
            if (logic == null)
            {
                return new ValueTask<T>(default(T));
            }
            CheckModuleReady();
            var mainSession = EsentProvider.MainSession;
            using (mainSession.UseSession())
            {
                return new ValueTask<T>(logic(mainSession));
            }
        }

        /// <summary>
        /// Параллельная обработка перечисления на разных сессиях.
        /// </summary>
        /// <typeparam name="TSrc">Тип данных.</typeparam>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="src">Перечисление.</param>
        /// <param name="parallelFunc">Функция обоработки.</param>
        /// <returns>Результат.</returns>
        protected async ValueTask<T[]> ParallelizeOnSessions<TSrc, T>(IEnumerable<TSrc> src, Func<IEsentSession, TSrc, ValueTask<T>> parallelFunc)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (parallelFunc == null) throw new ArgumentNullException(nameof(parallelFunc));
            var tasks = new List<ValueTask<T>>();
            var toDispose = new CompositeDisposable(null);
            try
            {
                foreach (var el in src)
                {
                    var session = await EsentProvider.GetSecondarySessionAndUse();
                    toDispose.AddDisposable(session.usage);
                    tasks.Add(parallelFunc(session.session, el));
                }
                return await CoreTaskHelper.WhenAllValueTasks(tasks);
            }
            finally
            {
                toDispose.Dispose();
            }
        }

        /// <summary>
        /// Параллельная обработка перечисления на разных сессиях.
        /// </summary>
        /// <typeparam name="TSrc">Тип данных.</typeparam>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="src">Перечисление.</param>
        /// <param name="parallelFunc">Функция обоработки.</param>
        /// <returns>Результат.</returns>
        protected async ValueTask<T[]> ParallelizeOnSessions<TSrc, T>(IEnumerable<TSrc> src, Func<IEsentSession, TSrc, T> parallelFunc)
        {
            ValueTask<T> Do(IEsentSession session, TSrc el)
            {
                return session.Run(() => parallelFunc(session, el));
            }

            if (src == null) throw new ArgumentNullException(nameof(src));
            if (parallelFunc == null) throw new ArgumentNullException(nameof(parallelFunc));
            var tasks = new List<ValueTask<T>>();
            var toDispose = new CompositeDisposable(null);
            try
            {
                foreach (var el in src)
                {
                    var session = await EsentProvider.GetSecondarySessionAndUse();
                    toDispose.AddDisposable(session.usage);
                    tasks.Add(Do(session.session, el));
                }
                return await CoreTaskHelper.WhenAllValueTasks(tasks);
            }
            finally
            {
                toDispose.Dispose();
            }
        }

        /// <summary>
        /// Найти таблицу.
        /// </summary>
        /// <param name="session">Сессия.</param>
        /// <param name="database">База данных.</param>
        /// <param name="tableName">Имя таблицы.</param>
        /// <param name="grbit">Флаги.</param>
        /// <returns>Идентификатор таблицы или NULL.</returns>
        protected JET_TABLEID? FindTable(Session session, JET_DBID database, string tableName, OpenTableGrbit grbit = OpenTableGrbit.None)
        {
            JET_TABLEID tableid;
            Api.TryOpenTable(session, database, tableName, grbit, out tableid);
            return tableid;
        }

        /// <summary>
        /// Имя таблицы с версиями таблиц.
        /// </summary>
        protected const string TableVersionTable = "_tableversion";

        /// <summary>
        /// Идентификатор.
        /// </summary>
        protected const string TableVersionIdColumn = "Id";

        /// <summary>
        /// Версия.
        /// </summary>
        protected const string TableVersionVersionColumn = "Version";

        protected const string TableVersionPkIndex = "PK_" + TableVersionTable;

        private readonly Dictionary<string, int> _versionCache = new Dictionary<string, int>();

        /// <summary>
        /// Проверить версию таблицы и создать таблицу (если её нет) или обновить (если версия не совпадает).
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        /// <param name="currentVersion">Текущая версия.</param>
        /// <param name="initialize">Метод инициализации. Если null и таблица не существует, то метод вернёт 0 в качестве версии таблицы.</param>
        /// <param name="update">Метод обновления. Если null, то обновление версии производиться не будет, вместо этого метод вернёт актуальную версию таблицы.</param>
        /// <param name="bypassCache">Не использовать кэш версий.</param>
        /// <returns>Версия таблицы.</returns>
        protected async ValueTask<int> EnsureTable(string tableName, int currentVersion, Action<IEsentSession, JET_TABLEID> initialize, Action<IEsentSession> update, bool bypassCache = false)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (tableName.Length > 125)
            {
                throw new ArgumentException("Имя таблицы длиннее 125 символов", nameof(tableName));
            }
            if (currentVersion < 1)
            {
                throw new InvalidOperationException("Номер версии таблицы должен быть 1 или выше");
            }

            async ValueTask<int> Do(IEsentSession session)
            {
                bool found = false;
                await session.Run(() =>
                {
                    var sid = session.Session;
                    var dbid = session.Database;
                    JET_TABLEID tableid;
                    if (Api.TryOpenTable(sid, dbid, tableName, OpenTableGrbit.ReadOnly, out tableid))
                    {
                        found = true;
                        Api.JetCloseTable(sid, tableid);
                    }
                });
                if (!found)
                {
                    if (initialize == null)
                    {
                        return 0;
                    }
                    await session.RunInTransaction(() =>
                    {
                        var sid = session.Session;
                        var dbid = session.Database;
                        JET_TABLEID tableid;
                        Api.JetCreateTable(sid, dbid, tableName, 1, 100, out tableid);
                        try
                        {
                            initialize(session, tableid);
                        }
                        finally
                        {
                            Api.JetCloseTable(sid, tableid);
                        }
                        return true;
                    });
                }
                else
                {
                    if (currentVersion == 1)
                    {
                        return currentVersion;
                    }
                    int version = 0;
                    await session.Run(() =>
                    {
                        var sid = session.Session;
                        var dbid = session.Database;
                        JET_TABLEID tvid;
                        Api.OpenTable(sid, dbid, TableVersionTable, OpenTableGrbit.ReadOnly, out tvid);
                        try
                        {
                            Api.MakeKey(sid, tvid, tableName, Encoding.Unicode, MakeKeyGrbit.NewKey);
                            if (!Api.TrySeek(sid, tvid, SeekGrbit.SeekEQ))
                            {
                                version = 1;
                            }
                            var verid = Api.GetTableColumnid(sid, tvid, TableVersionVersionColumn);
                            version = Api.RetrieveColumnAsInt32(sid, tvid, verid) ?? 0;
                            if (version < 1)
                            {
                                version = 1;
                            }
                        }
                        finally
                        {
                            Api.JetCloseTable(sid, tvid);
                        }
                    });
                    if (version == currentVersion || update == null)
                    {
                        return version;
                    }
                    await session.RunInTransaction(() =>
                    {
                        update(session);
                        return true;
                    });
                }
                await session.RunInTransaction(() =>
                {
                    var sid = session.Session;
                    var dbid = session.Database;
                    JET_TABLEID tvid;
                    Api.OpenTable(sid, dbid, TableVersionTable, OpenTableGrbit.DenyWrite, out tvid);
                    try
                    {
                        var verid = Api.GetTableColumnid(sid, tvid, TableVersionVersionColumn);
                        var iid = Api.GetTableColumnid(sid, tvid, TableVersionIdColumn);
                        Api.MakeKey(sid, tvid, tableName, Encoding.Unicode, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(sid, tvid, SeekGrbit.SeekEQ))
                        {
                            using (var rowUpdate = new Update(sid, tvid, JET_prep.Replace))
                            {
                                Api.SetColumn(sid, tvid, verid, currentVersion);
                                rowUpdate.Save();
                            }
                        }
                        else
                        {
                            using (var rowUpdate = new Update(sid, tvid, JET_prep.Insert))
                            {
                                Api.SetColumn(sid, tvid, iid, tableName, Encoding.Unicode);
                                Api.SetColumn(sid, tvid, verid, currentVersion);
                                rowUpdate.Save();
                            }
                        }
                        return true;
                    }
                    finally
                    {
                        Api.JetCloseTable(sid, tvid);
                    }
                });
                return currentVersion;
            }

            int? cachedVersion = null;

            if (!bypassCache)
            {
                lock (_versionCache)
                {
                    if (_versionCache.ContainsKey(tableName))
                    {
                        cachedVersion = _versionCache[tableName];
                    }
                }
            }

            if (cachedVersion != null)
            {
                return cachedVersion.Value;
            }

            var newVersion =  await InMainSessionAsync(Do);

            lock (_versionCache)
            {
                _versionCache[tableName] = newVersion;
            }

            return newVersion;
        }

        /// <summary>
        /// Удалить таблицу.
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        protected async Task<Nothing> DeleteTable(string tableName)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            return await InMainSessionAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    var sid = session.Session;
                    var dbid = session.Database;
                    using (var table = session.OpenTable(TableVersionTable, OpenTableGrbit.None))
                    {
                        Api.JetDeleteTable(sid, dbid, tableName);
                        Api.MakeKey(table.Session, table, tableName, Encoding.Unicode, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(sid, table, SeekGrbit.SeekEQ))
                        {
                            Api.JetDelete(sid, table);
                        }
                        return true;
                    }
                });
                return Nothing.Value;
            });
        }

        private ValueTask<Nothing> EnsureTableversion(IEsentSession session)
        {
            return session.RunInTransaction(() =>
            {
                var sid = session.Session;
                var dbid = session.Database;
                JET_TABLEID tvid;
                if (Api.TryOpenTable(sid, dbid, TableVersionTable, OpenTableGrbit.ReadOnly, out tvid))
                {
                    Api.JetCloseTable(sid, tvid);
                    return false;
                }
                Api.JetCreateTable(sid, dbid, TableVersionTable, 1, 100, out tvid);
                try
                {
                    JET_COLUMNID colid, colval;
                    Api.JetAddColumn(sid, tvid, TableVersionIdColumn, new JET_COLUMNDEF()
                    {
                        coltyp = JET_coltyp.Text,
                        cbMax = 125,
                        grbit = ColumndefGrbit.ColumnNotNULL,
                        cp = JET_CP.Unicode
                    }, null, 0, out colid);
                    Api.JetAddColumn(sid, tvid, TableVersionVersionColumn, new JET_COLUMNDEF()
                    {
                        coltyp = JET_coltyp.Long,
                        grbit = ColumndefGrbit.ColumnNotNULL
                    }, null, 0, out colval);
                    var indexDef = $"+{TableVersionIdColumn}\0\0";
                    Api.JetCreateIndex(sid, tvid, TableVersionPkIndex, CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique, indexDef, indexDef.Length, 100);
                    return true;
                }
                finally
                {
                    Api.JetCloseTable(sid, tvid);
                }
            });
        }

        /// <summary>
        /// Сериализовать контракт данных.
        /// </summary>
        /// <typeparam name="T">Тип контракта.</typeparam>
        /// <param name="obj">Объект.</param>
        /// <returns>Байты.</returns>
        protected byte[] SerializeDataContract<T>(T obj)
        {
            if (obj == null)
            {
                return null;
            }
            var serializer = DataContractSerializerCache.GetSerializer<T>();
            using (var str = new MemoryStream())
            {
                using (var wr = XmlDictionaryWriter.CreateBinaryWriter(str))
                {
                    serializer.WriteObject(wr, obj);
                    wr.Flush();
                }
                return str.ToArray();
            }
        }

        /// <summary>
        /// Десериализовать контракт данных.
        /// </summary>
        /// <typeparam name="T">Тип контракта.</typeparam>
        /// <param name="data">Байты.</param>
        /// <returns>Объект.</returns>
        protected T DeserializeDataContract<T>(byte[] data)
        {
            if (data == null)
            {
                return default(T);
            }
            var serializer = DataContractSerializerCache.GetSerializer<T>();
            using (var str = new MemoryStream(data))
            {
                using (var rd = XmlDictionaryReader.CreateBinaryReader(str, XmlDictionaryReaderQuotas.Max))
                {
                    return (T) serializer.ReadObject(rd);
                }
            }
        }

        /// <summary>
        /// Удалить все записи в таблице.
        /// </summary>
        /// <param name="table">Таблица.</param>
        protected void DeleteAllRows(BoardReferenceTable table)
        {
            table.Indexes.PrimaryIndex.SetAsCurrentIndex();
            Api.JetSetTableSequential(table.Session, table.Table, SetTableSequentialGrbit.None);
            try
            {
                foreach (var _ in table.Enumerate())
                {
                    table.DeleteCurrentRow();
                }
            }
            finally
            {
                Api.JetResetTableSequential(table.Session, table.Table, ResetTableSequentialGrbit.None);
            }
        }

        /// <summary>
        /// Проверить существование таблицы.
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        /// <returns>Результат.</returns>
        ValueTask<bool> IModelStorageForTests.IsTablePresent(string tableName)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            return OpenSession(session =>
            {
                var sid = session.Session;
                var dbid = session.Database;
                if (Api.TryOpenTable(sid, dbid, tableName, OpenTableGrbit.ReadOnly, out var tableid))
                {
                    Api.JetCloseTable(sid, tableid);
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Получить версию таблицы.
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        /// <returns>Версия.</returns>
        ValueTask<int> IModelStorageForTests.GetTableVersion(string tableName)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            return EnsureTable(tableName, int.MaxValue, null, null);
        }

        /// <summary>
        /// Имя таблицы с версиями.
        /// </summary>
        string IModelStorageForTests.TableversionTableName => TableVersionTable;

        /// <summary>
        /// Ожидать инициализации.
        /// </summary>
        Task IModelStorageForTests.WaitForInitialization()
        {
            return WaitForTablesInitialize();
        }

        /// <summary>
        /// Создать индекс.
        /// </summary>
        /// <param name="sid">Сессия.</param>
        /// <param name="tableid">Идентификатор таблицы.</param>
        /// <param name="tableName">Имя таблицы.</param>
        /// <param name="indexName">Имя индекса.</param>
        /// <param name="definition">Описание индекса.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CreateIndex(Session sid, JET_TABLEID tableid, string tableName, string indexName, IndexDefinition definition)
        {
            var def = definition.IndexDef();
            Api.JetCreateIndex(sid, tableid, GetIndexName(tableName, indexName), definition.Grbit, def, def.Length, 100);
        }

        /// <summary>
        /// Получить имя индекса.
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        /// <param name="indexName">Имя индекса.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string GetIndexName(string tableName, string indexName) => $"IX_{tableName}_{indexName}";

        /// <summary>
        /// Получить количество значений в столбце с несколькими значениями.
        /// </summary>
        /// <param name="table">Таблица.</param>
        /// <param name="columnid">Идентификатор столбца.</param>
        /// <returns>Количество значений.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int GetMultiValueCount(EsentTable table, JET_COLUMNID columnid)
        {
            JET_RETRIEVECOLUMN col = new JET_RETRIEVECOLUMN
            {
                columnid = columnid,
                itagSequence = 0
            };
            Api.JetRetrieveColumns(table.Session, table, new[] {col}, 1);
            return col.itagSequence;
        }

        /// <summary>
        /// Очистить столбец с несколькими значениями.
        /// </summary>
        /// <param name="table">Таблица.</param>
        /// <param name="columnid">Идентификатор столбца.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ClearMultiValue(EsentTable table, JET_COLUMNID columnid)
        {
            var cnt = GetMultiValueCount(table, columnid);
            var si = new JET_SETINFO() { itagSequence = 1 };
            for (var i = 0; i < cnt; i++)
            {
                Api.JetSetColumn(table.Session, table.Table, columnid, null, 0, SetColumnGrbit.None, si);
            }
        }

        /// <summary>
        /// Перечислить значения в столбце со многоими значениями.
        /// </summary>
        /// <param name="table">Таблица.</param>
        /// <param name="columnid">Идентификатор столбца.</param>
        /// <param name="factoryFunc">Фабрика создания значений для получения данных.</param>
        /// <returns>Результат.</returns>
        protected IEnumerable<ColumnValue> EnumMultivalueColumn(EsentTable table, JET_COLUMNID columnid, Func<ColumnValue> factoryFunc)
        {
            var count = GetMultiValueCount(table, columnid);
            if (count == 0)
            {
                yield break;
            }

            var a = new ColumnValue[1];
            for (var i = 1; i <= count; i++)
            {
                var col = factoryFunc();
                col.ItagSequence = i;
                col.Columnid = columnid;
                a[0] = col;
                Api.RetrieveColumns(table.Session, table.Table, a);
                yield return col;
            }
        }

        /// <summary>
        /// Перечислить значения в столбце со многоими значениями.
        /// </summary>
        /// <param name="table">Таблица.</param>
        /// <param name="columnid">Идентификатор столбца.</param>
        /// <param name="grbit">Флаг получения.</param>
        /// <returns>Результат.</returns>
        protected IEnumerable<T> EnumMultivalueColumn<T>(EsentTable table, JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None) where T: ColumnValue, new()
        {
            var count = GetMultiValueCount(table, columnid);
            if (count == 0)
            {
                yield break;
            }

            var a = new ColumnValue[1];
            for (var i = 1; i <= count; i++)
            {
                var col = new T
                {
                    ItagSequence = i,
                    Columnid = columnid,
                    RetrieveGrbit = grbit
                };
                a[0] = col;
                Api.RetrieveColumns(table.Session, table.Table, a);
                yield return col;
            }
        }

    }
}