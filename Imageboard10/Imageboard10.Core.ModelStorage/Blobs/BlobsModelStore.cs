using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Imageboard10.Core.Database;
using Microsoft.Isam.Esent.Interop;

using static Imageboard10.Core.ModelStorage.Blobs.BlobTableInfo;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Хранилище бинарных данных.
    /// </summary>
    public sealed class BlobsModelStore : ModelStorageBase<IBlobsModelStore>, IBlobsModelStore
    {
        /// <summary>
        /// Создать или обновить таблицы.
        /// </summary>
        protected override async ValueTask<Nothing> CreateOrUpgradeTables()
        {
            await base.CreateOrUpgradeTables();
            await EnsureTable(BlobsTable, 1, InitializeBlobsTable, null);
            await EnsureTable(BlockTable, 1, InitializeBlocksTable, null);
            try
            {
                await CleanInvalidBlobs();
            }
            catch (Exception ex)
            {
                GlobalErrorHandler?.SignalError(ex);
            }
            return Nothing.Value;
        }

        private void InitializeBlobsTable(IEsentSession session, JET_TABLEID tableid)
        {
            var sid = session.Session;

            JET_COLUMNID tempid;

            Api.JetAddColumn(sid, tableid, BlobTableColumns.Id, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnAutoincrement | ColumndefGrbit.ColumnNotNULL
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobTableColumns.Name, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnNotNULL,
                cp = JET_CP.Unicode
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobTableColumns.Category, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnMaybeNull,
                cp = JET_CP.Unicode
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobTableColumns.Length, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Currency,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobTableColumns.BlockSize, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobTableColumns.CreatedDate, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.DateTime,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobTableColumns.IsInlined, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Bit,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobTableColumns.InlineData, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongBinary,
                grbit = ColumndefGrbit.ColumnMaybeNull,
            }, null, 0, out tempid);

            var intzero = new byte[4] {0, 0, 0, 0};

            Api.JetAddColumn(sid, tableid, BlobTableColumns.BlockId, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnNotNULL | ColumndefGrbit.ColumnEscrowUpdate
            }, intzero, 4, out tempid);

            Api.JetAddColumn(sid, tableid, BlobTableColumns.UblockId, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnNotNULL | ColumndefGrbit.ColumnEscrowUpdate
            }, intzero, 4, out tempid);

            Api.JetAddColumn(sid, tableid, BlobTableColumns.BlockUntil, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.DateTime,
                grbit = ColumndefGrbit.ColumnMaybeNull,
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobTableColumns.Commited, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Bit,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempid);

            var pkDef = $"+{BlobTableColumns.Id}\0\0";
            var nameDef = $"+{BlobTableColumns.Name}\0\0";
            var categoryDef = $"+{BlobTableColumns.Category}\0\0";
            var commitedyDef = $"+{BlobTableColumns.Commited}\0\0";
            Api.JetCreateIndex(sid, tableid, BlobTableIndexes.Primary, CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique, pkDef, pkDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, BlobTableIndexes.Name, CreateIndexGrbit.IndexUnique, nameDef, nameDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, BlobTableIndexes.Category, CreateIndexGrbit.None, categoryDef, categoryDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, BlobTableIndexes.Commited, CreateIndexGrbit.None, commitedyDef, commitedyDef.Length, 100);
        }

        private void InitializeBlocksTable(IEsentSession session, JET_TABLEID tableid)
        {
            var sid = session.Session;

            JET_COLUMNID tempid;

            Api.JetAddColumn(sid, tableid, BlocksTableColumns.BlobId, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnNotNULL
            }, null, 0, out tempid);
            Api.JetAddColumn(sid, tableid, BlocksTableColumns.Counter, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnNotNULL
            }, null, 0, out tempid);
            Api.JetAddColumn(sid, tableid, BlocksTableColumns.Data, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongBinary,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempid);

            var pkDef = $"+{BlocksTableColumns.BlobId}\0+{BlocksTableColumns.Counter}\0\0";
            var blobidDef = $"+{BlocksTableColumns.BlobId}\0\0";

            Api.JetCreateIndex(sid, tableid, BlocksTableIndexes.Primary, CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique, pkDef, pkDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, BlocksTableIndexes.BlobId, CreateIndexGrbit.None, blobidDef, blobidDef.Length, 100);
        }

        private Task CleanInvalidBlobs()
        {
            return UpdateAsync(async session =>
            {
                var sid = session.Session;
                List<int> toDelete = new List<int>();
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(sid, table, BlobTableIndexes.Commited);
                        Api.MakeKey(sid, table, false, MakeKeyGrbit.NewKey);
                        var colid = Api.GetTableColumnid(sid, table, BlobTableColumns.Id);
                        if (Api.TrySeek(sid, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            do
                            {
                                var id = Api.RetrieveColumnAsInt32(sid, table, colid);
                                if (id != null)
                                {
                                    toDelete.Add(id.Value);
                                }
                            } while (Api.TryMoveNext(sid, table));
                        }
                    }
                    return false;
                });
                if (toDelete.Count > 0)
                {
                    foreach (var id in toDelete)
                    {
                        var bid = id;
                        try
                        {
                            await session.RunInTransaction(() =>
                            {
                                using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                                {
                                    Api.MakeKey(sid, table, bid, MakeKeyGrbit.NewKey);
                                    if (Api.TrySeek(sid, table, SeekGrbit.SeekEQ))
                                    {
                                        DeleteBlocks(session, bid);
                                        Api.JetDelete(sid, table);
                                    }
                                }
                                return true;
                            });
                        }
                        catch
                        {
                            // Игнорируем ошибки
                        }
                    }
                }
                return Nothing.Value;
            });
        }

        /// <summary>
        /// Сохранить файл.
        /// </summary>
        /// <param name="blob">Файл.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>GUID файла.</returns>
        public async Task<BlobId> SaveBlob(InputBlob blob, CancellationToken token)
        {
            if (blob.BlobStream == null)
            {
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("blob.BlobStream");
            }
            if (blob.UniqueName == null)
            {
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("blob.UniqueName");
            }

            CheckModuleReady();
            await WaitForTablesInitialize();

            return await UpdateAsync(async session =>
            {
                token.ThrowIfCancellationRequested();
                int blobId = -1;
                bool isInlined = false;
                var reader = new BlockReader(MaxBlockSize, blob.BlobStream, blob.MaxSize ?? long.MaxValue);
                List<byte[]> preRead = null;
                if (!blob.DisableInlining)
                {
                    long size;
                    (preRead, size) = await reader.ReadBlocks(MaxInlineSize, token);
                    if (preRead.Count < MaxInlineSize || size < MaxBlockSize * MaxInlineSize || preRead.Sum(r => r.Length) < MaxBlockSize * MaxInlineSize)
                    {
                        isInlined = true;
                    }
                }
                byte[] mergedInline = null;
                if (isInlined)
                {
                    using (var str = new MemoryStream())
                    {
                        foreach (var p in preRead)
                        {
                            str.Write(p, 0, p.Length);
                        }
                        mergedInline = str.ToArray();
                    }
                }
                await session.RunInTransaction(() =>
                {
                    var sid = session.Session;
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                    {
                        var columnMap = Api.GetColumnDictionary(sid, table);
                        using (var update = new Update(sid, table, JET_prep.Insert))
                        {
                            blobId = Api.RetrieveColumnAsInt32(sid, table, columnMap[BlobTableColumns.Id]) ??
                                     throw new BlobException("Не доступно значение autioncrement Id");
                            var columns = new ColumnValue[]
                            {
                                new StringColumnValue()
                                {
                                    Columnid = columnMap[BlobTableColumns.Name],
                                    Value = blob.UniqueName,
                                },
                                new StringColumnValue()
                                {
                                    Columnid = columnMap[BlobTableColumns.Category],
                                    Value = blob.Category,
                                },
                                new Int64ColumnValue()
                                {
                                    Columnid = columnMap[BlobTableColumns.Length],
                                    Value = mergedInline?.Length ?? 0,
                                },
                                new Int32ColumnValue()
                                {
                                    Columnid = columnMap[BlobTableColumns.BlockSize],
                                    Value = MaxBlockSize,
                                },
                                new DateTimeColumnValue()
                                {
                                    Columnid = columnMap[BlobTableColumns.CreatedDate],
                                    Value = DateTime.Now
                                },
                                new BoolColumnValue()
                                {
                                    Columnid = columnMap[BlobTableColumns.IsInlined],
                                    Value = mergedInline != null,
                                },
                                new BytesColumnValue()
                                {
                                    Columnid = columnMap[BlobTableColumns.InlineData],
                                    Value = mergedInline
                                },
                                new BoolColumnValue()
                                {
                                    Columnid = columnMap[BlobTableColumns.Commited],
                                    Value = mergedInline != null,
                                },
                            };
                            Api.SetColumns(sid, table, columns);
                            update.Save();
                        }
                    }
                    return true;
                });
                int blockCount = 0;
                if (mergedInline == null)
                {
                    long totalSize = 0;
                    if (preRead != null)
                    {
                        await session.RunInTransaction(() =>
                        {
                            SaveBlocks(session, blobId, preRead, token, ref blockCount);
                            totalSize = preRead.Sum(r => r.Length);
                            return true;
                        });
                    }
                    bool isStop = false;
                    while (!isStop)
                    {
                        (var toSave, var rdsize) = await reader.ReadBlocks(10, token);
                        if (toSave.Count < 10 || toSave.Sum(r => r.Length) < MaxBlockSize * 10)
                        {
                            isStop = true;
                        }
                        await session.RunInTransaction(() =>
                        {
                            SaveBlocks(session, blobId, toSave, token, ref blockCount);
                            totalSize = toSave.Sum(r => r.Length);
                            return true;
                        });
                    }
                    await session.RunInTransaction(() =>
                    {
                        var sid = session.Session;
                        using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                        {
                            Api.MakeKey(sid, table, blobId, MakeKeyGrbit.NewKey);
                            if (!Api.TrySeek(sid, table, SeekGrbit.SeekEQ))
                            {
                                throw new BlobException($"Не найден Blob с идентификатором {blobId} при завершении загрузки данных");
                            }
                            using (var update = new Update(sid, table, JET_prep.Replace))
                            {
                                var columnMap = Api.GetColumnDictionary(sid, table);
                                Api.SetColumn(sid, table, columnMap[BlobTableColumns.Length], totalSize);
                                Api.SetColumn(sid, table, columnMap[BlobTableColumns.Commited], true);
                                update.Save();
                            }
                        }
                        return true;
                    });
                }
                return new BlobId() { Id = blobId };
            });
        }

        /// <summary>
        /// Получить GUID файла.
        /// </summary>
        /// <param name="uniqueName">Имя файла.</param>
        /// <returns>GUID файла.</returns>
        public async Task<BlobId?> FindBlob(string uniqueName)
        {
            if (uniqueName == null) throw new ArgumentNullException(nameof(uniqueName));

            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                BlobId? result = null;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(table.Session, table, BlobTableIndexes.Name);
                        Api.MakeKey(table.Session, table, uniqueName, Encoding.Unicode, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            result = new BlobId()
                            {
                                Id = Api.RetrieveColumnAsInt32(table.Session, table, Api.GetTableColumnid(table.Session, table, BlobTableColumns.Id)) ?? 0
                            };
                        }
                    }
                });
                return result;
            });
        }

        /// <summary>
        /// Загрузить файл.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="maxLockTime">Максимальное время блокировки.</param>
        /// <returns>Результат.</returns>
        public async Task<Stream> LoadBlob(BlobId id, TimeSpan maxLockTime)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();

            var r = await QueryReadonly(async session =>
            {
                BlobStreamBase result = null;
                long size = 0;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            var columnMap = Api.GetColumnDictionary(table.Session, table);
                            var isInlined = Api.RetrieveColumnAsBoolean(table.Session, table, columnMap[BlobTableColumns.IsInlined]) ?? throw new BlobException($"Неверные даные в таблице {BlobsTable}");
                            size = Api.RetrieveColumnAsInt64(table.Session, table, columnMap[BlobTableColumns.Length]) ?? throw new BlobException($"Неверные даные в таблице {BlobsTable}");
                            if (isInlined)
                            {
                                var c = new ColumnValue[]
                                {
                                    new BytesColumnValue()
                                    {
                                        Columnid = columnMap[BlobTableColumns.InlineData],
                                        RetrieveGrbit = RetrieveColumnGrbit.None
                                    }
                                };
                                Api.RetrieveColumns(table.Session, table, c);
                                var data = ((BytesColumnValue) c[0]).Value;
                                if (data == null)
                                {
                                    throw new BlobException($"Неверные даные в таблице {BlobsTable}");
                                }
                                result = new InlineBlobStream(EsentProvider, this, GlobalErrorHandler, data);
                            }
                        }
                        else
                        {
                            throw new BlobNotFoundException(id);
                        }
                    }
                });
                if (result == null)
                {
                    throw new BlobException("");
                }
                return (result, size);
            });
            if (r.Item1 != null)
            {
                return r.Item1;
            }
            var l = await LockBlob(id, maxLockTime);
            if (l == null)
            {
                throw new BlobException($"Неверные даные в таблице {BlobsTable}");
            }
            return new BlocksBlobStream(EsentProvider, this, GlobalErrorHandler, l.Value, r.Item2, id);
        }

        /// <summary>
        /// Удалить файл.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>true, если файл найден и удалён. false, если нет такого файла или файл заблокирован на удаление.</returns>
        public Task<bool> DeleteBlob(BlobId id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Удалить блобы.
        /// </summary>
        /// <param name="idArray">Массив идентификаторов.</param>
        /// <returns>Массив идентификаторов тех файлов, которые получилось удалить.</returns>
        public Task<BlobId[]> DeleteBlobs(BlobId[] idArray)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Блокировка файла (нельзя удалить).
        /// </summary>
        /// <param name="id">Идентификатор файла.</param>
        /// <param name="maxLockTime">Максимальное время блокировки.</param>
        /// <returns>Идентификатор блокировки. null, если файл не найден.</returns>
        public async Task<BlobLockId?> LockBlob(BlobId id, TimeSpan maxLockTime)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            var until = DateTime.Now + maxLockTime;
            return await UpdateAsync(async session =>
            {
                BlobLockId? result = null;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                    {
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            var untilid = Api.GetTableColumnid(table.Session, table, BlobTableColumns.BlockUntil);
                            var oldUntil = Api.RetrieveColumnAsDateTime(table.Session, table, untilid);
                            using (var update = new Update(table.Session, table, JET_prep.Replace))
                            {
                                Api.EscrowUpdate(table.Session, table, Api.GetTableColumnid(table.Session, table, BlobTableColumns.BlockId), 1);
                                if (until > oldUntil || oldUntil == null)
                                {
                                    Api.SetColumn(table.Session, table.Table, untilid, until);
                                }
                                update.Save();
                            }
                            result = new BlobLockId() { BlobId = id };
                        }
                    }
                });
                return result;
            });
        }

        /// <summary>
        /// Разблокировать файл.
        /// </summary>
        /// <param name="lockId">Идентификатор блокировки.</param>
        /// <returns>true, если разблокировано.</returns>
        public async Task<bool> UnlockBlob(BlobLockId lockId)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await UpdateAsync(async session =>
            {
                bool result = false;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                    {
                        Api.MakeKey(table.Session, table, lockId.BlobId.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            using (var update = new Update(table.Session, table, JET_prep.Replace))
                            {
                                Api.EscrowUpdate(table.Session, table, Api.GetTableColumnid(table.Session, table, BlobTableColumns.UblockId), 1);
                                update.Save();
                            }
                            result = true;
                        }
                    }
                });
                return result;
            });
        }

        /// <summary>
        /// Получить размер файла.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Информация о файле.</returns>
        public Task<BlobInfo?> GetBlobInfo(BlobId id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Проверка, заблокирован ли файл.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Результат.</returns>
        public Task<bool?> IsLocked(BlobId id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Читать категорию.
        /// </summary>
        /// <param name="category">Категория.</param>
        /// <returns>Результат.</returns>
        public Task<BlobInfo[]> ReadCategory(string category)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получить размер категории.
        /// </summary>
        /// <param name="category">Категория.</param>
        /// <returns>Размер категории.</returns>
        public Task<long> GetCategorySize(string category)
        {
            throw new NotImplementedException();
        }

        private void DeleteBlocks(IEsentSession session, int blobId)
        {
            using (var table = session.OpenTable(BlockTable, OpenTableGrbit.DenyWrite))
            {
                Api.JetSetCurrentIndex(table.Session, table, BlocksTableIndexes.BlobId);
                Api.MakeKey(table.Session, table, blobId, MakeKeyGrbit.NewKey);
                if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                {
                    do
                    {
                        Api.JetDelete(table.Session, table);
                    } while (Api.TryMoveNext(table.Session, table));
                }
            }
        }

        private void SaveBlocks(IEsentSession session, int blobId, IList<byte[]> preRead, CancellationToken token, ref int blockCount)
        {
            var sid = session.Session;
            using (var table = session.OpenTable(BlockTable, OpenTableGrbit.DenyWrite))
            {
                foreach (var b in preRead)
                {
                    token.ThrowIfCancellationRequested();
                    var columnMap = Api.GetColumnDictionary(sid, table);
                    using (var update = new Update(sid, table, JET_prep.Insert))
                    {
                        var columns = new ColumnValue[]
                        {
                            new Int32ColumnValue()
                            {
                                Columnid = columnMap[BlocksTableColumns.BlobId],
                                Value = blobId,
                            },
                            new Int32ColumnValue()
                            {
                                Columnid = columnMap[BlocksTableColumns.Counter],
                                Value = blockCount,
                            },
                            new BytesColumnValue()
                            {
                                Columnid = columnMap[BlocksTableColumns.Data],
                                Value = b
                            },
                        };
                        Api.SetColumns(sid, table, columns);
                        update.Save();
                        blockCount++;
                    }
                }
            }
        }
    }
}