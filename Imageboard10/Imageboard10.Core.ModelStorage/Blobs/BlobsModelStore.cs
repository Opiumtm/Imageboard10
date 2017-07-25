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
            await EnsureTable(BlobsTableName, 1, InitializeBlobsTable, null);
            await EnsureTable(ReferencesTableName, 1, InitializeReferencesTable, null);
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
            BlobsTable.CreateColumnsAndIndexes(session.Session, tableid);
        }

        private void InitializeReferencesTable(IEsentSession session, JET_TABLEID tableid)
        {
            ReferencesTable.CreateColumnsAndIndexes(session.Session, tableid);
        }

        private BlobsTable OpenBlobsTable(IEsentSession session, OpenTableGrbit grbit)
        {
            var r = session.OpenTable(BlobsTableName, grbit);
            return new BlobsTable(r.Session, r.Table);
        }

        private ReferencesTable OpenReferencesTable(IEsentSession session, OpenTableGrbit grbit)
        {
            var r = session.OpenTable(ReferencesTableName, grbit);
            return new ReferencesTable(r.Session, r.Table);
        }

        private struct SaveId
        {
            public int Id;
            public byte[] Bookmark;
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
                bool isFilestream = false;
                byte[] bookmark = null;
                long tmpLength = 0;

                try
                {
                    using (var tmpStream = await DumpToTempStream(blob.BlobStream, blob.MaxSize, buffer, token))
                    {                        
                        var frs = await session.RunInTransaction(() =>
                        {
                            using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                            {
                                var sid = table.Session;
                                // ReSharper disable once AccessToDisposedClosure
                                var isFile = tmpStream.Length >= FileStreamSize;
                                var updateData = new BlobsTable.ViewValues.FullRowUpdate()
                                {
                                    Name = blob.UniqueName,
                                    Category = blob.Category,
                                    CreatedDate = DateTime.Now,
                                    Data = new byte[0],
                                    Length = 0,
                                    ReferenceId = blob.ReferenceId,
                                    IsCompleted = false,
                                    IsFilestream = isFile
                                };
                                using (var update = table.Insert.CreateUpdate())
                                {
                                    var bid = table.Columns.Id_AutoincrementValue;
                                    table.Insert.FullRowUpdate.Set(ref updateData);
                                    table.Insert.SaveUpdateWithBookmark(update, out var bmark);
                                    // ReSharper disable once AccessToDisposedClosure
                                    return (true, new SaveId() {Id = bid, Bookmark = bmark, IsFilestream = isFile });
                                }
                            }
                        });
                        blobId = frs.Id;
                        bookmark = frs.Bookmark;
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
                                    using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                                    {
                                        var sid = table.Session;

                                        if (!table.TryGotoBookmark(bookmark))
                                        {
                                            throw new BlobException($"Неверные данные в таблице {BlobsTableName}, ID={blobId}, pos={counter2}");
                                        }
                                        using (var update = table.Update.CreateUpdate())
                                        {
                                            using (var str = new ColumnStream(sid, table, table.GetColumnid(BlobsTable.Column.Data)))
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
                        using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                        {
                            if (!table.TryGotoBookmark(bookmark))
                            {
                                throw new BlobException($"Неверные данные в таблице {BlobsTableName}, ID={blobId}");
                            }
                            var size = tmpLength;
                            table.Update.UpdateAsCompletedUpdate(new BlobsTable.ViewValues.CompletedUpdate()
                            {
                                Length = (long)size,
                                IsCompleted = true
                            });
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
                            using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                            {
                                if (table.TryGotoBookmark(bookmark))
                                {
                                    table.DeleteCurrentRow();
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
                            using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                            {
                                if (table.TryGotoBookmark(bookmark))
                                {
                                    table.DeleteCurrentRow();
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.NameIndex.SetAsCurrentIndex();
                        if (table.Indexes.NameIndex.Find(table.Indexes.NameIndex.CreateKey(uniqueName)))
                        {
                            if (table.Columns.IsCompleted)
                            {
                                result = new BlobId()
                                {
                                    Id = table.Views.IdFromIndexView.Fetch().Id
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(id.Id)))
                        {
                            var isFilestream = table.Columns.IsFilestream;
                            if (isFilestream)
                            {
                                filestream = id;
                                return;
                            }
                            else
                            {
                                var size = Api.RetrieveColumnSize(table.Session, table, table.GetColumnid(BlobsTable.Column.Data)) ?? 0;
                                if (size <= MaxInlineSize)
                                {
                                    var data = table.Columns.Data;
                                    if (data == null)
                                    {
                                        throw new BlobException($"Неверные данные в таблице {BlobsTableName}");
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
                    throw new BlobException($"Неверные данные в таблице {BlobsTableName}");
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                    {
                        if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(id.Id)))
                        {
                            isFileStream = table.Columns.IsFilestream;
                            if (!isFileStream)
                            {
                                table.DeleteCurrentRow();
                                result = true;
                            }
                            else
                            {
                                bookmark = table.GetBookmark();
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
                            using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                            {
                                if (table.TryGotoBookmark(bookmark))
                                {
                                    table.DeleteCurrentRow();
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                    {
                        foreach (var id in idArray)
                        {
                            if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(id.Id)))
                            {
                                var isFileStream = table.Columns.IsFilestream;
                                if (!isFileStream)
                                {
                                    table.DeleteCurrentRow();
                                    result.Add(id);
                                }
                                else
                                {
                                    filestream.Add((id, table.GetBookmark()));
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
                var res = (await Task.WhenAll(tasks)).Where(f => f.id != null).ToArray();

                var deleted = await session.RunInTransaction(() =>
                {
                    var result2 = new List<BlobId>();
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                    {
                        foreach (var rid in res)
                        {
                            var id = rid.id.Value;
                            var bookmark = rid.bookmark;
                            if (table.TryGotoBookmark(bookmark))
                            {
                                table.DeleteCurrentRow();
                                result2.Add(id);
                            }
                        }
                    }
                    return (true, result2);
                }, 1.5);

                result.AddRange(deleted);
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        result = LoadBlobInfo(table, id);
                    }
                });
                return await AddFileSize(result);
            });
        }

        private BlobInfo? LoadBlobInfo(BlobsTable table, BlobId id)
        {
            BlobInfo? result = null;
            if (SeekBlob(table, id, false))
            {
                var data = table.Views.FullRowUpdate.Fetch();
                result = new BlobInfo()
                {
                    Id = id,
                    UniqueName = data.Name,
                    Category = data.Category,
                    CreatedTime = data.CreatedDate,
                    Size = data.Length,
                    ReferenceId = data.ReferenceId,
                    IsUncompleted = !data.IsCompleted,
                    IsFilestream = data.IsFilestream
                };
            }
            return result;
        }

        private BlobInfo LoadBlobInfo(BlobsTable table)
        {
            var data = table.Views.FullRowUpdate.Fetch();
            return new BlobInfo()
            {
                Id = new BlobId() { Id = table.Columns.Id },
                UniqueName = data.Name,
                Category = data.Category,
                CreatedTime = data.CreatedDate,
                Size = data.Length,
                ReferenceId = data.ReferenceId,
                IsUncompleted = !data.IsCompleted,
                IsFilestream = data.IsFilestream
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

        private bool SeekBlob(BlobsTable table, BlobId id, bool includeUncompleted)
        {
            if (!table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(id.Id)))
            {
                return false;
            }
            if (!includeUncompleted)
            {
                return table.Columns.IsCompleted;
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.CategoryIndex.SetAsCurrentIndex();
                        foreach (var _ in table.Indexes.CategoryIndex.Enumerate(table.Indexes.CategoryIndex.CreateKey(category, true)))
                        {
                            result.Add(LoadBlobInfo(table));
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.ReferenceIdIndex.SetAsCurrentIndex();
                        foreach (var _ in table.Indexes.ReferenceIdIndex.Enumerate(table.Indexes.ReferenceIdIndex.CreateKey(referenceId, true)))
                        {
                            result.Add(LoadBlobInfo(table));
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.CategoryIndex.SetAsCurrentIndex();
                        foreach (var _ in table.Indexes.CategoryIndex.Enumerate(table.Indexes.CategoryIndex.CreateKey(category, true)))
                        {
                            result += table.Columns.Length;
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
                return await session.Run(() =>
                {
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.CategoryIndex.SetAsCurrentIndex();
                        return table.Indexes.CategoryIndex.GetIndexRecordCount(table.Indexes.CategoryIndex.CreateKey(category, true));
                    }
                });
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.ReferenceIdIndex.SetAsCurrentIndex();
                        foreach (var _ in table.Indexes.ReferenceIdIndex.Enumerate(table.Indexes.ReferenceIdIndex.CreateKey(referenceId, true)))
                        {
                            result += table.Columns.Length;
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
                return await session.Run(() =>
                {
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.ReferenceIdIndex.SetAsCurrentIndex();
                        return table.Indexes.ReferenceIdIndex.GetIndexRecordCount(table.Indexes.ReferenceIdIndex.CreateKey(referenceId, true));
                    }
                });
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
                    using (var table = OpenReferencesTable(session, OpenTableGrbit.None))
                    {
                        if (!table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(referenceId)))
                        {
                            table.Insert.InsertAsReferenceView(new ReferencesTable.ViewValues.ReferenceView() { ReferenceId = referenceId });
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
                    using (var table = OpenReferencesTable(session, OpenTableGrbit.None))
                    {
                        if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(referenceId)))
                        {
                            table.DeleteCurrentRow();
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
                    using (var table = OpenReferencesTable(session, OpenTableGrbit.ReadOnly))
                    {
                        foreach (var referenceId in references)
                        {
                            if (table.Indexes.PrimaryIndex.Find(table.Indexes.PrimaryIndex.CreateKey(referenceId)))
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                    {
                        foreach (var _ in table.Enumerate())
                        {
                            if (table.Columns.IsFilestream)
                            {
                                filestream.Add(new BlobId() { Id = table.Columns.Id });
                            }
                            table.DeleteCurrentRow();
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
                    using (var table = OpenReferencesTable(session, OpenTableGrbit.None))
                    {
                        foreach (var _ in table.Enumerate())
                        {
                            table.DeleteCurrentRow();
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                    {
                        table.Indexes.IsCompletedIndex.SetAsCurrentIndex();
                        foreach (var _ in table.Indexes.IsCompletedIndex.Enumerate(table.Indexes.IsCompletedIndex.CreateKey(false)))
                        {
                            if (table.Columns.IsFilestream)
                            {
                                filestream.Add(new BlobId() { Id = table.Columns.Id });
                            }
                            table.DeleteCurrentRow();
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
                return await session.Run(() =>
                {
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.IsCompletedIndex.SetAsCurrentIndex();
                        return table.Indexes.IsCompletedIndex.GetIndexRecordCount(table.Indexes.IsCompletedIndex.CreateKey(true));
                    }
                });
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.IsCompletedIndex.SetAsCurrentIndex();
                        foreach (var _ in table.Indexes.IsCompletedIndex.Enumerate(table.Indexes.IsCompletedIndex.CreateKey(true)))
                        {
                            result += table.Columns.Length;
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
                return await session.Run(() =>
                {
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.IsCompletedIndex.SetAsCurrentIndex();
                        return table.Indexes.IsCompletedIndex.GetIndexRecordCount(table.Indexes.IsCompletedIndex.CreateKey(false));
                    }
                });
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
                var filestream = new List<BlobId>();
                var result = await session.Run(() =>
                {
                    long result1 = 0;
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.IsCompletedIndex.SetAsCurrentIndex();
                        foreach (var _ in table.Indexes.IsCompletedIndex.Enumerate(table.Indexes.IsCompletedIndex.CreateKey(false)))
                        {
                            if (table.Columns.IsFilestream)
                            {
                                filestream.Add(new BlobId() { Id = table.Columns.Id });
                            }
                            else
                            {
                                result1 += table.Columns.Length;
                            }
                        }
                    }
                    return result1;
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.ReadOnly))
                    {
                        table.Indexes.IsCompletedIndex.SetAsCurrentIndex();
                        foreach (var _ in table.Indexes.IsCompletedIndex.Enumerate(table.Indexes.IsCompletedIndex.CreateKey(false)))
                        {
                            result.Add(new BlobId() { Id = table.Views.IdFromIndexView.Fetch().Id });
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
                    using (var table = OpenBlobsTable(session, OpenTableGrbit.None))
                    {
                        if (SeekBlob(table, id, false))
                        {
                            using (var update = table.Update.CreateUpdate())
                            {
                                var columns = table.Columns;
                                columns.IsCompleted = false;
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