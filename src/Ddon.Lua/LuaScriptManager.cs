using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using Microsoft.Extensions.Options;
using NLua;

namespace Ddon.LuaEngine
{
    internal class LuaScriptManager : ILuaScriptManager, IDisposable
    {
        private readonly ConcurrentDictionary<string, LuaScriptGroup> _groups = new ConcurrentDictionary<string, LuaScriptGroup>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new ConcurrentDictionary<string, FileSystemWatcher>(StringComparer.OrdinalIgnoreCase);
        private readonly ILuaVmManager _vmManager;
        private readonly LuaOptions _options;
        private readonly Timer _debounceTimer;
        private readonly ConcurrentDictionary<string, bool> _pendingChanges = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private bool _disposed;

        private readonly struct FileEntry
        {
            public readonly string FilePath;
            public readonly string RelativePath;

            public FileEntry(string filePath, string relativePath)
            {
                FilePath = filePath;
                RelativePath = relativePath;
            }
        }

        private static IEnumerable<FileEntry> EnumerateLuaFilesRecursive(string directoryPath)
        {
            var dirs = new Stack<string>();
            dirs.Push(directoryPath);

            while (dirs.Count > 0)
            {
                var currentDir = dirs.Pop();

                string[] files = null;
                try
                {
                    files = Directory.GetFiles(currentDir, "*.lua", SearchOption.TopDirectoryOnly);
                }
                catch (UnauthorizedAccessException) { }
                catch (SecurityException) { }

                if (files != null)
                {
                    foreach (var file in files)
                    {
                        var relativePath = file.Substring(directoryPath.Length).TrimStart(Path.DirectorySeparatorChar);
                        yield return new FileEntry(file, relativePath);
                    }
                }

                string[] subDirs = null;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException) { }
                catch (SecurityException) { }

                if (subDirs != null)
                {
                    foreach (var subDir in subDirs)
                    {
                        dirs.Push(subDir);
                    }
                }
            }
        }

        public bool IsFileWatcherEnabled { get; private set; }

        public LuaScriptManager(ILuaVmManager vmManager, IOptions<LuaOptions> options)
        {
            _vmManager = vmManager;
            _options = options.Value;
            IsFileWatcherEnabled = _options.EnableFileWatcher;

            if (_options.FileWatcherDebounceMilliseconds > 0)
            {
                _debounceTimer = new Timer(OnDebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void LoadScriptsFromDirectory(string directoryPath, string groupName = null)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));

            directoryPath = Path.GetFullPath(directoryPath);

            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            if (groupName == null) groupName = Path.GetFileName(directoryPath);

            if (_groups.TryGetValue(groupName, out _))
            {
                ReloadGroup(groupName);
                return;
            }

            Lua vm;
            if (_vmManager.ContainsVm(groupName))
            {
                vm = _vmManager.GetVm(groupName);
            }
            else
            {
                vm = _vmManager.AddVm(groupName);
            }

            var group = new LuaScriptGroup(groupName, directoryPath, vm);

            foreach (var entry in EnumerateLuaFilesRecursive(directoryPath))
            {
                var scriptFile = new LuaScriptFile
                {
                    FilePath = entry.FilePath,
                    FileName = entry.RelativePath,
                    LastWriteTime = File.GetLastWriteTime(entry.FilePath),
                    IsLoaded = false
                };

                try
                {
                    group.LuaVm.DoFile(entry.FilePath);
                    scriptFile.IsLoaded = true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to load Lua script '{entry.RelativePath}': {ex.Message}", ex);
                }

                group.Scripts[entry.RelativePath] = scriptFile;
            }

            if (!_groups.TryAdd(groupName, group))
            {
                group.Dispose();
                throw new InvalidOperationException($"Script group '{groupName}' already exists.");
            }

            if (IsFileWatcherEnabled)
            {
                StartFileWatcher(groupName, directoryPath);
            }
        }

