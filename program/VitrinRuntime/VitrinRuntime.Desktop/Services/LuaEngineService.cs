using Ddon.Desktop.Core.Annotations;
using Ddon.LuaEngine;
using Microsoft.Extensions.Logging;
using VitrinRuntime.Desktop.Stores;

namespace VitrinRuntime.Desktop.Services;

[BridgeService(Name = "LuaEngine")]
public sealed class LuaEngineService
{
    private readonly ILuaScriptManager _scriptManager;
    private readonly ILuaVmManager _vmManager;
    private readonly ILuaGroupStore _store;
    private readonly ILogger<LuaEngineService> _logger;
    private readonly LuaEventBridgeService _eventBridge;

    public LuaEngineService(ILuaScriptManager scriptManager, ILuaVmManager vmManager,
        ILuaGroupStore store, ILogger<LuaEngineService> logger,
        LuaEventBridgeService eventBridge)
    {
        _scriptManager = scriptManager;
        _vmManager = vmManager;
        _store = store;
        _logger = logger;
        _eventBridge = eventBridge;
    }

    [BridgeMethod(Name = "ListGroups")]
    public List<object> ListGroups()
    {
        var groups = _scriptManager.GetAllGroups();
        return groups.Values.Select(g => new
        {
            name = g.GroupName,
            path = g.DirectoryPath,
            scriptCount = g.Scripts.Count,
            watcherEnabled = _scriptManager.IsFileWatcherEnabled,
            vmLoaded = _vmManager.ContainsVm(g.GroupName),
            hasScripts = g.Scripts.Count > 0
        } as object).ToList();
    }

    [BridgeMethod(Name = "LoadGroup")]
    public object LoadGroup(LoadGroupRequest req)
    {
        var dirPath = Path.GetFullPath(req.DirectoryPath);
        if (!Directory.Exists(dirPath))
            throw new InvalidOperationException($"目录不存在: {dirPath}");

        var groupName = req.GroupName ?? Path.GetFileName(dirPath);

        _scriptManager.LoadScriptsFromDirectory(dirPath, groupName);

        _eventBridge.SubscribeGroup(groupName);

        if (!_store.Contains(groupName))
        {
            _store.Add(new LuaGroupConfig
            {
                GroupName = groupName,
                DirectoryPath = dirPath
            });
        }

        var group = _scriptManager.GetGroup(groupName);
        return new
        {
            name = group?.GroupName ?? groupName,
            path = dirPath,
            scriptCount = group?.Scripts.Count ?? 0
        };
    }

    [BridgeMethod(Name = "ReloadGroup")]
    public object ReloadGroup(GroupNameRequest req)
    {
        _scriptManager.ReloadGroup(req.GroupName);
        _eventBridge.ResubscribeGroup(req.GroupName);
        return new { success = true };
    }

    [BridgeMethod(Name = "UnloadGroup")]
    public object UnloadGroup(GroupNameRequest req)
    {
        _eventBridge.UnsubscribeGroup(req.GroupName);
        _scriptManager.UnloadGroup(req.GroupName);
        _store.Remove(req.GroupName);
        return new { success = true };
    }

    [BridgeMethod(Name = "GetGroupDetail")]
    public object GetGroupDetail(GroupNameRequest req)
    {
        var group = _scriptManager.GetGroup(req.GroupName);
        if (group is null) return null;

        return new
        {
            name = group.GroupName,
            path = group.DirectoryPath,
            scripts = group.Scripts.Values.Select(s => new
            {
                fileName = s.FileName,
                filePath = s.FilePath,
                isLoaded = s.IsLoaded,
                lastWriteTime = s.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList()
        };
    }

    [BridgeMethod(Name = "ReloadScript")]
    public object ReloadScript(ScriptRequest req)
    {
        _scriptManager.ReloadScript(req.GroupName, req.FileName);
        return new { success = true };
    }

    [BridgeMethod(Name = "UnloadScript")]
    public object UnloadScript(ScriptRequest req)
    {
        _scriptManager.UnloadScript(req.GroupName, req.FileName);
        return new { success = true };
    }

    [BridgeMethod(Name = "GetWatcherStatus")]
    public object GetWatcherStatus()
    {
        return new { enabled = _scriptManager.IsFileWatcherEnabled };
    }

    [BridgeMethod(Name = "SetWatcher")]
    public object SetWatcher(SetWatcherRequest req)
    {
        if (req.Enabled)
            _scriptManager.EnableFileWatcher();
        else
            _scriptManager.DisableFileWatcher();

        return new { enabled = _scriptManager.IsFileWatcherEnabled };
    }

    [BridgeMethod(Name = "ListVms")]
    public List<object> ListVms()
    {
        var vms = _vmManager.GetAllVms();
        return vms.Select(kvp => new
        {
            groupName = kvp.Key,
            hasVm = kvp.Value is not null
        } as object).ToList();
    }

    [BridgeMethod(Name = "ExecuteLua")]
    public async Task<object> ExecuteLua(ExecuteLuaRequest req)
    {
        await Task.CompletedTask;

        try
        {
            var vm = _vmManager.GetVm(req.GroupName);
            if (vm is null)
                throw new InvalidOperationException($"组 '{req.GroupName}' 没有对应的 Lua VM。");

            var result = vm.DoString(req.Code);
            return new { success = true, result = result?.ToString() ?? "" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行 Lua 代码失败");
            return new { success = false, error = ex.Message };
        }
    }
}

// ── Request DTOs ────────────────────────────────

public sealed class LoadGroupRequest
{
    public string DirectoryPath { get; set; } = string.Empty;
    public string? GroupName { get; set; }
}

public sealed class GroupNameRequest
{
    public string GroupName { get; set; } = string.Empty;
}

public sealed class ScriptRequest
{
    public string GroupName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public sealed class SetWatcherRequest
{
    public bool Enabled { get; set; }
}

public sealed class ExecuteLuaRequest
{
    public string GroupName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
