using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Windows.Storage;
using Imageboard10.Core.Database;
using Imageboard10.Core.IO;
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
        private StorageFolder _filestreamFolder;

        /// <summary>
        /// Создать или обновить таблицы.
        /// </summary>
        protected override async ValueTask<Nothing> CreateOrUpgradeTables()
        {
            await base.CreateOrUpgradeTables();
            await EnsureTable(BlobsTable, 1, InitializeBlobsTable, null);
            await EnsureTable(ReferencesTable, 1, InitializeReferencesTable, null);
            _filestreamFolder = await CreateFilestreamTable();
            try
            {
                await DoDeleteAllUncompletedBlobs();
            }
            catch (Exception e)
            {
                GlobalErrorHandler?.SignalError(e);
            }
            return Nothing.Value;
        }

        private async Task<StorageFolder> CreateFilestreamTable()
        {
            if (EsentProvider.Purge)
            {
                return await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("filestream", CreationCollisionOption.ReplaceExisting);
            }
            else
            {
                return await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("filestream", CreationCollisionOption.OpenIfExists);
            }
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
                grbit = ColumndefGrbit.None,
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
                grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobsTableColumns.ReferenceId, new JET_COLUMNDEF()
            {
                coltyp = VistaColtyp.GUID,
                grbit = ColumndefGrbit.ColumnTagged
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobsTableColumns.IsCompleted, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Bit,
                grbit = ColumndefGrbit.ColumnNotNULL
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobsTableColumns.IsFilestream, new JET_COLUMNDEF()
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
            public bool IsFilestream;
        }

        private async Task<SemiMemoryStream> DumpToTempStream(Stream inStream, long? maxSize, byte[] buffer, CancellationToken token)
        {
            SemiMemoryStream result = new SemiMemoryStream(64*1024);
            try
            {
                var toRead = maxSize ?? long.MaxValue;
                do
                {
                    var szToRead = (int)Math.Min(buffer.Length, toRead);
                    if (szToRead <= 0)
                    {
                        break;
                    }

                    var sz = await inStream.ReadAsync(buffer, 0, szToRead, token);
                    if (sz <= 0)
                    {
                        break;
                    }

                    await result.WriteAsync(buffer, 0, sz, token);

                    if (sz < szToRead)
                    {
                        break;
                    }

                    if (result.Length > MaxFileSize)
                    {
                        throw new BlobException("Слишком большой размер файла");
                    }
                } while (true);
                return result;
            }
            catch
            {
                await result.DisposeAsync();
                throw;
            }
        }

        private string BlobFileName(int blobId) => $"{blobId}.blob";

        private readonly ConcurrentDictionary<BlobId, string> _tempPaths = new ConcurrentDictionary<BlobId, string>();

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

            return await OpenSessionAsync(async session =>
            {
                token.ThrowIfCancellationRequested();

                byte[] buffer = new byte[64 * 1024];

                int blobId = 0;
                byte[] bookmark = null;
                int bookmarkSize = 0;
                bool isFilestream = false;
                long tmpLength = 0;

                try
                {
                    using (var tmpStream = await DumpToTempStream(blob.BlobStream, blob.MaxSize, buffer, token))
                    {                        
                        var frs = await session.RunInTransaction(() =>
                        {
                            using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
                            {
                                var sid = table.Session;
                                var columnMap = Api.GetColumnDictionary(sid, table);
                                // ReSharper disable once AccessToDisposedClosure
                                var isFile = tmpStream.Length >= FileStreamSize;
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
                                        new BoolColumnValue()
                                        {
                                            Columnid = columnMap[BlobsTableColumns.IsFilestream],
                                            // ReSharper disable once AccessToDisposedClosure
                                            Value = isFile
                                        },
                                    };
                                    Api.SetColumns(sid, table, columns);
                                    update.Save(bmark, bmark.Length, out bsize);
                                    // ReSharper disable once AccessToDisposedClosure
                                    return (true, new SaveId() {Id = bid, Bookmark = bmark, BookmarkSize = bsize, IsFilestream = isFile });
                                }
                            }
                        });
                        blobId = frs.Id;
                        bookmark = frs.Bookmark;
                        bookmarkSize = frs.BookmarkSize;
                        isFilestream = frs.IsFilestream;

                        if (isFilestream)
                        {
                            tmpStream.MoveAfterClose(_filestreamFolder, BlobFileName(blobId));
                        }
                        else
                        {
                            tmpStream.Position = 0;
                            long counter = 0;
                            do
                            {
                                var counter2 = counter;

                                var sz = await tmpStream.ReadAsync(buffer, 0, buffer.Length, token);
                                if (sz <= 0)
                                {
                                    break;
                                }

                                await session.RunInTransaction(() =>
                                {
                                    token.ThrowIfCancellationRequested();
                                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
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
                                }, 1.5);

                                counter += sz;
                                if (sz < buffer.Length)
                                {
                                    break;
                                }
                            } while (true);
                        }
                        tmpLength = tmpStream.Length;
                        if (blob.RememberTempFile)
                        {
                            _tempPaths[new BlobId() {Id = blobId}] = tmpStream.TempFilePath;
                        }
                    }

                    await session.RunInTransaction(() =>
                    {
                        using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
                        {
                            var sid = table.Session;
                            var columnMap = Api.GetColumnDictionary(sid, table);
                            if (!Api.TryGotoBookmark(table.Session, table, bookmark, bookmarkSize))
                            {
                                throw new BlobException($"Неверные данные в таблице {BlobsTable}, ID={blobId}");
                            }
                            using (var update = new Update(sid, table, JET_prep.Replace))
                            {
                                var size = tmpLength;
                                Api.SetColumn(sid, table, columnMap[BlobsTableColumns.Length], (long)size);
                                Api.SetColumn(sid, table, columnMap[BlobsTableColumns.IsCompleted], true);
                                update.Save();
                            }
                            return true;
                        }
                    }, 1.5);
                }
                catch (OperationCanceledException)
                {
                    if (bookmark == null)
                    {
                        throw;
                    }
                    try
                    {
                        await session.RunInTransaction(() =>
                        {
                            using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
                            {
                                if (Api.TryGotoBookmark(table.Session, table, bookmark, bookmarkSize))
                                {
                                    Api.JetDelete(table.Session, table);
                                }
                                return true;
                            }
                        }, 1.5);
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
                    if (bookmark == null)
                    {
                        throw;
                    }
                    try
                    {
                        await session.RunInTransaction(() =>
                        {
                            using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
                            {
                                if (Api.TryGotoBookmark(table.Session, table, bookmark, bookmarkSize))
                                {
                                    Api.JetDelete(table.Session, table);
                                }
                                return true;
                            }
                        }, 1.5);
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
            return await OpenSessionAsync(async session =>
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

            return await OpenSessionAsync(async session =>
            {
                BlobStreamBase result = null;
                BlobId? filestream = null;

                void DoLoad()
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.ReadOnly))
                    {
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            var columnMap = Api.GetColumnDictionary(table.Session, table);
                            var isFilestream = Api.RetrieveColumnAsBoolean(table.Session, table, columnMap[BlobsTableColumns.IsFilestream]) ?? false;
                            if (isFilestream)
                            {
                                //result = new InlineFileStream(GlobalErrorHandler, await _filestreamFolder.OpenStreamForReadAsync(BlobFileName(id.Id)));
                                filestream = id;
                                return;
                            }
                            else
                            {
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
                                    var data = ((BytesColumnValue)columns[0]).Value;
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
                        }
                        else
                        {
                            throw new BlobNotFoundException(id);
                        }
                    }
                }

                await session.Run(DoLoad);

                if (filestream != null)
                {
                    return new InlineFileStream(GlobalErrorHandler, await _filestreamFolder.OpenStreamForReadAsync(BlobFileName(id.Id)));
                }

                if (result == null)
                {
                    throw new BlobException($"Неверные данные в таблице {BlobsTable}");
                }
                return result;
            });
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
            return await OpenSessionAsync(async session =>
            {
                bool isFileStream = false;
                byte[] bookmark = null;
                bool result = false;
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
                    {
                        Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table.Table, SeekGrbit.SeekEQ))
                        {
                            isFileStream = Api.RetrieveColumnAsBoolean(table.Session, table, Api.GetTableColumnid(table.Session, table, BlobsTableColumns.IsFilestream)) ?? false;
                            if (!isFileStream)
                            {
                                Api.JetDelete(table.Session, table);
                                result = true;
                            }
                            else
                            {
                                bookmark = Api.GetBookmark(table.Session, table);
                            }
                        }
                        return true;
                    }
                }, 1.5);
                if (isFileStream)
                {
                    bool isDeleted;
                    try
                    {
                        await (await _filestreamFolder.GetFileAsync(BlobFileName(id.Id))).DeleteAsync();
                        isDeleted = true;
                    }
                    catch
                    {
                        isDeleted = false;
                    }
                    if (isDeleted)
                    {
                        await session.RunInTransaction(() =>
                        {
                            using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
                            {
                                if (Api.TryGotoBookmark(table.Session, table, bookmark, bookmark.Length))
                                {
                                    Api.JetDelete(table.Session, table);
                                    result = true;
                                }
                            }
                            return true;
                        }, 1.5);
                    }
                }
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
            return await OpenSessionAsync(async session =>
            {
                var result = new List<BlobId>();
                var filestream = new List<(BlobId id, byte[] bookmark)>();
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
                    {
                        foreach (var id in idArray)
                        {
                            Api.MakeKey(table.Session, table, id.Id, MakeKeyGrbit.NewKey);
                            if (Api.TrySeek(table.Session, table.Table, SeekGrbit.SeekEQ))
                            {
                                var isFileStream = Api.RetrieveColumnAsBoolean(table.Session, table, Api.GetTableColumnid(table.Session, table, BlobsTableColumns.IsFilestream)) ?? false;
                                if (!isFileStream)
                                {
                                    Api.JetDelete(table.Session, table);
                                    result.Add(id);
                                }
                                else
                                {
                                    filestream.Add((id, Api.GetBookmark(table.Session, table)));
                                }
                            }
                        }
                    }
                    return true;
                }, 1.5);

                async Task<(BlobId? id, byte[] bookmark)> DeleteFilestream(BlobId id, byte[] bookmark)
                {
                    try
                    {
                        await (await _filestreamFolder.GetFileAsync(BlobFileName(id.Id))).DeleteAsync();
                        return (id, bookmark);
                    }
                    catch
                    {
                        return (null, null);
                    }
                }

                var tasks = filestream.Select(f => DeleteFilestream(f.id, f.bookmark)).ToArray();
                var res = (await Task.WhenAll(tasks)).Where(f => f.id != null);

                foreach (var rid in res)
                {
                    var id = rid.id.Value;
                    var bookmark = rid.bookmark;
                    await session.RunInTransaction(() =>
                    {
                        using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
                        {
                            if (Api.TryGotoBookmark(table.Session, table, bookmark, bookmark.Length))
                            {
                                Api.JetDelete(table.Session, table);
                                result.Add(id);
                            }
                        }
                        return true;
                    }, 1.5);
                }

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
            return await OpenSessionAsync(async session =>
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
                return await AddFileSize(result);
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
                    new BoolColumnValue()
                    {
                        Columnid = columnMap[BlobsTableColumns.IsFilestream],
                    },
                };
                Api.RetrieveColumns(table.Session, table, columns);
                var isFileStream = ((BoolColumnValue) columns[6]).Value ?? false;
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
                        IsUncompleted = false,
                        IsFilestream = isFileStream
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
                        IsUncompleted = true,
                        IsFilestream = isFileStream
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
                new BoolColumnValue()
                {
                    Columnid = columnMap[BlobsTableColumns.IsFilestream],
                },
            };
            Api.RetrieveColumns(table.Session, table, columns);
            var isFileStream = ((BoolColumnValue) columns[7]).Value ?? false;
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
                    IsUncompleted = false,
                    IsFilestream = isFileStream
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
                IsUncompleted = true,
                IsFilestream = isFileStream
            };
        }

        private async ValueTask<BlobInfo?> AddFileSize(BlobInfo? info)
        {
            if (info == null || !info.Value.IsFilestream || !info.Value.IsUncompleted)
            {
                return info;
            }
            var r = info.Value;
            try
            {
                var f = await _filestreamFolder.GetFileAsync(BlobFileName(r.Id.Id));
                var p = await f.GetBasicPropertiesAsync();
                r.Size = (long) p.Size;
            }
            catch
            {
                r.Size = -1;
            }
            return r;
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
            return await OpenSessionAsync(async session =>
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
                var result2 = new List<BlobInfo>();
                foreach (var r in result)
                {
                    var ra = await AddFileSize(r);
                    if (ra != null)
                    {
                        result2.Add(ra.Value);
                    }
                }
                return result2.ToArray();
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
            return await OpenSessionAsync(async session =>
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
                var result2 = new List<BlobInfo>();
                foreach (var r in result)
                {
                    var ra = await AddFileSize(r);
                    if (ra != null)
                    {
                        result2.Add(ra.Value);
                    }
                }
                return result2.ToArray();
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
            return await OpenSessionAsync(async session =>
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
            return await OpenSessionAsync(async session =>
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
            return await OpenSessionAsync(async session =>
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
            return await OpenSessionAsync(async session =>
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
            await OpenSessionAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(ReferencesTable, OpenTableGrbit.None))
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
                }, 1.5);
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
            await OpenSessionAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(ReferencesTable, OpenTableGrbit.None))
                    {
                        Api.MakeKey(table.Session, table, referenceId, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ))
                        {
                            Api.JetDelete(table.Session, table);
                        }
                    }
                    return true;
                }, 1.5);
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
            return await OpenSessionAsync(async session =>
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
                }, 1.5);
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

            await OpenSessionAsync(async session =>
            {
                var filestream = new List<BlobId>();
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
                    {
                        if (Api.TryMoveFirst(table.Session, table))
                        {
                            do
                            {
                                if (Api.RetrieveColumnAsBoolean(table.Session, table, Api.GetTableColumnid(table.Session, table, BlobsTableColumns.IsFilestream)) ?? false)
                                {
                                    filestream.Add(new BlobId()
                                    {
                                        Id = Api.RetrieveColumnAsInt32(table.Session, table, Api.GetTableColumnid(table.Session, table, BlobsTableColumns.Id)) ?? -1
                                    });
                                }
                                Api.JetDelete(table.Session, table);
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                    return true;
                }, 1.5);

                async Task DeleteFilestream(BlobId id)
                {
                    try
                    {
                        var f = await _filestreamFolder.GetFileAsync(BlobFileName(id.Id));
                        await f.DeleteAsync();
                    }
                    catch (Exception)
                    {
                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
                    }
                }

                if (filestream.Count > 0)
                {
                    await Task.WhenAll(filestream.Select(DeleteFilestream).ToArray());
                }

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
            await OpenSessionAsync(async session =>
            {
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(ReferencesTable, OpenTableGrbit.None))
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
                }, 1.5);
                return Nothing.Value;
            });
        }

        private ValueTask<Nothing> DoDeleteAllUncompletedBlobs()
        {
            return OpenSessionAsync(async session =>
            {
                var filestream = new List<BlobId>();
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
                    {
                        Api.JetSetCurrentIndex(table.Session, table, BlobsTableIndexes.IsCompleted);
                        Api.MakeKey(table.Session, table, false, MakeKeyGrbit.NewKey);
                        if (Api.TrySeek(table.Session, table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
                        {
                            do
                            {
                                if (Api.RetrieveColumnAsBoolean(table.Session, table, Api.GetTableColumnid(table.Session, table, BlobsTableColumns.IsFilestream)) ?? false)
                                {
                                    filestream.Add(new BlobId()
                                    {
                                        Id = Api.RetrieveColumnAsInt32(table.Session, table, Api.GetTableColumnid(table.Session, table, BlobsTableColumns.Id)) ?? -1
                                    });
                                }
                                Api.JetDelete(table.Session, table);
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                    return true;
                }, 1.5);
                async Task DeleteFilestream(BlobId id)
                {
                    try
                    {
                        var f = await _filestreamFolder.GetFileAsync(BlobFileName(id.Id));
                        await f.DeleteAsync();
                    }
                    catch (Exception)
                    {
                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
                    }
                }

                var filestream1 = filestream.Where(f => f.Id >= 0).ToList();

                if (filestream1.Count > 0)
                {
                    await Task.WhenAll(filestream1.Select(DeleteFilestream).ToArray());
                }
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
            return await OpenSessionAsync(async session =>
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
            return await OpenSessionAsync(async session =>
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
            return await OpenSessionAsync(async session =>
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
            return await OpenSessionAsync(async session =>
            {
                long result = 0;
                var filestream = new List<BlobId>();
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
                                if (Api.RetrieveColumnAsBoolean(table.Session, table, columnMap[BlobsTableColumns.IsFilestream]) ?? false)
                                {
                                    filestream.Add(new BlobId()
                                    {
                                        Id = Api.RetrieveColumnAsInt32(table.Session, table.Table, columnMap[BlobsTableColumns.Id]) ?? -1
                                    });
                                }
                                else
                                {
                                    result += Api.RetrieveColumnSize(table.Session, table.Table, columnMap[BlobsTableColumns.Data]) ?? 0;
                                }
                            } while (Api.TryMoveNext(table.Session, table));
                        }
                    }
                });

                foreach (var fid in filestream)
                {
                    var f = await _filestreamFolder.GetFileAsync(BlobFileName(fid.Id));
                    var p = await f.GetBasicPropertiesAsync();
                    result += (long) p.Size;
                }
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
            return await OpenSessionAsync(async session =>
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
            return await OpenSessionAsync(async session =>
            {
                var result = false;
                await session.RunInTransaction(() =>
                {
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.None))
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
                }, 1.5);
                return result;
            });
        }

        /// <summary>
        /// Для юнит тестов. Проверка на наличие Файла.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Результат.</returns>
        public async Task<bool> IsFilePresent(BlobId id)
        {
            try
            {
                var f = await _filestreamFolder.GetFileAsync(BlobFileName(id.Id));
                var s = await f.GetBasicPropertiesAsync();
                return s.Size > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Для юнит-тестов.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Получить путь к временному файлу.</returns>
        public string GetTempFilePath(BlobId id)
        {
            if (_tempPaths.TryGetValue(id, out var v))
            {
                return v;
            }
            return null;
        }
    }
}