        public void ReloadGroup(string groupName)
        {
            if (!_groups.TryGetValue(groupName, out var group))
                throw new InvalidOperationException($"Script group '{groupName}' not found.");

            var luaFiles = EnumerateLuaFilesRecursive(group.DirectoryPath).ToList();
            var currentKeys = new HashSet<string>(luaFiles.Select(f => f.RelativePath), StringComparer.OrdinalIgnoreCase);

            foreach (var entry in luaFiles)
            {
                try
                {
                    group.LuaVm.DoFile(entry.FilePath);
                    group.Scripts[entry.RelativePath] = new LuaScriptFile
                    {
                        FilePath = entry.FilePath,
                        FileName = entry.RelativePath,
                        LastWriteTime = File.GetLastWriteTime(entry.FilePath),
                        IsLoaded = true
                    };
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to reload Lua script '{entry.RelativePath}': {ex.Message}", ex);
                }
            }

            var removedFiles = group.Scripts.Keys
                .Where(key => !currentKeys.Contains(key))
                .ToList();

            foreach (var fileName in removedFiles)
            {
                group.Scripts.Remove(fileName);
            }
        }

        public void ReloadScript(string groupName, string fileName)
        {
            if (!_groups.TryGetValue(groupName, out var group))
                throw new InvalidOperationException($"Script group '{groupName}' not found.");

            if (!group.Scripts.TryGetValue(fileName, out var scriptFile))
                throw new InvalidOperationException($"Script '{fileName}' not found in group '{groupName}'.");

            var filePath = scriptFile.FilePath;

            if (!File.Exists(filePath))
            {
                group.Scripts.Remove(fileName);
                throw new FileNotFoundException($"Script file not found: {filePath}");
            }

            try
            {
                group.LuaVm.DoFile(filePath);
                scriptFile.LastWriteTime = File.GetLastWriteTime(filePath);
                scriptFile.IsLoaded = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to reload Lua script '{filePath}': {ex.Message}", ex);
            }
        }

        public void UnloadGroup(string groupName)
        {
            if (!_groups.TryRemove(groupName, out var group))
                throw new InvalidOperationException($"Script group '{groupName}' not found.");

            StopFileWatcher(groupName);
            group.Dispose();
        }

        public void UnloadScript(string groupName, string fileName)
        {
            if (!_groups.TryGetValue(groupName, out var group))
                throw new InvalidOperationException($"Script group '{groupName}' not found.");

            if (!group.Scripts.Remove(fileName))
                throw new InvalidOperationException($"Script '{fileName}' not found in group '{groupName}'.");
        }

        public IReadOnlyDictionary<string, LuaScriptGroup> GetAllGroups()
        {
            return new Dictionary<string, LuaScriptGroup>(_groups);
        }

        public LuaScriptGroup GetGroup(string groupName)
        {
            _groups.TryGetValue(groupName, out var group);
            return group;
        }

        public bool ContainsGroup(string groupName)
        {
            return _groups.ContainsKey(groupName);
        }

        public void EnableFileWatcher()
        {
            if (IsFileWatcherEnabled) return;
            IsFileWatcherEnabled = true;

            foreach (var groupName in _groups.Keys)
            {
                var group = _groups[groupName];
                StartFileWatcher(groupName, group.DirectoryPath);
            }
        }

        public void DisableFileWatcher()
        {
            if (!IsFileWatcherEnabled) return;
            IsFileWatcherEnabled = false;

            foreach (var watcher in _watchers.Values)
            {
                watcher.EnableRaisingEvents = false;
            }
        }

        private void StartFileWatcher(string groupName, string directoryPath)
        {
            if (_watchers.ContainsKey(groupName))
                return;

            var watcher = new FileSystemWatcher(directoryPath, "*.lua")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            watcher.Changed += (_, args) => OnFileChanged(groupName, args.FullPath);
            watcher.Created += (_, args) => OnFileChanged(groupName, args.FullPath);
            watcher.Deleted += (_, args) => OnFileChanged(groupName, args.FullPath);
            watcher.Renamed += (_, args) => OnFileChanged(groupName, args.FullPath);

            _watchers[groupName] = watcher;
        }

        private void StopFileWatcher(string groupName)
        {
            if (_watchers.TryRemove(groupName, out var watcher))
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
        }

        private void OnFileChanged(string groupName, string filePath)
        {
            _pendingChanges[groupName] = true;
            _debounceTimer?.Change(_options.FileWatcherDebounceMilliseconds, Timeout.Infinite);
        }

        private void OnDebounceElapsed(object state)
        {
            var changedGroups = _pendingChanges.Keys.ToList();
            _pendingChanges.Clear();

            foreach (var groupName in changedGroups)
            {
                try
                {
                    ReloadGroup(groupName);
                }
                catch
                {
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _debounceTimer?.Dispose();

            foreach (var watcher in _watchers.Values)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            _watchers.Clear();

            foreach (var group in _groups.Values)
            {
                group.Dispose();
            }

            _groups.Clear();
        }
    }
}
