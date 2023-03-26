using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Ddon.Core.Use
{
    public class DdonDictionary<TValue> : Dictionary<string, TValue>
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions =
            new() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

        private readonly string _persistDataFullName;

        /// <summary>
        /// DdonDictionary
        /// </summary>
        /// <param name="fullname">持久化数据文件全名</param>
        public DdonDictionary(string fullname)
        {
            _persistDataFullName = fullname;
            Initial();
            LoadAsync().Wait();
        }

        private void Initial()
        {
            if (File.Exists(_persistDataFullName)) return;

            var baseDirectory = Path.GetDirectoryName(_persistDataFullName) ?? throw new Exception("文件路径有误");
            Directory.CreateDirectory(baseDirectory);
            File.Create(_persistDataFullName).Close();
        }

        public async Task<bool> SaveAsync()
        {
            using var ddonLock = new LocalSpinLock();
            if (!await ddonLock.GetLockAsync("save")) throw new Exception("保存失败");

            await using var stream = new StreamWriter(_persistDataFullName);

            var enumerable = this.Select(value =>
                    $"{value.Key}\t{JsonSerializer.Serialize(value.Value, _jsonSerializerOptions)}{Environment.NewLine}")
                .ToList();

            await Parallel.ForEachAsync(enumerable, async (value, _) => { await stream.WriteAsync(value); });

            return true;
        }

        private async Task LoadAsync()
        {
            using var stream = new StreamReader(_persistDataFullName);
            while (await stream.ReadLineAsync() is { } text)
            {
                var kv = text.Split("\t");
                var value = JsonSerializer.Deserialize<TValue>(kv[1]);
                Add(kv[0], value!);
            }
        }
    }
}
