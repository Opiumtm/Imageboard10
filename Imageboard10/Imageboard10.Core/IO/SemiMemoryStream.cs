using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Imageboard10.Core.Tasks;

namespace Imageboard10.Core.IO
{
    /// <summary>
    /// Условно содержащийся в памяти поток. При превышении максимального размера будет храниться во временном файле.
    /// </summary>
    public sealed class SemiMemoryStream : Stream
    {
        private readonly long _maxSize;

        private Stream _currentStream;

        private StorageFile _tempFile;

        private StorageFolder _moveToFolder;

        private string _moveToFileName;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="maxSize">Максимальный размер.</param>
        public SemiMemoryStream(long maxSize)
        {
            _currentStream = new MemoryStream();
            this._maxSize = maxSize;
        }

        /// <summary>When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.</summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        public override void Flush()
        {
            _currentStream.Flush();
        }

        /// <summary>Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.</summary>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _currentStream.FlushAsync(cancellationToken);
        }

        /// <summary>When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.</summary>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream. </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer" /> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> or <paramref name="count" /> is negative. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <filterpriority>1</filterpriority>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _currentStream.Read(buffer, offset, count);
        }

        /// <summary>Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.</summary>
        /// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached. </returns>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous read operation. </exception>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _currentStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <summary>Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.</summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <filterpriority>2</filterpriority>
        public override int ReadByte()
        {
            return _currentStream.ReadByte();
        }

        /// <summary>When overridden in a derived class, sets the position within the current stream.</summary>
        /// <returns>The new position within the current stream.</returns>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter. </param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <filterpriority>1</filterpriority>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _currentStream.Seek(offset, origin);
        }

        /// <summary>When overridden in a derived class, sets the length of the current stream.</summary>
        /// <param name="value">The desired length of the current stream in bytes. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <filterpriority>2</filterpriority>
        public override void SetLength(long value)
        {
            _currentStream.SetLength(value);
            if (!CoreTaskHelper.RunAsyncFuncOnNewThread(CheckStreamSize).Wait(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException();
            }
        }

        /// <summary>When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.</summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream. </param>
        /// <param name="count">The number of bytes to be written to the current stream. </param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is greater than the buffer length.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer" />  is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occured, such as the specified file cannot be found.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// <see cref="M:System.IO.Stream.Write(System.Byte[],System.Int32,System.Int32)" /> was called after the stream was closed.</exception>
        /// <filterpriority>1</filterpriority>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _currentStream.Write(buffer, offset, count);
            if (!CoreTaskHelper.RunAsyncFuncOnNewThread(CheckStreamSize).Wait(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException();
            }
        }

        /// <summary>Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.</summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous write operation. </exception>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _currentStream.WriteAsync(buffer, offset, count, cancellationToken);
            await CheckStreamSize();
        }

        /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports reading.</summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanRead
        {
            get { return _currentStream.CanRead; }
        }

        /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports seeking.</summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanSeek
        {
            get { return _currentStream.CanSeek; }
        }

        /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports writing.</summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanWrite
        {
            get { return _currentStream.CanWrite; }
        }

        /// <summary>When overridden in a derived class, gets the length in bytes of the stream.</summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <filterpriority>1</filterpriority>
        public override long Length
        {
            get { return _currentStream.Length; }
        }

        private async Task<Nothing> CheckStreamSize()
        {
            if (_tempFile != null)
            {
                return Nothing.Value;
            }
            if (_currentStream.Length > _maxSize)
            {
                await CreateTempFile();
            }
            return Nothing.Value;
        }

        private async Task CreateTempFile()
        {
            _tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(Guid.NewGuid() + ".tmp", CreationCollisionOption.GenerateUniqueName);
            var newStream = File.Open(_tempFile.Path, FileMode.Open);
            var position = _currentStream.Position;
            _currentStream.Position = 0;
            await _currentStream.CopyToAsync(newStream);
            newStream.Position = position;
            _currentStream.Dispose();
            _currentStream = newStream;
        }

        /// <summary>When overridden in a derived class, gets or sets the position within the current stream.</summary>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <filterpriority>1</filterpriority>
        public override long Position
        {
            get { return _currentStream.Position; }
            set { _currentStream.Position = value; }
        }

        /// <summary>
        /// Переместить после закрытия вместо удаления.
        /// </summary>
        /// <param name="folder">Папка.</param>
        /// <param name="fileName">Имя файла.</param>
        public void MoveAfterClose(StorageFolder folder, string fileName)
        {
            _moveToFolder = folder;
            _moveToFileName = fileName;
        }

        private int _isDisposed;

        private async Task<Nothing> CloseFile(string tmpName)
        {
            return await CoreTaskHelper.RunAsyncFuncOnNewThread(async () =>
            {
                var tempFile2 = await ApplicationData.Current.TemporaryFolder.GetFileAsync(tmpName);
                if (_moveToFolder != null && !string.IsNullOrWhiteSpace(_moveToFileName))
                {
                    try
                    {
                        await tempFile2.MoveAsync(_moveToFolder, _moveToFileName, NameCollisionOption.ReplaceExisting);
                    }
                    catch
                    {
                        await tempFile2.DeleteAsync();
                        throw;
                    }
                }
                else
                {
                    await tempFile2.DeleteAsync();
                }
                return Nothing.Value;
            });
        }

        /// <summary>
        /// Завершить работу асинхронно.
        /// </summary>
        public async Task DisposeAsync()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                _currentStream.Dispose();
                if (_tempFile != null)
                {
                    var tmpName = _tempFile.Name;
                    _tempFile = null;
                    await CloseFile(tmpName);
                }
            }
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
                {
                    _currentStream.Dispose();
                    if (_tempFile != null)
                    {
                        var tmpName = _tempFile.Name;
                        _tempFile = null;
                        var tcs = new TaskCompletionSource<Nothing>();
                        CoreTaskHelper.RunAsyncFuncOnNewThread(async () =>
                        {
                            try
                            {
                                await CloseFile(tmpName);
                                tcs.SetResult(Nothing.Value);
                            }
                            catch (Exception ex)
                            {
                                tcs.SetException(ex);
                            }
                            return Nothing.Value;
                        });
                        if (!tcs.Task.Wait(TimeSpan.FromSeconds(5)))
                        {
                            throw new TimeoutException();
                        }
                    }
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Поток находится в памяти.
        /// </summary>
        public bool IsInMemory => _tempFile == null;

        /// <summary>
        /// Путь к временному файлу.
        /// </summary>
        public string TempFilePath => _tempFile?.Path;
    }
}