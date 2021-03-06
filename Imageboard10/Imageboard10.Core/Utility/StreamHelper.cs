﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Imageboard10.Core.Utility
{
    /// <summary>
    /// Помощник для работы с потоками.
    /// </summary>
    public static class StreamHelper
    {
        /// <summary>
        /// Копировать из RT потока в .NET поток с прогрессом.
        /// </summary>
        /// <param name="src">Исхоный поток.</param>
        /// <param name="outStream">Поток результата.</param>
        /// <param name="progress">Прогресс.</param>
        /// <param name="token">Токен отмены.</param>
        /// <param name="bufferSize">Размер буфера.</param>
        /// <returns>Таск.</returns>
        public static async Task CopyToNetStreamWithProgress(this IInputStream src, Stream outStream, IProgress<ulong> progress, CancellationToken token, uint bufferSize = 16384)
        {
            ulong totalRead = 0;
            using (var rd = new DataReader(src))
            {
                progress.Report(0);
                do
                {
                    token.ThrowIfCancellationRequested();
                    var r = await rd.LoadAsync(bufferSize);
                    totalRead += r;
                    progress.Report(totalRead);
                    if (r <= 0) break;
                    var buf = new byte[r];
                    rd.ReadBytes(buf);
                    await outStream.WriteAsync(buf, 0, (int)r, token);
                } while (true);
                rd.DetachStream();
            }
        }

        /// <summary>
        /// Копировать из одного RT потока в другой RT поток.
        /// </summary>
        /// <param name="src">Исходный поток.</param>
        /// <param name="outStream">Поток результата.</param>
        /// <param name="bufferSize">Размер буфера.</param>
        /// <returns>Таск.</returns>
        public static async Task CopyStreamAsync(this IInputStream src, IOutputStream outStream, uint bufferSize = 16384)
        {
            using (var rd = new DataReader(src))
            {
                using (var wr = new DataWriter(outStream))
                {
                    do
                    {
                        var r = await rd.LoadAsync(bufferSize);
                        if (r <= 0) break;
                        var buf = new byte[r];
                        rd.ReadBytes(buf);
                        wr.WriteBytes(buf);
                        await wr.FlushAsync();
                    } while (true);
                }
            }
        }
    }
}