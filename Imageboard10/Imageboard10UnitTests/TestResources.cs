using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Imageboard10.Makaba.Network.Json;
using Newtonsoft.Json;

namespace Imageboard10UnitTests
{
    /// <summary>
    /// Тестовые ресурсы.
    /// </summary>
    public static class TestResources
    {
        /// <summary>
        /// Прочитать файл.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <returns>Результат.</returns>
        public static async Task<string> ReadTestTextFile(string fileName)
        {
            var uri = new Uri($"ms-appx:///Resources/{fileName}");
            StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var text = await FileIO.ReadTextAsync(f);
            return text;
        }

        /// <summary>
        /// Прочитать ссылки на доски из файла ресурсов.
        /// </summary>
        /// <returns>Ссылки на доски.</returns>
        public static async Task<MobileBoardInfoCollection> LoadBoardReferencesFromResource()
        {
            var str = await ReadTestTextFile("boards.json");
            var obj = JsonConvert.DeserializeObject<Dictionary<string, MobileBoardInfo[]>>(str);
            return new MobileBoardInfoCollection()
            {
                Boards = obj
            };
        }

        /// <summary>
        /// Прочитать файл.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <returns>Результат.</returns>
        public static async Task<Stream> ReadTestFile(string fileName)
        {
            var uri = new Uri($"ms-appx:///Resources/{fileName}");
            StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(uri);
            return await f.OpenStreamForReadAsync();
        }
    }
}