using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Средство чтения блоков.
    /// </summary>
    internal struct BlockReader
    {
        private readonly int _blockSize;
        private readonly Stream _stream;
        private readonly long _maxSize;
        private long _cntread;

        /// <summary>
        /// Средство чтения данных.
        /// </summary>
        /// <param name="blockSize">Размер блока.</param>
        /// <param name="stream">Поток.</param>
        /// <param name="maxSize">Максимальный размер.</param>
        public BlockReader(int blockSize, Stream stream, long maxSize) : this()
        {
            _blockSize = blockSize;
            _stream = stream;
            _maxSize = maxSize;
            _cntread = 0;
        }

        /// <summary>
        /// Читать блоки.
        /// </summary>
        /// <param name="blocksToRead">Количество блоков.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Количество прочитанных байт.</returns>
        public async Task<(List<byte[]> blocks, int size)> ReadBlocks(int blocksToRead, CancellationToken token)
        {
            var l = new List<byte[]>();
            int szread = 0;
            for (var i = 0; i < blocksToRead; i++)
            {
                token.ThrowIfCancellationRequested();
                var buf = new byte[_blockSize];
                var toRead = Math.Min(_blockSize, _maxSize - _cntread);
                if (toRead <= 0)
                {
                    break;
                }
                var sz = await _stream.ReadAsync(buf, 0, (int)toRead);
                if (sz <= 0)
                {
                    break;
                }
                szread += sz;
                _cntread += sz;
                if (sz < _blockSize)
                {
                    var buf2 = new byte[sz];
                    Array.Copy(buf, buf2, sz);
                    l.Add(buf2);
                    break;
                }
                l.Add(buf);
            }
            return (l, szread);
        }
    }
}