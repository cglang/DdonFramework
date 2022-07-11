using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Ddon.Core.Use
{
    public class DdonDictionary<TValue> : Dictionary<string, TValue>
    {
        private readonly JsonSerializerOptions jsonSerializerOptions = new() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

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
            if (!File.Exists(_persistDataFullName))
            {
                var baseDirectory = Path.GetDirectoryName(_persistDataFullName) ?? throw new Exception("文件路径有误");
                Directory.CreateDirectory(baseDirectory);
                File.Create(_persistDataFullName).Close();
            }
        }

        public async Task<bool> SaveAsync()
        {
            using var ddonLock = new LocalSpinLock();
            if (!await ddonLock.GetLock("save")) throw new Exception("保存失败");

            using var stream = new StreamWriter(_persistDataFullName);
            foreach (var value in this)
            {
                var kv = $"{value.Key}\t{JsonSerializer.Serialize(value.Value, jsonSerializerOptions)}{Environment.NewLine}";
                await stream.WriteAsync(kv);
            }
            return true;
        }

        private async Task LoadAsync()
        {
            using var stream = new StreamReader(_persistDataFullName);
            string? text;
            while ((text = await stream.ReadLineAsync()) is not null)
            {
                var kv = text.Split("\t");
                var value = JsonSerializer.Deserialize<TValue>(kv[1]);
                Add(kv[0], value!);
            }
        }
    }
}
