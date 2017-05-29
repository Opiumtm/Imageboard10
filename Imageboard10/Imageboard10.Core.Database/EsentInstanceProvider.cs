using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Tasks;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Провайдер экземпляров ESENT.
    /// </summary>
    public class EsentInstanceProvider : ModuleBase<IEsentInstanceProvider>, IEsentInstanceProvider, IEsentInstanceProviderForTests, IDisposeWaiters
    {
        private readonly bool _clearDbOnStart;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="clearDbOnStart">Удалять содержимое базы при старте.</param>
        public EsentInstanceProvider(bool clearDbOnStart)
            :base(true, false)
        {
            _clearDbOnStart = true;
        }

        /// <summary>
        /// Запросить представление модуля.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        /// <returns>Представление.</returns>
        public override object QueryView(Type viewType)
        {
            if (viewType == typeof(IEsentInstanceProviderForTests))
            {
                return this;
            }
            return base.QueryView(viewType);
        }

        /// <summary>
        /// Получить директорию.
        /// </summary>
        /// <param name="purge">Удалить содержимое.</param>
        /// <returns>Директория.</returns>
        public async Task<StorageFolder> GetDirectory(bool purge)
        {
            if (purge)
            {
                var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "esent");
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            var indexFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("esent", CreationCollisionOption.OpenIfExists);
            return indexFolder;
        }

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            DatabasePath = (await GetDirectory(_clearDbOnStart)).Path;
            await CreateCachedInstance();
            return Nothing.Value;
        }

        /// <summary>
        /// Действие по вовозбновлению работы.
        /// </summary>
        protected override async ValueTask<Nothing> OnResumed()
        {
            await base.OnResumed();
            await CreateCachedInstance();
            Interlocked.Exchange(ref _isSuspended, 0);
            Interlocked.Exchange(ref _isSuspendRequested, 0);
            return Nothing.Value;
        }

        /// <summary>
        /// Действие по завершению работы.
        /// </summary>
        protected override async ValueTask<Nothing> OnDispose()
        {
            await base.OnDispose();
            await WaitDisposed();
            return Nothing.Value;
        }

        /// <summary>
        /// Действие по приостановке работы.
        /// </summary>
        protected override async ValueTask<Nothing> OnSuspended()
        {
            Interlocked.Exchange(ref _isSuspendRequested, 1);
            await base.OnSuspended();
            await WaitDisposed();
            Interlocked.Exchange(ref _isSuspended, 1);
            return Nothing.Value;
        }

        private IEsentSession _cachedSession;

        private async Task CreateCachedInstance()
        {
            var dispatcher = new SingleThreadDispatcher();
            try
            {
                // ReSharper disable once AccessToDisposedClosure
                var session = await dispatcher.QueueAction(() => CreateInstance(dispatcher));
                var old = Interlocked.Exchange(ref _cachedSession, session.session);
                if (old is EsentSession s)
                {
                    await s.DisposeInternal();
                }
            }
            catch
            {
                dispatcher.Dispose();
                throw;
            }
        }

        private string GetEdbFilePath() => Path.Combine(DatabasePath, "database.edb");

        private (IEsentSession session, bool isCreated) CreateInstance(SingleThreadDispatcher dispatcher)
        {
            var databasePath = GetEdbFilePath();
            var instance = DoCreateInstance();
            IEsentSession result;
            bool isCreated = false;
            if (!File.Exists(databasePath))
            {
                var session = new Session(instance);
                try
                {
                    JET_DBID database;

                    Api.JetCreateDatabase(session, databasePath, null, out database, CreateDatabaseGrbit.None);
                    result = new EsentSession(instance, session, database, databasePath, this, dispatcher, false);
                    isCreated = true;
                }
                catch
                {
                    session.Dispose();
                    instance.Dispose();
                    throw;
                }
            }
            else
            {
                var session = new Session(instance);
                try
                {
                    JET_DBID database;

                    Api.JetAttachDatabase(session, databasePath, AttachDatabaseGrbit.None);
                    Api.OpenDatabase(session, databasePath, out database, OpenDatabaseGrbit.None);
                    result = new EsentSession(instance, session, database, databasePath, this, dispatcher, false);
                }
                catch
                {
                    session.Dispose();
                    instance.Dispose();
                    throw;
                }
            }
            return (result, isCreated);
        }

        private Instance DoCreateInstance()
        {
            var instance = new Instance("globalInstance")
            {
                Parameters =
                {
                    CreatePathIfNotExist = true,
                    TempDirectory = Path.Combine(DatabasePath, "temp"),
                    SystemDirectory = Path.Combine(DatabasePath, "system"),
                    LogFileDirectory = Path.Combine(DatabasePath, "logs"),
                    Recovery = true,
                    CircularLog = true,
                    LogFileSize = 1024,
                },
            };
            instance.Init();
            Interlocked.Increment(ref _instancesCreated);
            return instance;
        }

        private readonly HashSet<Task> _waitingDisposed = new HashSet<Task>();

        private TimeSpan _shutdownTimeout = TimeSpan.FromSeconds(5);

        private async ValueTask<Nothing> WaitDisposed()
        {
            var session = Interlocked.Exchange(ref _cachedSession, null);
            Task[] toWait;
            lock (_waitingDisposed)
            {
                toWait = _waitingDisposed.ToArray();
                _waitingDisposed.Clear();
            }
            if (toWait.Length > 0)
            {
                var waiter = new Task[]
                {
                    Task.WhenAll(toWait),
                    Task.Delay(_shutdownTimeout)
                };
                var task = await Task.WhenAny(waiter);
                LastShutdownTimeout = !ReferenceEquals(task, waiter[0]);
            }
            if (session is EsentSession s)
            {
                await s.DisposeInternal();
            }
            return Nothing.Value;
        }

        /// <summary>
        /// Основная сессия. Не вызывать Dispose(), т.к. временем жизни основной сессии управляет провайдер.
        /// </summary>
        public IEsentSession MainSession => Interlocked.CompareExchange(ref _cachedSession, null, null) ?? throw new InvalidOperationException("Нет активной сессии ESENT");

        /// <summary>
        /// Получить сессию только для чтения.
        /// </summary>
        /// <returns>Экземпляр.</returns>
        public Task<IEsentSession> CreateReadOnlySession()
        {
            return EsentSession.CreateReadOnlySession(MainSession, this);
        }

        /// <summary>
        /// Получить сессию только для чтения, вызовы к которой строго должны производиться из одного потока.
        /// </summary>
        /// <returns></returns>
        public IEsentSession CreateThreadUnsafeReadOnlySession()
        {
            return EsentSession.CreateThreadUnsafeReadOnlySession(MainSession, this);
        }

        /// <summary>
        /// Путь к базе данных.
        /// </summary>
        public string DatabasePath { get; private set; }

        private sealed class EsentSession : IEsentSession
        {
            private readonly string _databasePath;
            private readonly IDisposeWaiters _waiters;
            private readonly IDisposable _defaultUsage;

            public static async Task<IEsentSession> CreateReadOnlySession(IEsentSession parentSession, IDisposeWaiters waiters)
            {
                var dispatcher = new SingleThreadDispatcher();
                try
                {
                    return await dispatcher.QueueAction(() =>
                    {
                        var session = new Session(parentSession.Instance);
                        try
                        {
                            Api.JetAttachDatabase(session, parentSession.DatabaseFile, AttachDatabaseGrbit.ReadOnly);
                            JET_DBID dbid;
                            Api.JetOpenDatabase(session, parentSession.DatabaseFile, string.Empty, out dbid, OpenDatabaseGrbit.ReadOnly);
                            // ReSharper disable once AccessToDisposedClosure
                            return new EsentSession(parentSession.Instance, session, dbid, parentSession.DatabaseFile, waiters, dispatcher, true);
                        }
                        catch
                        {
                            session.Dispose();
                            throw;
                        }
                    });
                }
                catch
                {
                    dispatcher.Dispose();
                    throw;
                }
            }

            public static IEsentSession CreateThreadUnsafeReadOnlySession(IEsentSession parentSession, IDisposeWaiters waiters)
            {
                var session = new Session(parentSession.Instance);
                try
                {
                    Api.JetAttachDatabase(session, parentSession.DatabaseFile, AttachDatabaseGrbit.ReadOnly);
                    JET_DBID dbid;
                    Api.JetOpenDatabase(session, parentSession.DatabaseFile, string.Empty, out dbid, OpenDatabaseGrbit.ReadOnly);
                    return new EsentSession(parentSession.Instance, session, dbid, parentSession.DatabaseFile, waiters, null, true);
                }
                catch
                {
                    session.Dispose();
                    throw;
                }
            }

            public EsentSession(Instance instance, Session session, JET_DBID dbid, string databasePath, IDisposeWaiters waiters, SingleThreadDispatcher dispatcher, bool isReadOnly)
            {
                Instance = instance;
                _databasePath = databasePath;
                _waiters = waiters;
                _session = session;
                _database = dbid;
                IsReadOnly = isReadOnly;
                _dispatcher = dispatcher;
                if (isReadOnly)
                {
                    _defaultUsage = new SessionDisposeWaiter(_waiters);
                }
            }

            public ValueTask<Nothing> DisposeInternal()
            {
                Nothing Do()
                {
                    try
                    {
                        Api.JetCloseDatabase(Session, Database, CloseDatabaseGrbit.None);
                        if (!IsReadOnly)
                        {
                            Api.JetDetachDatabase(Session, _databasePath);
                            Instance.Dispose();
                        }
                    }
                    finally 
                    {
                        _dispatcher?.Dispose();
                        _defaultUsage?.Dispose();
                    }
                    return Nothing.Value;
                }


                if (_dispatcher == null)
                {
                    return new ValueTask<Nothing>(Do());
                }
                return new ValueTask<Nothing>(_dispatcher.QueueAction(Do));
            }

            public void Dispose()
            {
                if (!IsReadOnly)
                {
                    throw new InvalidOperationException("Нельзя вручную завершать основную сессию ESENT");
                }

                async void Do()
                {
                    try
                    {
                        await DisposeInternal();
                    }
                    catch
                    {
                    }
                }
                Do();
            }

            public Instance Instance { get; }

            public string DatabaseFile => _databasePath;

            public bool IsReadOnly { get; }

            private readonly Session _session;

            /// <summary>
            /// Сессия.
            /// </summary>
            public Session Session => _dispatcher?.CheckAccess(() => _session) ?? _session;

            private readonly JET_DBID _database;

            /// <summary>
            /// База данных.
            /// </summary>
            public JET_DBID Database => _dispatcher?.CheckAccess(() => _database) ?? _database;

            private readonly SingleThreadDispatcher _dispatcher;

            public ValueTask<Nothing> RunInTransaction(Func<bool> logic)
            {                
                if (logic == null)
                {
                    return new ValueTask<Nothing>(Nothing.Value);
                }

                Nothing Do()
                {
                    using (var transaction = new Transaction(Session))
                    {
                        if (logic())
                        {
                            transaction.Commit(CommitTransactionGrbit.LazyFlush);
                        }
                    }
                    return Nothing.Value;
                }

                if (_dispatcher == null)
                {
                    return new ValueTask<Nothing>(Do());
                }
                return new ValueTask<Nothing>(_dispatcher.QueueAction(Do));
            }

            /// <summary>
            /// Выполнить вне транзакции.
            /// </summary>
            /// <param name="logic">Логика.</param>
            public ValueTask<Nothing> Run(Action logic)
            {
                if (logic == null)
                {
                    return new ValueTask<Nothing>(Nothing.Value);
                }

                Nothing Do()
                {
                    logic();
                    return Nothing.Value;
                }

                if (_dispatcher == null)
                {
                    return new ValueTask<Nothing>(Do());
                }
                return new ValueTask<Nothing>(_dispatcher.QueueAction(Do));
            }

            public IDisposable UseSession()
            {
                return new SessionDisposeWaiter(_waiters);
            }

            /// <summary>
            /// Открыть таблицу.
            /// </summary>
            /// <param name="tableName">Имя таблицы.</param>
            /// <param name="grbit">Биты.</param>
            /// <returns>Таблица.</returns>
            public EsentTable OpenTable(string tableName, OpenTableGrbit grbit)
            {
                if (tableName == null) throw new ArgumentNullException(nameof(tableName));
                if (_dispatcher != null)
                {
                    _dispatcher.CheckAccess();
                }
                JET_TABLEID tableid;
                Api.OpenTable(_session, _database, tableName, grbit, out tableid);
                return new EsentTable(_session, tableid);
            }

            private class SessionDisposeWaiter : IDisposable
            {
                private readonly IDisposeWaiters _waiters;
                private readonly TaskCompletionSource<bool> tcs;
                private int _isDisposed;

                public SessionDisposeWaiter(IDisposeWaiters waiters)
                {
                    _waiters = waiters;
                    tcs = new TaskCompletionSource<bool>();
                    var task = tcs.Task;
                    task.ContinueWith((t, o) =>
                    {
                        _waiters.RemoveWaiter(t);
                    }, null);
                    _waiters.RegisterWaiter(task);
                }

                public void Dispose()
                {
                    if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                tcs.SetResult(true);
                            }
                            catch
                            {
                            }
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Получить путь к папке базы данных.
        /// </summary>
        /// <returns>Путь к папке базы данных.</returns>
        async Task<string> IEsentInstanceProviderForTests.GetDatabaseFolder()
        {
            return (await GetDirectory(false)).Path;
        }

        /// <summary>
        /// Таймаут последнего завершения.
        /// </summary>
        private bool LastShutdownTimeout { get; set; }

        /// <summary>
        /// Таймаут последнего завершения.
        /// </summary>
        bool IEsentInstanceProviderForTests.LastShutdownTimeout => this.LastShutdownTimeout;

        /// <summary>
        /// Установить таймаут завершения.
        /// </summary>
        /// <param name="timeout">Таймаут.</param>
        void IEsentInstanceProviderForTests.SetShutdownTimeout(TimeSpan timeout) => _shutdownTimeout = timeout;

        private int _instancesCreated;

        /// <summary>
        /// Инстансов создано.
        /// </summary>
        int IEsentInstanceProviderForTests.InstancesCreated => Interlocked.CompareExchange(ref _instancesCreated, 0, 0);

        private int _isSuspended;

        /// <summary>
        /// Работа приостановлена.
        /// </summary>
        bool IEsentInstanceProviderForTests.IsSuspended => Interlocked.CompareExchange(ref _isSuspended, 0, 0) != 0;

        private int _isSuspendRequested;

        /// <summary>
        /// Остановка запрошена.
        /// </summary>
        bool IEsentInstanceProviderForTests.IsSuspendRequested => Interlocked.CompareExchange(ref _isSuspendRequested, 0, 0) != 0;

        /// <summary>
        /// Зарегистрировать использование.
        /// </summary>
        /// <param name="task">Таск.</param>
        void IDisposeWaiters.RegisterWaiter(Task task)
        {
            lock (_waitingDisposed)
            {
                _waitingDisposed.Add(task);
            }
        }

        /// <summary>
        /// Удалить использование.
        /// </summary>
        /// <param name="task">Таск.</param>
        void IDisposeWaiters.RemoveWaiter(Task task)
        {
            lock (_waitingDisposed)
            {
                _waitingDisposed.Remove(task);
            }
        }
    }
}