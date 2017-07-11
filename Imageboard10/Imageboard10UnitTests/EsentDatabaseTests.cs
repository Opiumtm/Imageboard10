using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Imageboard10.Core;
using Imageboard10.Core.Database;
using Imageboard10.Core.Database.UnitTests;
using Imageboard10.Core.Modules;
using Imageboard10.Core.Utility;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable AccessToDisposedClosure

namespace Imageboard10UnitTests
{
    [TestClass]
    public class EsentDatabaseTests
    {
        [TestMethod]
        [TestCategory("ESENT")]
        public async Task TestEsentAccess()
        {
            await RunEsentTest(async (provider, testData) =>
            {
                Assert.AreEqual(1, testData.InstancesCreated, "Было создано больше одного инстанса ESENT");
                await SimpleSyncTest(provider, "test_table");
            });
        }

        private Task RunAsyncOnThread(Func<Task> task)
        {
            var tcs = new TaskCompletionSource<bool>();

            async void DoRun()
            {
                try
                {
                    await task();
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }

            Task.Factory.StartNew(DoRun);

            return tcs.Task;
        }

        private Task SimpleSyncTestOnNewThread(IEsentInstanceProvider provider, string tableName)
        {
            var tcs = new TaskCompletionSource<bool>();

            async void DoRun()
            {
                try
                {
                    await SimpleSyncTest(provider, tableName);
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }

            Task.Factory.StartNew(DoRun);

            return tcs.Task;
        }

        private async Task SimpleSyncTest(IEsentInstanceProvider provider, string tableName)
        {
            var mainSession = provider.MainSession;
            using (mainSession.UseSession())
            {
                JET_TABLEID tableid1 = default(JET_TABLEID);
                string pk = null;
                JET_COLUMNID id = default(JET_COLUMNID), val = default(JET_COLUMNID);
                await mainSession.RunInTransaction(() =>
                {
                    var tt = CreateSimpleTable(mainSession, tableName);
                    pk = tt.pk;
                    id = tt.id;
                    val = tt.val;
                    return true;
                });
                await mainSession.Run(() =>
                {
                    Api.OpenTable(mainSession.Session, mainSession.Database, tableName, OpenTableGrbit.DenyWrite, out tableid1);
                });
                try
                {
                    HashSet<string> addedValues = new HashSet<string>();
                    await mainSession.RunInTransaction(() =>
                    {
                        for (var i = 0; i < 1000; i++)
                        {
                            Api.JetPrepareUpdate(mainSession.Session, tableid1, JET_prep.Insert);
                            var vv = "VAL_" + i.ToString();
                            Api.SetColumns(mainSession.Session, tableid1,
                                new StringColumnValue()
                                {
                                    Columnid = val,
                                    Value = vv,
                                });
                            addedValues.Add(vv);
                            Api.JetUpdate(mainSession.Session, tableid1);
                        }
                        return true;
                    });
                    await RunAsyncOnThread(async () =>
                    {
                        var session = await provider.GetSecondarySession();
                        using (session.UseSession())
                        {
                            await session.RunInTransaction(() =>
                            {
                                JET_TABLEID tableid;
                                Api.OpenTable(session.Session, session.Database, tableName, OpenTableGrbit.ReadOnly,
                                    out tableid);
                                try
                                {
                                    Api.JetSetTableSequential(session.Session, tableid, SetTableSequentialGrbit.None);
                                    int count;
                                    Api.TryMoveFirst(session.Session, tableid);
                                    Api.JetIndexRecordCount(session.Session, tableid, out count, int.MaxValue);
                                    Assert.AreEqual(1000, count, "Количество записей в таблице не равно 1000");
                                    Api.MoveBeforeFirst(session.Session, tableid);
                                    while (Api.TryMoveNext(session.Session, tableid))
                                    {
                                        var s = Api.RetrieveColumnAsString(session.Session, tableid, val);
                                        Assert.IsTrue(addedValues.Remove(s),
                                            $"Строка {s} получена из базы, но она не была туда добавлена");
                                    }
                                    return false;
                                }
                                finally
                                {
                                    Api.JetCloseTable(session.Session, tableid);
                                }
                            });
                            Assert.IsTrue(addedValues.Count == 0, "Не все строки были прочитаны из базы");
                        }
                    });
                    {
                        var session = await provider.GetSecondarySession();
                        using (session.UseSession())
                        {
                            JET_TABLEID tableid = default(JET_TABLEID);
                            await session.Run(() =>
                            {
                                Api.OpenTable(session.Session, session.Database, tableName, OpenTableGrbit.ReadOnly,
                                    out tableid);
                            });
                            try
                            {
                                await session.RunInTransaction(() =>
                                {
                                    Api.JetSetTableSequential(session.Session, tableid, SetTableSequentialGrbit.None);
                                    int count;
                                    Api.TryMoveFirst(session.Session, tableid);
                                    Api.JetIndexRecordCount(session.Session, tableid, out count, int.MaxValue);
                                    Assert.AreEqual(1000, count, "Количество записей в таблице не равно 1000");
                                    return false;
                                });
                                await session.RunInTransaction(() =>
                                {
                                    Api.MoveBeforeFirst(session.Session, tableid);
                                    int counter = 0;
                                    while (Api.TryMoveNext(session.Session, tableid))
                                    {
                                        counter++;
                                    }
                                    Assert.AreEqual(1000, counter, "Количество записей в таблице не равно 1000 при построчном переборе");
                                    return false;
                                });
                            }
                            finally
                            {
                                await session.Run(() =>
                                {
                                    Api.JetCloseTable(session.Session, tableid);
                                });
                            }
                        }
                    }
                }
                finally
                {
                    await mainSession.Run(() =>
                    {
                        Api.JetCloseTable(mainSession.Session, tableid1);
                    });
                }
            }
        }

        [TestMethod]
        [TestCategory("ESENT")]
        public async Task TestEsentAccessParallel()
        {
            await RunEsentTest(async (provider, testData) =>
            {
                Assert.AreEqual(1, testData.InstancesCreated, "Было создано больше одного инстанса ESENT");
                var tasks = new Task[]
                {
                    SimpleSyncTestOnNewThread(provider, "test_table_1"),
                    SimpleSyncTestOnNewThread(provider, "test_table_2"),
                    SimpleSyncTestOnNewThread(provider, "test_table_3"),
                    SimpleSyncTestOnNewThread(provider, "test_table_4"),
                    SimpleSyncTestOnNewThread(provider, "test_table_5"),
                    SimpleSyncTestOnNewThread(provider, "test_table_6"),
                    SimpleSyncTestOnNewThread(provider, "test_table_7"),
                    SimpleSyncTestOnNewThread(provider, "test_table_8"),
                    SimpleSyncTestOnNewThread(provider, "test_table_9"),
                    SimpleSyncTestOnNewThread(provider, "test_table_10"),
                    SimpleSyncTestOnNewThread(provider, "test_table_11"),
                    SimpleSyncTestOnNewThread(provider, "test_table_12"),
                    SimpleSyncTestOnNewThread(provider, "test_table_13"),
                    SimpleSyncTestOnNewThread(provider, "test_table_14"),
                    SimpleSyncTestOnNewThread(provider, "test_table_15"),
                };
                await Task.WhenAll(tasks);
            });
        }

        [TestMethod]
        [TestCategory("ESENT")]
        public async Task TestEsentSuspension()
        {
            await RunEsentTest(async (provider, testData, collection) =>
            {
                var steps = new[]
                {
                    "test_table_1",
                    "test_table_2",
                    "test_table_3",
                    "test_table_4",
                    "test_table_5",
                };
                int expectedInstanceCount = 1;
                foreach (var step in steps)
                {
                    await collection.Suspend();
                    Assert.IsTrue(testData.IsSuspended, "Провайдер ESENT не был приостановлен");
                    Assert.AreEqual(expectedInstanceCount, testData.InstancesCreated, $"Должно быть создано {expectedInstanceCount} инстансов вместо {testData.InstancesCreated}");
                    await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                    {
                        var session = await provider.GetSecondarySession();
                        session.UseSession().Dispose();
                    }, "Не было брошено исключение при попытке получить сессию в приостановленном провайдере");
                    await collection.Resume();
                    Assert.IsFalse(testData.IsSuspended, "Провайдер ESENT не был возобновлён");
                    expectedInstanceCount++;
                    Assert.AreEqual(expectedInstanceCount, testData.InstancesCreated, $"Должно быть создано {expectedInstanceCount} инстансов вместо {testData.InstancesCreated}");
                    await SimpleSyncTest(provider, step);
                }
            });
        }

        [TestMethod]
        [TestCategory("ESENT")]
        public async Task TestEsentSuspensionOverlapped()
        {
            await RunEsentTest(async (provider, testData, collection) =>
            {
                int expectedInstanceCount = 1;
                List<Task> toAwait = new List<Task>();
                for (var i = 1; i < 5; i++)
                {
                    await collection.Suspend();
                    Assert.IsTrue(testData.IsSuspended, "Провайдер ESENT не был приостановлен");
                    Assert.AreEqual(expectedInstanceCount, testData.InstancesCreated, $"Должно быть создано {expectedInstanceCount} инстансов вместо {testData.InstancesCreated}");
                    await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                    {
                        var session = await provider.GetSecondarySession();
                        session.UseSession().Dispose();
                    }, "Не было брошено исключение при попытке получить сессию в приостановленном провайдере");
                    await Task.WhenAll(toAwait.ToArray());
                    toAwait.Clear();
                    await collection.Resume();
                    Assert.IsFalse(testData.IsSuspended, "Провайдер ESENT не был возобновлён");
                    expectedInstanceCount++;
                    Assert.AreEqual(expectedInstanceCount, testData.InstancesCreated, $"Должно быть создано {expectedInstanceCount} инстансов вместо {testData.InstancesCreated}");
                    var startTasks = new List<Task>();
                    for (var j = 1; j < 3; j++)
                    {
                        var step = $"table_{i}_{j}";
                        var tcs = new TaskCompletionSource<Nothing>();
                        startTasks.Add(tcs.Task);

                        async Task TestTask()
                        {
                            var mainSession = provider.MainSession;
                            using (mainSession.UseSession())
                            {
                                await Task.Delay(TimeSpan.FromSeconds(0.2));
                                tcs.SetResult(Nothing.Value);
                                JET_TABLEID tableid1 = default(JET_TABLEID);
                                string pk = null;
                                JET_COLUMNID id = default(JET_COLUMNID), val = default(JET_COLUMNID);
                                await mainSession.RunInTransaction(() =>
                                {
                                    var tt = CreateSimpleTable(mainSession, step);
                                    pk = tt.pk;
                                    id = tt.id;
                                    val = tt.val;
                                    return true;
                                });
                                await mainSession.Run(() => { Api.OpenTable(mainSession.Session, mainSession.Database, step, OpenTableGrbit.DenyWrite, out tableid1); });
                                try
                                {
                                    await mainSession.RunInTransaction(() =>
                                    {
                                        for (var x = 0; x < 1000; x++)
                                        {
                                            Api.JetPrepareUpdate(mainSession.Session, tableid1, JET_prep.Insert);
                                            var vv = "VAL_" + x.ToString();
                                            Api.SetColumns(mainSession.Session, tableid1, new StringColumnValue()
                                            {
                                                Columnid = val,
                                                Value = vv,
                                            });
                                            Api.JetUpdate(mainSession.Session, tableid1);
                                        }
                                        return true;
                                    });
                                    await mainSession.RunInTransaction(() =>
                                    {
                                        Api.JetSetTableSequential(mainSession.Session, tableid1, SetTableSequentialGrbit.None);
                                        int count;
                                        Api.TryMoveFirst(mainSession.Session, tableid1);
                                        Api.JetIndexRecordCount(mainSession.Session, tableid1, out count, int.MaxValue);
                                        Assert.AreEqual(1000, count, "Количество записей в таблице не равно 1000");
                                        return false;
                                    });
                                }
                                finally
                                {
                                    await mainSession.Run(() => { Api.JetCloseTable(mainSession.Session, tableid1); });
                                }
                                Assert.IsTrue(!testData.IsSuspended && testData.IsSuspendRequested, "Провайдер должен ожидать завершения, но не должен быть завершён");
                            }
                        }
                        toAwait.Add(RunAsyncOnThread(TestTask));
                    }
                    await Task.WhenAll(startTasks.ToArray());
                }
            });
        }

