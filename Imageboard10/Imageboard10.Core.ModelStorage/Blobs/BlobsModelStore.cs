using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Imageboard10.Core.Database;
using Imageboard10.Core.Tasks;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;
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
            await EnsureTable(ReferencesTable, 1, InitializeReferencesTable, null);
            await DoDeleteAllUncompletedBlobs();
            return Nothing.Value;
        }

        private void InitializeBlobsTable(IEsentSession session, JET_TABLEID tableid)
        {
            var sid = session.Session;

            JET_COLUMNID tempid;

            Api.JetAddColumn(sid, tableid, BlobsTableColumns.Id, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnAutoincrement | ColumndefGrbit.ColumnNotNULL
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobsTableColumns.Name, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnNotNULL,
                cp = JET_CP.Unicode
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobsTableColumns.Category, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.ColumnMaybeNull,
                cp = JET_CP.Unicode
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobsTableColumns.Length, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Currency,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobsTableColumns.CreatedDate, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.DateTime,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobsTableColumns.Data, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongBinary,
                grbit = ColumndefGrbit.ColumnMaybeNull | ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobsTableColumns.ReferenceId, new JET_COLUMNDEF()
            {
                coltyp = VistaColtyp.GUID,
                grbit = ColumndefGrbit.ColumnMaybeNull
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobsTableColumns.IsCompleted, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Bit,
                grbit = ColumndefGrbit.ColumnNotNULL
            }, null, 0, out tempid);

            var pkDef = $"+{BlobsTableColumns.Id}\0\0";
            var nameDef = $"+{BlobsTableColumns.Name}\0\0";
            var categoryDef = $"+{BlobsTableColumns.Category}\0+{BlobsTableColumns.IsCompleted}\0\0";
            var refDef = $"+{BlobsTableColumns.ReferenceId}\0+{BlobsTableColumns.IsCompleted}\0\0";
            var compDef = $"+{BlobsTableColumns.IsCompleted}\0\0";
            Api.JetCreateIndex(sid, tableid, BlobsTableIndexes.Primary, CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique, pkDef, pkDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, BlobsTableIndexes.Name, CreateIndexGrbit.IndexUnique, nameDef, nameDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, BlobsTableIndexes.Category, CreateIndexGrbit.None, categoryDef, categoryDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, BlobsTableIndexes.ReferenceId, CreateIndexGrbit.None, refDef, refDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, BlobsTableIndexes.IsCompleted, CreateIndexGrbit.None, compDef, compDef.Length, 100);
        }

        private void InitializeReferencesTable(IEsentSession session, JET_TABLEID tableid)
        {
            var sid = session.Session;

            JET_COLUMNID tempid;

            Api.JetAddColumn(sid, tableid, ReferencesTableColumns.ReferenceId, new JET_COLUMNDEF()
            {
                coltyp = VistaColtyp.GUID,
                grbit = ColumndefGrbit.ColumnNotNULL
            }, null, 0, out tempid);
            var pkDef = $"+{ReferencesTableColumns.ReferenceId}\0\0";
            Api.JetCreateIndex(sid, tableid, BlobsTableIndexes.Primary, CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique, pkDef, pkDef.Length, 100);
        }

        private struct SaveId
        {
            public int Id;
            public byte[] Bookmark;
            public int BookmarkSize;
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

                var frs = await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                    {
                        var sid = table.Session;
                        var columnMap = Api.GetColumnDictionary(sid, table);
                        using (var update = new Update(sid, table, JET_prep.Insert))
                        {
                            var bid = Api.RetrieveColumnAsInt32(sid, table, columnMap[BlobsTableColumns.Id], RetrieveColumnGrbit.RetrieveCopy) ??
                                     throw new BlobException("Не доступно значение autioncrement Id");
                            var bmark = new byte[SystemParameters.BookmarkMost];
                            int bsize = 0;
                            var columns = new ColumnValue[]
                            {
                                new StringColumnValue()
                                {
                                    Columnid = columnMap[BlobsTableColumns.Name],
                                    Value = blob.UniqueName,
                                },
                                new StringColumnValue()
                                {
                                    Columnid = columnMap[BlobsTableColumns.Category],
                                    Value = blob.Category,
                                },
                                new DateTimeColumnValue()
                                {
                                    Columnid = columnMap[BlobsTableColumns.CreatedDate],
                                    Value = DateTime.Now
                                },
                                new BytesColumnValue()
                                {
                                    Columnid = columnMap[BlobsTableColumns.Data],
                                    Value = new byte[0]
                                },
                                new Int64ColumnValue()
                                {
                                    Columnid = columnMap[BlobsTableColumns.Length],
                                    Value = 0
                                },
                                new GuidColumnValue()
                                {
                                    Columnid = columnMap[BlobsTableColumns.ReferenceId],
                                    Value = blob.ReferenceId
                                },
                                new BoolColumnValue()
                                {
                                    Columnid = columnMap[BlobsTableColumns.IsCompleted],
                                    Value = false
                                },
                            };
                            Api.SetColumns(sid, table, columns);
                            update.Save(bmark, bmark.Length, out bsize);
                            return (true, new SaveId() { Id = bid, Bookmark = bmark, BookmarkSize = bsize });
                        }
                    }
                });

                var blobId = frs.Id;
                var bookmark = frs.Bookmark;
                var bookmarkSize = frs.BookmarkSize;

                byte[] buffer = new byte[64 * 1024];
                long counter = 0;

                try
                {
                    var toRead = blob.MaxSize ?? long.MaxValue;
                    do
                    {
                        var szToRead = (int) Math.Min(buffer.Length, toRead);
                        if (szToRead <= 0)
                        {
                            break;
                        }

                        var sz = await blob.BlobStream.ReadAsync(buffer, 0, szToRead, token);
                        if (sz <= 0)
                        {
                            break;
                        }

                        var counter2 = counter;
                        await session.RunInTransaction(() =>
                        {
                            token.ThrowIfCancellationRequested();
                            using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                            {
                                var sid = table.Session;
                                var columnMap = Api.GetColumnDictionary(sid, table);

                                if (!Api.TryGotoBookmark(table.Session, table, bookmark, bookmarkSize))
                                {                                    
                                    throw new BlobException($"Неверные данные в таблице {BlobsTable}, ID={blobId}, pos={counter2}");
                                }
                                using (var update = new Update(sid, table, JET_prep.Replace))
                                {
                                    using (var str = new ColumnStream(sid, table, columnMap[BlobsTableColumns.Data]))
                                    {
                                        str.Seek(0, SeekOrigin.End);
                                        str.Write(buffer, 0, sz);
                                        if (str.Length > MaxFileSize)
                                        {
                                            throw new BlobException("Размер сохраняемого файла больше допустимого");
                                        }
                                    }
                                    update.Save();
                                }

                                return true;
                            }
                        });

                        counter += sz;

                        if (sz < szToRead)
                        {
                            break;
                        }
                    } while (true);

                    await session.RunInTransaction(() =>
                    {
                        using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                        {
                            var sid = table.Session;
                            var columnMap = Api.GetColumnDictionary(sid, table);
                            if (!Api.TryGotoBookmark(table.Session, table, bookmark, bookmarkSize))
                            {
                                throw new BlobException($"Неверные данные в таблице {BlobsTable}, ID={blobId}");
                            }
                            using (var update = new Update(sid, table, JET_prep.Replace))
                            {
                                var size = Api.RetrieveColumnSize(table.Session, table, columnMap[BlobsTableColumns.Data]) ?? 0;
                                Api.SetColumn(sid, table, columnMap[BlobsTableColumns.Length], (long)size);
                                Api.SetColumn(sid, table, columnMap[BlobsTableColumns.IsCompleted], true);
                                update.Save();
                            }
                            return true;
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    try
                    {
                        await session.RunInTransaction(() =>
                        {
                            using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                            {
                                if (Api.TryGotoBookmark(table.Session, table, bookmark, bookmarkSize))
                                {
                                    Api.JetDelete(table.Session, table);
                                }
                                return true;
                            }
                        });
                    }
                    catch (Exception e2)
                    {
                        // ReSharper disable once PossibleIntendedRethrow
                        throw e2;
                    }
                    throw;
                }
                catch (Exception e)
                {
                    try
                    {
                        await session.RunInTransaction(() =>
                        {
                            using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                            {
                                if (Api.TryGotoBookmark(table.Session, table, bookmark, bookmarkSize))
                                {
                                    Api.JetDelete(table.Session, table);
                                }
                                return true;
                            }
                        });
                    }
                    catch (Exception e2)
                    {
                        throw new AggregateException(e, e2);
                    }
                    throw;
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
                        Api.JetSetCurrentIndex(table.Session, table, BlobsTableIndexes.Name);
                        Api.MakeKey(table.Session, table, uniqueName, Encoding.Unicode, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            if (Api.RetrieveColumnAsBoolean(table.Session, table.Table, Api.GetTableColumnid(table.Session, table.Table, BlobsTableColumns.IsCompleted)) ?? false)
                            {
                                result = new BlobId()
                                {
                                    Id = Api.RetrieveColumnAsInt32(table.Session, table, Api.GetTableColumnid(table.Session, table, BlobsTableColumns.Id)) ?? 0
                                };
                            }
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
        /// <returns>Результат.</returns>
        public async Task<Stream> LoadBlob(BlobId id)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();

            return await QueryReadonly(async session =>
            {
                BlobStreamBase result = null;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            var columnMap = Api.GetColumnDictionary(table.Session, table);
                            var size = Api.RetrieveColumnSize(table.Session, table, columnMap[BlobsTableColumns.Data]) ?? 0;
                            if (size <= MaxInlineSize)
                            {
                                var columns = new ColumnValue[]
                                {
                                    new BytesColumnValue()
                                    {
                                        Columnid = columnMap[BlobsTableColumns.Data],
                                    },
                                };
                                Api.RetrieveColumns(table.Session, table, columns);
                                var data = ((BytesColumnValue) columns[0]).Value;
                                if (data == null)
                                {
                                    throw new BlobException($"Неверные данные в таблице {BlobsTable}");
                                }
                                result = new InlineBlobStream(GlobalErrorHandler, data);
                            }
                            else
                            {
                                result = new BlocksBlobStream(GlobalErrorHandler, session, id);
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
                    throw new BlobException($"Неверные данные в таблице {BlobsTable}");
                }
                return result;
            });
        }

        private bool DoDeleteBlob(EsentTable table, BlobId blobId)
        {
            var sid = table.Session;
            Api.MakeKey(sid, table, blobId.Id, MakeKeyGrbit.NewKey);
            if (Api.TrySeek(sid, table, SeekGrbit.SeekEQ))
            {
                Api.JetDelete(sid, table);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Удалить файл.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>true, если файл найден и удалён. false, если нет такого файла или файл заблокирован на удаление.</returns>
        public async Task<bool> DeleteBlob(BlobId id)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await UpdateAsync(async session =>
            {
                bool result = false;
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                    {
                        result = DoDeleteBlob(table, id);
                    }
                    return true;
                });
                return result;
            });
        }

        /// <summary>
        /// Удалить блобы.
        /// </summary>
        /// <param name="idArray">Массив идентификаторов.</param>
        /// <returns>Массив идентификаторов тех файлов, которые получилось удалить.</returns>
        public async Task<BlobId[]> DeleteBlobs(BlobId[] idArray)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await UpdateAsync(async session =>
            {
                List<BlobId> result = new List<BlobId>();
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                    {
                        foreach (var id in idArray)
                        {
                            if (DoDeleteBlob(table, id))
                            {
                                result.Add(id);
                            }
                        }
                    }
                    return true;
                });
                return result.ToArray();
            });
        }

        /// <summary>
        /// Получить размер файла.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Информация о файле.</returns>
        public async Task<BlobInfo?> GetBlobInfo(BlobId id)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                BlobInfo? result = null;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        var columnMap = Api.GetColumnDictionary(table.Session, table);
                        result = LoadBlobInfo(table, id, columnMap);
                    }
                });
                return result;
            });
        }

        private BlobInfo? LoadBlobInfo(EsentTable table, BlobId id, IDictionary<string, JET_COLUMNID> columnMap)
        {
            BlobInfo? result = null;
            if (SeekBlob(table, id, false))
            {
                var columns = new ColumnValue[]
                {
                    new StringColumnValue()
                    {
                        Columnid = columnMap[BlobsTableColumns.Name],
                    },
                    new StringColumnValue()
                    {
                        Columnid = columnMap[BlobsTableColumns.Category],
                    },
                    new DateTimeColumnValue()
                    {
                        Columnid = columnMap[BlobsTableColumns.CreatedDate],
                    },
                    new Int64ColumnValue()
                    {
                        Columnid = columnMap[BlobsTableColumns.Length],
                    },
                    new GuidColumnValue()
                    {
                        Columnid = columnMap[BlobsTableColumns.ReferenceId],
                    },
                    new BoolColumnValue()
                    {
                        Columnid = columnMap[BlobsTableColumns.IsCompleted],
                    },
                };
                Api.RetrieveColumns(table.Session, table, columns);
                if (((BoolColumnValue) columns[5]).Value ?? false)
                {
                    result = new BlobInfo()
                    {
                        Id = id,
                        UniqueName = ((StringColumnValue)columns[0]).Value,
                        Category = ((StringColumnValue)columns[1]).Value,
                        CreatedTime = ((DateTimeColumnValue)columns[2]).Value ?? DateTime.MinValue,
                        Size = ((Int64ColumnValue)columns[3]).Value ?? 0,
                        ReferenceId = ((GuidColumnValue)columns[4]).Value,
                        IsUncompleted = false
                    };
                }
                else
                {
                    result = new BlobInfo()
                    {
                        Id = id,
                        UniqueName = ((StringColumnValue)columns[0]).Value,
                        Category = ((StringColumnValue)columns[1]).Value,
                        CreatedTime = ((DateTimeColumnValue)columns[2]).Value ?? DateTime.MinValue,
                        Size = Api.RetrieveColumnSize(table.Session, table, columnMap[BlobsTableColumns.Data]) ?? 0,
                        ReferenceId = ((GuidColumnValue)columns[4]).Value,
                        IsUncompleted = true
                    };
                }
            }
            return result;
        }

        private BlobInfo LoadBlobInfo(EsentTable table, IDictionary<string, JET_COLUMNID> columnMap)
        {
            var columns = new ColumnValue[]
            {
                new StringColumnValue()
                {
                    Columnid = columnMap[BlobsTableColumns.Name],
                },
                new StringColumnValue()
                {
                    Columnid = columnMap[BlobsTableColumns.Category],
                },
                new DateTimeColumnValue()
                {
                    Columnid = columnMap[BlobsTableColumns.CreatedDate],
                },
                new Int64ColumnValue()
                {
                    Columnid = columnMap[BlobsTableColumns.Length],
                },
                new GuidColumnValue()
                {
                    Columnid = columnMap[BlobsTableColumns.ReferenceId],
                },
                new Int32ColumnValue()
                {
                    Columnid = columnMap[BlobsTableColumns.Id],
                },
                new BoolColumnValue()
                {
                    Columnid = columnMap[BlobsTableColumns.IsCompleted],
                },
            };
            Api.RetrieveColumns(table.Session, table, columns);
            if (((BoolColumnValue) columns[6]).Value ?? false)
            {
                return new BlobInfo()
                {
                    Id = new BlobId() { Id = ((Int32ColumnValue)columns[5]).Value ?? 0 },
                    UniqueName = ((StringColumnValue)columns[0]).Value,
                    Category = ((StringColumnValue)columns[1]).Value,
                    CreatedTime = ((DateTimeColumnValue)columns[2]).Value ?? DateTime.MinValue,
                    Size = ((Int64ColumnValue)columns[3]).Value ?? 0,
                    ReferenceId = ((GuidColumnValue)columns[4]).Value,
                    IsUncompleted = false
                };
            }
            return new BlobInfo()
            {
                Id = new BlobId() { Id = ((Int32ColumnValue)columns[5]).Value ?? 0 },
                UniqueName = ((StringColumnValue)columns[0]).Value,
                Category = ((StringColumnValue)columns[1]).Value,
                CreatedTime = ((DateTimeColumnValue)columns[2]).Value ?? DateTime.MinValue,
                Size = Api.RetrieveColumnSize(table.Session, table, columnMap[BlobsTableColumns.Data]) ?? 0,
                ReferenceId = ((GuidColumnValue)columns[4]).Value,
                IsUncompleted = true
            };
        }

        private bool SeekBlob(EsentTable table, BlobId id, bool includeUncompleted)
        {
            Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
            if (!Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
            {
                return false;
            }
            if (!includeUncompleted)
            {
                return Api.RetrieveColumnAsBoolean(table.Session, table.Table, Api.GetTableColumnid(table.Session, table, BlobsTableColumns.IsCompleted)) ?? false;
            }
            return true;
        }

        /// <summary>
        /// Читать категорию.
        /// </summary>
        /// <param name="category">Категория.</param>
        /// <returns>Результат.</returns>
        public async Task<BlobInfo[]> ReadCategory(string category)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                List<BlobInfo> result = new List<BlobInfo>();
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        var columnMap = Api.GetColumnDictionary(table.Session, table);
                        Api.JetSetCurrentIndex(table.Session, table.Table, BlobsTableIndexes.Category);
                        Api.MakeKey(table.Session, table, category, Encoding.Unicode, MakeKeyGrbit.NewKey);
                        Api.MakeKey(table.Session, table, true, MakeKeyGrbit.None);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            do
                            {
                                result.Add(LoadBlobInfo(table, columnMap));
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                });
                return result.ToArray();
            });
        }

        /// <summary>
        /// Читать все файлы по ссылке.
        /// </summary>
        /// <param name="referenceId">Идентификатор ссылки.</param>
        /// <returns>Результат.</returns>
        public async Task<BlobInfo[]> ReadReferencedBlobs(Guid referenceId)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                List<BlobInfo> result = new List<BlobInfo>();
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        var columnMap = Api.GetColumnDictionary(table.Session, table);
                        Api.JetSetCurrentIndex(table.Session, table.Table, BlobsTableIndexes.ReferenceId);
                        Api.MakeKey(table.Session, table.Table, referenceId, MakeKeyGrbit.NewKey);
                        Api.MakeKey(table.Session, table, true, MakeKeyGrbit.None);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            do
                            {
                                result.Add(LoadBlobInfo(table, columnMap));
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                });
                return result.ToArray();
            });
        }

        /// <summary>
        /// Получить размер категории.
        /// </summary>
        /// <param name="category">Категория.</param>
        /// <returns>Размер категории.</returns>
        public async Task<long> GetCategorySize(string category)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                long result = 0;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        var columnMap = Api.GetColumnDictionary(table.Session, table);
                        Api.JetSetCurrentIndex(table.Session, table.Table, BlobsTableIndexes.Category);
                        Api.MakeKey(table.Session, table.Table, category, Encoding.Unicode, MakeKeyGrbit.NewKey);
                        Api.MakeKey(table.Session, table, true, MakeKeyGrbit.None);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            do
                            {
                                result += Api.RetrieveColumnAsInt64(table.Session, table.Table, columnMap[BlobsTableColumns.Length]) ?? 0;
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                });
                return result;
            });
        }

        /// <summary>
        /// Получить количество файлов в категории.
        /// </summary>
        /// <param name="category">Категория.</param>
        /// <returns>Количество файлов.</returns>
        public async Task<int> GetCategoryBlobsCount(string category)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                int result = 0;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(table.Session, table.Table, BlobsTableIndexes.Category);
                        Api.MakeKey(table.Session, table.Table, category, Encoding.Unicode, MakeKeyGrbit.NewKey);
                        Api.MakeKey(table.Session, table, true, MakeKeyGrbit.None);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            Api.JetIndexRecordCount(table.Session, table.Table, out result, int.MaxValue);
                        }
                    }
                });
                return result;
            });
        }

        /// <summary>
        /// Получить размер элементов со ссылкой.
        /// </summary>
        /// <param name="referenceId">Идентификатор ссылки.</param>
        /// <returns>Размер категории.</returns>
        public async Task<long> GetReferencedSize(Guid referenceId)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                long result = 0;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        var columnMap = Api.GetColumnDictionary(table.Session, table);
                        Api.JetSetCurrentIndex(table.Session, table.Table, BlobsTableIndexes.ReferenceId);
                        Api.MakeKey(table.Session, table.Table, referenceId, MakeKeyGrbit.NewKey);
                        Api.MakeKey(table.Session, table, true, MakeKeyGrbit.None);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            do
                            {
                                result += Api.RetrieveColumnAsInt64(table.Session, table.Table, columnMap[BlobsTableColumns.Length]) ?? 0;
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                });
                return result;
            });
        }

        /// <summary>
        /// Получить количество файлов со ссылкой.
        /// </summary>
        /// <param name="referenceId">Идентификатор ссылки.</param>
        /// <returns>Количество файлов.</returns>
        public async Task<int> GetReferencedBlobsCount(Guid referenceId)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                int result = 0;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(table.Session, table.Table, BlobsTableIndexes.ReferenceId);
                        Api.MakeKey(table.Session, table.Table, referenceId, MakeKeyGrbit.NewKey);
                        Api.MakeKey(table.Session, table, true, MakeKeyGrbit.None);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            Api.JetIndexRecordCount(table.Session, table.Table, out result, int.MaxValue);
                        }
                    }
                });
                return result;
            });
        }

        /// <summary>
        /// Добавить постоянную ссылку.
        /// </summary>
        /// <param name="referenceId">Идентификатор ссылки.</param>
        public async Task AddPermanentReference(Guid referenceId)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            await UpdateAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(ReferencesTable, OpenTableGrbit.DenyWrite))
                    {
                        Api.MakeKey(table.Session, table, referenceId, MakeKeyGrbit.NewKey);
                        if (!Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            using (var update = new Update(table.Session, table, JET_prep.Insert))
                            {
                                Api.SetColumn(table.Session, table, Api.GetTableColumnid(table.Session, table, ReferencesTableColumns.ReferenceId), referenceId);
                                update.Save();
                            }
                        }
                    }
                    return true;
                });
                return Nothing.Value;
            });
        }

        /// <summary>
        /// Удалить постоянную ссылку.
        /// </summary>
        /// <param name="referenceId">Идентификатор ссылки.</param>
        public async Task RemovePermanentReference(Guid referenceId)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            await UpdateAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(ReferencesTable, OpenTableGrbit.DenyWrite))
                    {
                        Api.MakeKey(table.Session, table, referenceId, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            Api.JetDelete(table.Session, table);
                        }
                    }
                    return true;
                });
                return Nothing.Value;
            });
        }

        /// <summary>
        /// Проверить являются ли ссылки постояными.
        /// </summary>
        /// <param name="references">Массив ссылок.</param>
        /// <returns>Массив постоянных ссылок.</returns>
        public async Task<Guid[]> CheckIfReferencesPermanent(Guid[] references)
        {
            if (references == null) throw new ArgumentNullException(nameof(references));

            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                HashSet<Guid> result = new HashSet<Guid>();
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(ReferencesTable, OpenTableGrbit.ReadOnly))
                    {
                        foreach (var referenceId in references)
                        {
                            Api.MakeKey(table.Session, table, referenceId, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                            {
                                result.Add(referenceId);
                            }
                        }
                        return false;
                    }
                });
                return result.ToArray();
            });
        }

        /// <summary>
        /// Удалить все файлы.
        /// </summary>
        public async Task DeleteAllBlobs()
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            await UpdateAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                    {
                        if (Api.TryMoveFirst(table.Session, table))
                        {
                            do
                            {
                                Api.JetDelete(table.Session, table);
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                    return true;
                });
                return Nothing.Value;
            });
        }

        /// <summary>
        /// Удалить все ссылки.
        /// </summary>
        public async Task DeleteAllReferences()
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            await UpdateAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(ReferencesTable, OpenTableGrbit.DenyWrite))
                    {
                        if (Api.TryMoveFirst(table.Session, table))
                        {
                            do
                            {
                                Api.JetDelete(table.Session, table);
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                    return true;
                });
                return Nothing.Value;
            });
        }

        private Task<Nothing> DoDeleteAllUncompletedBlobs()
        {
            return UpdateAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                    {
                        Api.JetSetCurrentIndex(table.Session, table, BlobsTableIndexes.IsCompleted);
                        Api.MakeKey(table.Session, table, false, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            do
                            {
                                Api.JetDelete(table.Session, table);
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                    return true;
                });
                return Nothing.Value;
            });
        }

        /// <summary>
        /// Удалить все не завершённые файлы.
        /// </summary>
        public async Task DeleteAllUncompletedBlobs()
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            await DoDeleteAllUncompletedBlobs();
        }

        /// <summary>
        /// Получить количество всех файлов.
        /// </summary>
        /// <returns>Все файлы.</returns>
        public async Task<int> GetBlobsCount()
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                int result = 0;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(table.Session, table.Table, BlobsTableIndexes.IsCompleted);
                        Api.MakeKey(table.Session, table, true, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            Api.JetIndexRecordCount(table.Session, table.Table, out result, int.MaxValue);
                        }
                    }
                });
                return result;
            });
        }

        /// <summary>
        /// Получить общий размер.
        /// </summary>
        /// <returns></returns>
        public async Task<long> GetTotalSize()
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                long result = 0;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        var columnMap = Api.GetColumnDictionary(table.Session, table);
                        Api.JetSetCurrentIndex(table.Session, table.Table, BlobsTableIndexes.IsCompleted);
                        Api.MakeKey(table.Session, table, true, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            do
                            {
                                result += Api.RetrieveColumnAsInt64(table.Session, table.Table, columnMap[BlobsTableColumns.Length]) ?? 0;
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                });
                return result;
            });
        }

        /// <summary>
        /// Получить количество всех незавершённых файлов.
        /// </summary>
        /// <returns>Все файлы.</returns>
        public async Task<int> GetUncompletedBlobsCount()
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                int result = 0;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        Api.JetSetCurrentIndex(table.Session, table.Table, BlobsTableIndexes.IsCompleted);
                        Api.MakeKey(table.Session, table, false, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            Api.JetIndexRecordCount(table.Session, table.Table, out result, int.MaxValue);
                        }
                    }
                });
                return result;
            });
        }

        /// <summary>
        /// Получить общий размер незавершённых файлов.
        /// </summary>
        /// <returns></returns>
        public async Task<long> GetUncompletedTotalSize()
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                long result = 0;
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        var columnMap = Api.GetColumnDictionary(table.Session, table);
                        Api.JetSetCurrentIndex(table.Session, table.Table, BlobsTableIndexes.IsCompleted);
                        Api.MakeKey(table.Session, table, false, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            do
                            {
                                result += Api.RetrieveColumnSize(table.Session, table.Table, columnMap[BlobsTableColumns.Data]) ?? 0;
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                });
                return result;
            });
        }

        /// <summary>
        /// Найти незавершённые файлы.
        /// </summary>
        /// <returns>Список файлов.</returns>
        public async Task<BlobId[]> FindUncompletedBlobs()
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await QueryReadonly(async session =>
            {
                List<BlobId> result = new List<BlobId>();
                await session.Run(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        var columnMap = Api.GetColumnDictionary(table.Session, table);
                        Api.JetSetCurrentIndex(table.Session, table.Table, BlobsTableIndexes.IsCompleted);
                        Api.MakeKey(table.Session, table, false, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            do
                            {
                                result.Add(new BlobId() { Id = Api.RetrieveColumnAsInt32(table.Session, table, columnMap[BlobsTableColumns.Id]) ?? 0});
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                });
                return result.ToArray();
            });
        }

        /// <summary>
        /// Для юнит-тестов. Пометить файл как незавершённый.
        /// </summary>
        /// <param name="id">Идентификатор файла.</param>
        /// <returns>true, если файл найден и помечен.</returns>
        public async Task<bool> MarkUncompleted(BlobId id)
        {
            CheckModuleReady();
            await WaitForTablesInitialize();
            return await UpdateAsync(async session =>
            {
                var result = false;
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                    {
                        var columnMap = Api.GetColumnDictionary(table.Session, table);
                        if (SeekBlob(table, id, false))
                        {
                            using (var update = new Update(table.Session, table, JET_prep.Replace))
                            {
                                Api.SetColumn(table.Session, table, columnMap[BlobsTableColumns.IsCompleted], false);
                                update.Save();
                                result = true;
                            }
                        }
                    }
                    return true;
                });
                return result;
            });
        }
    }
}