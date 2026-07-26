using Ddon.VitrinPLC.Models;

namespace VitrinRuntime.Desktop.Services;

public sealed class UserFriendlyException : Exception
{
    public UserFriendlyException(string message) : base(message) { }
}

/// <summary>PLC 连接配置</summary>
public sealed class PlcConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Ip { get; set; } = "192.168.0.0";
    public int Port { get; set; } = 102;
    public int Rack { get; set; } = 0;
    public int Slot { get; set; } = 1;
    public int CpuType { get; set; } = 40;
    public int ScanInterval { get; set; } = 200;
    public bool AutoConnect { get; set; }
    public bool IsConnected { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastConnectedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>DB 块分组</summary>
public sealed class DbGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PlcName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>点位配置</summary>
public sealed class TagConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string GroupId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public PlcDataType DataType { get; set; } = PlcDataType.Int32;
    public int StringLength { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ── Bridge 方法 Request DTO ──────────────────────────

public sealed class AddPlcRequest
{
    public string Name { get; set; } = string.Empty;
    public string Ip { get; set; } = "192.168.1.10";
    public int Port { get; set; } = 102;
    public int Rack { get; set; } = 0;
    public int Slot { get; set; } = 1;
    public int CpuType { get; set; } = 40;
    public int ScanInterval { get; set; } = 200;
    public bool AutoConnect { get; set; }
}

public sealed class PlcNameRequest
{
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdatePlcRequest
{
    public string OldName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Ip { get; set; } = "192.168.1.10";
    public int Port { get; set; } = 102;
    public int Rack { get; set; } = 0;
    public int Slot { get; set; } = 1;
    public int CpuType { get; set; } = 40;
    public int ScanInterval { get; set; } = 200;
    public bool AutoConnect { get; set; }
}

public sealed class ListDbGroupsRequest
{
    public string PlcName { get; set; } = string.Empty;
}

public sealed class CreateDbGroupRequest
{
    public string PlcName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
}

public sealed class GroupIdRequest
{
    public string GroupId { get; set; } = string.Empty;
}

public sealed class RenameDbGroupRequest
{
    public string GroupId { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;
}

public sealed class ListTagsRequest
{
    public string GroupId { get; set; } = string.Empty;
}

public sealed class AddTagRequest
{
    public string GroupId { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = "Int32";
    public int StringLength { get; set; }
}

public sealed class UpdateTagRequest
{
    public string TagId { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = "Int32";
    public int StringLength { get; set; }
}

public sealed class TagIdRequest
{
    public string TagId { get; set; } = string.Empty;
}

public sealed class WriteTagRequest
{
    public string TagId { get; set; } = string.Empty;
    public object? Value { get; set; }
}

/// <summary>点位值变化历史记录</summary>
public sealed class TagHistoryRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TagName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public sealed class TagHistoryRequest
{
    public string GroupId { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
}
