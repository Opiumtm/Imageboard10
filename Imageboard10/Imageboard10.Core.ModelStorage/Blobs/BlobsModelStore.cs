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

            Api.JetAddColumn(sid, tableid, BlobTableColumns.CreatedDate, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.DateTime,
                grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempid);

            Api.JetAddColumn(sid, tableid, BlobTableColumns.Data, new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.LongBinary,
                grbit = ColumndefGrbit.ColumnMaybeNull,
            }, null, 0, out tempid);

            var pkDef = $"+{BlobTableColumns.Id}\0\0";
            var nameDef = $"+{BlobTableColumns.Name}\0\0";
            var categoryDef = $"+{BlobTableColumns.Category}\0\0";
            Api.JetCreateIndex(sid, tableid, BlobTableIndexes.Primary, CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique, pkDef, pkDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, BlobTableIndexes.Name, CreateIndexGrbit.IndexUnique, nameDef, nameDef.Length, 100);
            Api.JetCreateIndex(sid, tableid, BlobTableIndexes.Category, CreateIndexGrbit.None, categoryDef, categoryDef.Length, 100);
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
                await session.Run(() =>
                {
                    var sid = session.Session;
                    using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
                    {
                        var columnMap = Api.GetColumnDictionary(sid, table);
                        using (var transaction = new Transaction(sid))
                        {
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
                                    new DateTimeColumnValue()
                                    {
                                        Columnid = columnMap[BlobTableColumns.CreatedDate],
                                        Value = DateTime.Now
                                    },
                                    new BytesColumnValue()
                                    {
                                        Columnid = columnMap[BlobTableColumns.Data],
                                        Value = new byte[0]
                                    },
                                    new Int64ColumnValue()
                                    {
                                        Columnid = columnMap[BlobTableColumns.Length],
                                        Value = null
                                    },
                                };
                                Api.SetColumns(sid, table, columns);
                                update.Save();
                            }
                            transaction.Commit(CommitTransactionGrbit.None);
                        }

                        byte[] buffer = new byte[64 * 1024];
                        long lastSz;
                        try
                        {
                            var toRead = blob.MaxSize ?? long.MaxValue;
                            do
                            {
                                var szToRead = (int)Math.Min(buffer.Length, toRead);
                                if (szToRead <= 0)
                                {
                                    break;
                                }

                                var sz = blob.BlobStream.Read(buffer, 0, szToRead);
                                if (sz <= 0)
                                {
                                    break;
                                }

                                using (var transaction = new Transaction(sid))
                                {
                                    Api.MakeKey(sid, table, blobId, MakeKeyGrbit.NewKey);
                                    if (!Api.TrySeek(sid, table, SeekGrbit.SeekEQ))
                                    {
                                        throw new BlobException($"Неверные данные в таблице {BlobsTable}");
                                    }
                                    using (var update = new Update(sid, table, JET_prep.Replace))
                                    {
                                        using (var str = new ColumnStream(sid, table, columnMap[BlobTableColumns.Data]))
                                        {
                                            str.Seek(0, SeekOrigin.End);
                                            str.Write(buffer, 0, sz);
                                        }
                                        update.Save();
                                    }
                                    transaction.Commit(CommitTransactionGrbit.None);
                                }

                                if (sz < szToRead)
                                {
                                    break;
                                }
                            } while (true);

                            using (var transaction = new Transaction(sid))
                            {
                                Api.MakeKey(sid, table, blobId, MakeKeyGrbit.NewKey);
                                if (!Api.TrySeek(sid, table, SeekGrbit.SeekEQ))
                                {
                                    throw new BlobException($"Неверные данные в таблице {BlobsTable}");
                                }
                                using (var update = new Update(sid, table, JET_prep.Replace))
                                {
                                    var size = Api.RetrieveColumnSize(table.Session, table, columnMap[BlobTableColumns.Length]) ?? 0;
                                    Api.SetColumn(sid, table, columnMap[BlobTableColumns.Length], size);
                                    update.Save();
                                }
                                transaction.Commit(CommitTransactionGrbit.None);
                            }
                        }
                        catch
                        {
                            using (var transaction = new Transaction(sid))
                            {
                                DoDeleteBlob(session, new BlobId() { Id = blobId });
                                transaction.Commit(CommitTransactionGrbit.None);
                            }
                            throw;
                        }
                    }
                });

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
                            var size = Api.RetrieveColumnSize(table.Session, table, columnMap[BlobTableColumns.Length]) ?? 0;
                            if (size <= MaxInlineSize)
                            {
                                var columns = new ColumnValue[]
                                {
                                    new ByteColumnValue()
                                    {
                                        Columnid = columnMap[BlobTableColumns.Length],
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

        private void DoDeleteBlob(IEsentSession session, BlobId blobId)
        {
            var sid = session.Session;
            using (var table = session.OpenTable(BlobsTable, OpenTableGrbit.DenyWrite))
            {
                Api.MakeKey(sid, table, blobId.Id, MakeKeyGrbit.NewKey);
                if (Api.TrySeek(sid, table, SeekGrbit.SeekEQ))
                {
                    Api.JetDelete(sid, table);
                }
            }
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
    }
}