        [TestMethod]
        [TestCategory("ESENT")]
        public async Task TestEsentAccessAsync()
        {
            await RunEsentTest(async (provider, testData) =>
            {
                Assert.AreEqual(1, testData.InstancesCreated, "Было создано больше одного инстанса ESENT");
                var session = provider.MainSession;
                using (session.UseSession())
                {
                    JET_TABLEID tableid = default(JET_TABLEID);
                    string pk = null;
                    JET_COLUMNID id = default(JET_COLUMNID), val = default(JET_COLUMNID);
                    await RunAsyncOnThread(async () =>
                    {
                        await session.RunInTransaction(() =>
                        {
                            var tt = CreateSimpleTable(session, "test_table");
                            pk = tt.pk;
                            id = tt.id;
                            val = tt.val;
                            return true;
                        });
                    });
                    await session.Run(() =>
                    {
                        Api.OpenTable(session.Session, session.Database, "test_table", OpenTableGrbit.None, out tableid);
                    });
                    try
                    {
                        HashSet<string> addedValues = new HashSet<string>();
                        await RunAsyncOnThread(async () =>
                        {
                            await session.RunInTransaction(() =>
                            {
                                for (var i = 0; i < 1000; i++)
                                {
                                    Api.JetPrepareUpdate(session.Session, tableid, JET_prep.Insert);
                                    var vv = "VAL_" + i.ToString();
                                    Api.SetColumns(session.Session, tableid,
                                        new StringColumnValue()
                                        {
                                            Columnid = val,
                                            Value = vv,
                                        });
                                    addedValues.Add(vv);
                                    Api.JetUpdate(session.Session, tableid);
                                }
                                return true;
                            });
                        });
                        await RunAsyncOnThread(async () =>
                        {
                            await session.RunInTransaction(() =>
                            {
                                Api.JetSetTableSequential(session.Session, tableid, SetTableSequentialGrbit.None);
                                int count;
                                Api.TryMoveFirst(session.Session, tableid);
                                Api.JetIndexRecordCount(session.Session, tableid, out count, int.MaxValue);
                                Assert.AreEqual(1000, count, "Количество записей в таблице не равно 1000");
                                Api.MoveBeforeFirst(session.Session, tableid);
                                while (Api.TryMoveNext(session.Session, tableid))
                                {
                                    var s = Api.RetrieveColumnAsString(session.Session, tableid, val);
                                    Assert.IsTrue(addedValues.Remove(s), $"Строка {s} получена из базы, но она не была туда добавлена");
                                }
                                return false;
                            });
                        });
                        Assert.IsTrue(addedValues.Count == 0, $"Не все строки были прочитаны из базы = {addedValues.Count}");
                    }
                    finally
                    {
                        await session.Run(() =>
                        {
                            Api.JetCloseTable(session.Session, tableid);
                        });
                    }
                }
            });
        }

