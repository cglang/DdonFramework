using System;
using System.Collections.Generic;
using System.IO;

namespace Ddon.IniParser
{
    public class IniParser
    {
        private readonly Dictionary<string, Dictionary<string, string>> _ini = new();

        private readonly string _iniPath;

        public IniParser(string iniPath)
        {
            _iniPath = iniPath;
            List<string> iniList = new();
            using StreamReader sR = File.OpenText(iniPath);
            string? nextLine;
            while ((nextLine = sR.ReadLine()) != null)
            {
                nextLine = nextLine.Replace(" ", "");
                var index = nextLine.IndexOf(";");

                if (index > 0)
                    nextLine = nextLine[..index];

                if (!string.IsNullOrWhiteSpace(nextLine))
                {
                    iniList.Add(nextLine);
                }

            }

            string sectionName = "default";
            foreach (var ini in iniList)
            {
                if (ini[0] == '[')
                {
                    sectionName = ini.Replace("[", "").Replace("]", "");
                }
                else
                {
                    var kv = ini.Split("=");
                    if (kv.Length >= 2)
                    {
                        if (!_ini.ContainsKey(sectionName))
                            _ini.Add(sectionName, new Dictionary<string, string>());
                        _ini[sectionName][kv[0]] = kv[1];
                    }
                }
            }

            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(_ini));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <typeparam name="T">获取值的类型</typeparam>
        /// <param name="section">节名称</param>
        /// <param name="key">键名称</param>
        /// <returns></returns>
        public T? GetValue<T>(string section, string key)
        {
            if (_ini.ContainsKey(section) && _ini[section].ContainsKey(key))
            {
                var value = _ini[section][key];
                T ret = (T)Convert.ChangeType(value, typeof(T));
                return ret;
            }
            return default;
        }

        /// <summary>
        /// 获取指定节下的键值
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Dictionary<string, string>? GetSection(string section)
        {
            if (_ini.ContainsKey(section))
            {
                return _ini[section];
            }
            return default;
        }

        /// <summary>
        /// 添加节
        /// </summary>
        /// <param name="section"></param>
        public void AddSection(string section)
        {
            if (!_ini.ContainsKey(section))
            {
                _ini.Add(section, new());
            }
        }

        /// <summary>
        /// 移除指定节
        /// </summary>
        /// <param name="section"></param>
        public void RemoveSection(string section)
        {
            if (_ini.ContainsKey(section))
            {
                _ini.Remove(section);
            }
        }

        /// <summary>
        /// 向指定节下添加/修改键值,节不存在会自动创建节
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ModifyValue(string section, string key, string value)
        {
            if (!_ini.ContainsKey(section))
            {
                _ini.Add(section, new());
                _ini[section].Add(key, value);
            }
            else
            {
                if (!_ini[section].ContainsKey(key))
                    _ini[section].Add(key, value);
                else
                    _ini[section][key] = value;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            using StreamWriter streamReader = new(_iniPath);

            foreach (var section in _ini)
            {
                streamReader.WriteLine($"[{section.Key}]");
                foreach (var kv in section.Value)
                {
                    streamReader.WriteLine($"{kv.Key}={kv.Value}");
                }

                streamReader.WriteLine();
            }
        }
    }
}
