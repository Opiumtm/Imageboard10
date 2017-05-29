using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Imageboard10.Core.Database;
using Imageboard10.Core.ModelInterface.Links;
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
    public abstract class ModelStorageBase<TIntf> : ModuleBase<TIntf> where TIntf : class
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
        /// Провайдер ESENT.
        /// </summary>
        protected IEsentInstanceProvider EsentProvider { get; private set; }

        /// <summary>
        /// Сериализация ссылок.
        /// </summary>
        protected ILinkSerializationService LinkSerialization { get; private set; }

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
            EsentProvider = await moduleProvider.QueryModuleAsync<IEsentInstanceProvider>();
            LinkSerialization = await moduleProvider.QueryModuleAsync<ILinkSerializationService>();
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
                await UpdateAsync(EnsureTableversion);

                async ValueTask<Nothing> DoUpdate()
                {
                    void SignalUpdateComplete()
                    {
                        _upgradeTcs.SetResult(Nothing.Value);
                    }

                    await CreateOrUpgradeTables();
                    CoreTaskHelper.RunUnawaitedTask(SignalUpdateComplete);
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
        /// Запросить данные базы в режиме read-only.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="logic">Логика.</param>
        /// <returns>Результат.</returns>
        protected async Task<T> QueryReadonly<T>(Func<IEsentSession, Task<T>> logic)
        {
            if (logic == null)
            {
                return default(T);
            }
            CheckModuleReady();
            using (var readonlySession = await EsentProvider.CreateReadOnlySession())
            {
                return await logic(readonlySession);
            }
        }

        /// <summary>
        /// Запросить данные базы в режиме read-only. Обращения к базе должны производиться из одного потока.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="logic">Логика.</param>
        /// <returns>Результат.</returns>
        protected T QueryReadonlyThreadUnsafe<T>(Func<IEsentSession, T> logic)
        {
            if (logic == null)
            {
                return default(T);
            }
            CheckModuleReady();
            using (var readonlySession = EsentProvider.CreateThreadUnsafeReadOnlySession())
            {
                return logic(readonlySession);
            }
        }

        /// <summary>
        /// Асинхронно запросить данные базы в режиме read-only. Обращения к базе должны производиться из одного потока.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="logic">Логика.</param>
        /// <returns>Результат.</returns>
        protected Task<T> QueryReadonlyThreadUnsafeAsync<T>(Func<IEsentSession, T> logic)
        {
            return Task.Factory.StartNew(() => QueryReadonlyThreadUnsafe(logic));
        }

        /// <summary>
        /// Обновить данные в базе.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="logic">Логика.</param>
        /// <returns>Результат.</returns>
        protected async Task<T> UpdateAsync<T>(Func<IEsentSession, Task<T>> logic)
        {
            if (logic == null)
            {
                return default(T);
            }
            CheckModuleReady();
            var mainSession = EsentProvider.MainSession;
            using (mainSession.UseSession())
            {
                return await logic(mainSession);
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
        protected async Task<int> EnsureTable(string tableName, int currentVersion, Action<IEsentSession, JET_TABLEID> initialize, Action<IEsentSession> update, bool bypassCache = false)
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

            async Task<int> Do(IEsentSession session)
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
                            Api.JetPrepareUpdate(sid, tvid, JET_prep.Replace);
                            Api.SetColumn(sid, tvid, verid, currentVersion);
                            Api.JetUpdate(sid, tvid);
                        }
                        else
                        {
                            Api.JetPrepareUpdate(sid, tvid, JET_prep.Insert);
                            Api.SetColumn(sid, tvid, iid, tableName, Encoding.Unicode);
                            Api.SetColumn(sid, tvid, verid, currentVersion);
                            Api.JetUpdate(sid, tvid);
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

            var newVersion =  await UpdateAsync(Do);

            lock (_versionCache)
            {
                _versionCache[tableName] = newVersion;
            }

            return newVersion;
        }

        private async Task<Nothing> EnsureTableversion(IEsentSession session)
        {
            await session.RunInTransaction(() =>
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
            return Nothing.Value;
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
    }
}