        [TestMethod]
        [TestCategory("ESENT")]
        public async Task TestMultivalueDistinct()
        {
            await RunEsentTest(async (provider, testData) =>
            {
                IEsentSession session = provider.MainSession;
                const string tableName = "TestMultivalue";
                const string idColumn = "Id";
                const string tagColumn = "Tag";
                const string indexName = "IX_Tag";
                const string pkDef = "+Id\0\0";
                const string ixDef = "+Tag\0\0";

                var toAdd = new List<Guid[]>()
                {
                    new [] { new Guid("{1BF157B8-FFEE-4D7E-AD21-F65A67DAC49B}") },
                    new [] { new Guid("{1BF157B8-FFEE-4D7E-AD21-F65A67DAC49B}") },
                    new Guid[0],
                    new [] { new Guid("{1BF157B8-FFEE-4D7E-AD21-F65A67DAC49B}"), new Guid("{EA3539E9-9205-4085-93C4-04B961E940CB}") },
                    new [] { new Guid("{1BF157B8-FFEE-4D7E-AD21-F65A67DAC49B}"), new Guid("{EA3539E9-9205-4085-93C4-04B961E940CB}") },
                    new [] { new Guid("{1BF157B8-FFEE-4D7E-AD21-F65A67DAC49B}"), new Guid("{EA3539E9-9205-4085-93C4-04B961E940CB}") },
                    new [] { new Guid("{EA3539E9-9205-4085-93C4-04B961E940CB}"), new Guid("{BC599936-E897-48FD-B0CD-E1F03FB60CE2}"), },
                    new [] { new Guid("{1BF157B8-FFEE-4D7E-AD21-F65A67DAC49B}"), new Guid("{EA3539E9-9205-4085-93C4-04B961E940CB}"), new Guid("{BC599936-E897-48FD-B0CD-E1F03FB60CE2}")},
                    new [] { new Guid("{1BF157B8-FFEE-4D7E-AD21-F65A67DAC49B}"), new Guid("{EA3539E9-9205-4085-93C4-04B961E940CB}"), new Guid("{BC599936-E897-48FD-B0CD-E1F03FB60CE2}")},
                    new [] { new Guid("{1BF157B8-FFEE-4D7E-AD21-F65A67DAC49B}"), new Guid("{BC599936-E897-48FD-B0CD-E1F03FB60CE2}")},
                    new [] { new Guid("{1BF157B8-FFEE-4D7E-AD21-F65A67DAC49B}"), new Guid("{2BAB0AEA-972D-463E-B43F-115A083DCF2D}") },
                    new [] { new Guid("{1BF157B8-FFEE-4D7E-AD21-F65A67DAC49B}"), new Guid("{2BAB0AEA-972D-463E-B43F-115A083DCF2D}") },
                };

                var toCheck = new HashSet<Guid>()
                {
                    new Guid("{1BF157B8-FFEE-4D7E-AD21-F65A67DAC49B}"),
                    new Guid("{EA3539E9-9205-4085-93C4-04B961E940CB}"),
                    new Guid("{BC599936-E897-48FD-B0CD-E1F03FB60CE2}"),
                    new Guid("{2BAB0AEA-972D-463E-B43F-115A083DCF2D}")
                };

                var counters = new Dictionary<Guid, int>();

                foreach (var a in toAdd)
                {
                    foreach (var id in a)
                    {
                        if (!counters.ContainsKey(id))
                        {
                            counters[id] = 1;
                        }
                        else
                        {
                            counters[id] = counters[id] + 1;
                        }
                    }
                }

                await session.RunInTransaction(() =>
                {
                    var sid = session.Session;
                    var dbid = session.Database;
                    JET_TABLEID tableid;
                    JET_COLUMNID tempid;
                    JET_COLUMNID tagId;
                    Api.JetCreateTable(sid, dbid, tableName, 0, 100, out tableid);
                    Api.JetAddColumn(sid, tableid, idColumn, new JET_COLUMNDEF()
                    {
                        coltyp = JET_coltyp.Long,
                        grbit = ColumndefGrbit.ColumnAutoincrement | ColumndefGrbit.ColumnNotNULL
                    }, null, 0, out tempid);
                    Api.JetAddColumn(sid, tableid, tagColumn, new JET_COLUMNDEF()
                    {
                        coltyp = VistaColtyp.GUID,
                        grbit = ColumndefGrbit.ColumnMultiValued | ColumndefGrbit.ColumnTagged
                    }, null, 0, out tagId);
                    Api.JetCreateIndex(sid, tableid, "PK", CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique, pkDef, pkDef.Length, 100);
                    Api.JetCreateIndex(sid, tableid, indexName, CreateIndexGrbit.IndexIgnoreAnyNull, ixDef, ixDef.Length, 100);

                    foreach (var tags in toAdd)
                    {
                        using (var update = new Update(sid, tableid, JET_prep.Insert))
                        {
                            var columns = tags.Select(t => new GuidColumnValue()
                            {
                                Value = t,
                                ItagSequence = 0,
                                Columnid = tagId,
                                SetGrbit = SetColumnGrbit.UniqueMultiValues
                            }).OfType<ColumnValue>().ToArray();
                            if (columns.Length > 0)
                            {
                                Api.SetColumns(sid, tableid, columns);
                            }
                            update.Save();
                        }
                    }
                    Api.JetCloseTable(sid, tableid);
                    return true;
                });

                session = await provider.GetSecondarySession();
                using (session.UseSession())
                {
                    await session.Run(() =>
                    {
                        using (var table = session.OpenTable(tableName, OpenTableGrbit.ReadOnly))
                        {
                            var colid = Api.GetTableColumnid(table.Session, table, tagColumn);
                            Api.JetSetCurrentIndex(table.Session, table.Table, indexName);

                            Assert.IsTrue(Api.TryMoveFirst(table.Session, table), "Нет ни одной записи");
                            do
                            {
                                var id1 = Api.RetrieveColumnAsGuid(table.Session, table, colid, RetrieveColumnGrbit.RetrieveFromIndex);
                                if (id1 != null)
                                {
                                    Assert.IsTrue(toCheck.Contains(id1.Value), $"{id1.Value}: Не найден ключ или повторное получение ключа");
                                    toCheck.Remove(id1.Value);
                                }
                            } while (Api.TryMove(table.Session, table, JET_Move.Next, MoveGrbit.MoveKeyNE));
                            Assert.AreEqual(0, toCheck.Count, "Не все ключи найдены в базе");

                            foreach (var key in counters.Keys)
                            {
                                Api.MakeKey(table.Session, table, key, MakeKeyGrbit.NewKey);
                                Assert.IsTrue(Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange));
                                int count;
                                Api.JetIndexRecordCount(table.Session, table, out count, Int32.MaxValue);
                                Assert.AreEqual(counters[key], count, $"{key}: Не совпадает количество элементов (Count)");

                                Api.MakeKey(table.Session, table, key, MakeKeyGrbit.NewKey);
                                Assert.IsTrue(Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange));
                                int count2 = 0;
                                do
                                {
                                    count2++;
                                } while (Api.TryMoveNext(table.Session, table));
                                Assert.AreEqual(counters[key], count2, $"{key}: Не совпадает количество элементов (Move)");
                            }
                        }
                    });
                }
            });
        }

        private int GetMultiValueCount(EsentTable table, JET_COLUMNID columnid)
        {
            JET_RETRIEVECOLUMN col = new JET_RETRIEVECOLUMN
            {
                columnid = columnid,
                itagSequence = 0
            };
            Api.JetRetrieveColumns(table.Session, table, new[] { col }, 1);
            return col.itagSequence;
        }

        /// <summary>
        /// Перечислить значения в столбце со многоими значениями.
        /// </summary>
        /// <param name="table">Таблица.</param>
        /// <param name="columnid">Идентификатор столбца.</param>
        /// <param name="factoryFunc">Фабрика создания значений для получения данных.</param>
        /// <returns>Результат.</returns>
        private IEnumerable<ColumnValue> EnumMultivalueColumn(EsentTable table, JET_COLUMNID columnid, Func<ColumnValue> factoryFunc)
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


        private (string pk, JET_COLUMNID id, JET_COLUMNID val) CreateSimpleTable(IEsentSession session, string tableName)
        {
            JET_TABLEID tableid;
            Api.JetCreateTable(session.Session, session.Database, tableName, 1, 100, out tableid);
            JET_COLUMNID colId, colVal;
            Api.JetAddColumn(session.Session, tableid, "Id", new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnNotNULL | ColumndefGrbit.ColumnAutoincrement,
            }, null, 0, out colId);
            Api.JetAddColumn(session.Session, tableid, "Val", new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.None,
                cp = JET_CP.Unicode
            }, null, 0, out colVal);
            var idxDef = "+Id\0\0";
            var pk = $"PK_{tableName}";
            Api.JetCreateIndex(session.Session, tableid, pk, CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique, idxDef, idxDef.Length, 100);
            Api.JetCloseTable(session.Session, tableid);
            return (pk, colId, colVal);
        }

        private async Task<ModuleCollection> InitializeEsent()
        {
            var collection = new ModuleCollection();
            EsentModulesRegistration.RegisterModules(collection, true);
            await collection.Seal();
            return collection;
        }

        private static readonly SemaphoreSlim _testSemaphore = new SemaphoreSlim(1, 1);

        private async Task RunEsentTest(Func<IEsentInstanceProvider, IEsentInstanceProviderForTests, Task> test)
        {
            await _testSemaphore.WaitAsync();
            try
            {
                var collection = await InitializeEsent();
                try
                {
                    var module = collection.GetModuleProvider().QueryModule<object>(typeof(IEsentInstanceProvider), null);
                    var provider = module?.QueryView<IEsentInstanceProvider>();
                    var testData = module?.QueryView<IEsentInstanceProviderForTests>();
                    Assert.IsNotNull(provider, "Провайдер ESENT не найден");
                    Assert.IsNotNull(testData, "Тестовый интерфейс ESENT не найден");
                    await test(provider, testData);
                }
                finally
                {
                    await collection.Dispose();
                }
            }
            finally
            {
                _testSemaphore.Release();
            }
        }
        private async Task RunEsentTest(Func<IEsentInstanceProvider, IEsentInstanceProviderForTests, ModuleCollection, Task> test)
        {
            await _testSemaphore.WaitAsync();
            try
            {
                var collection = await InitializeEsent();
                try
                {
                    var module = collection.GetModuleProvider().QueryModule<object>(typeof(IEsentInstanceProvider), null);
                    var provider = module?.QueryView<IEsentInstanceProvider>();
                    var testData = module?.QueryView<IEsentInstanceProviderForTests>();
                    Assert.IsNotNull(provider, "Провайдер ESENT не найден");
                    Assert.IsNotNull(testData, "Тестовый интерфейс ESENT не найден");
                    await test(provider, testData, collection);
                }
                finally
                {
                    await collection.Dispose();
                }
            }
            finally
            {
                _testSemaphore.Release();
            }
        }
    }
}