using Ddon.VitrinPLC.Models;

namespace VitrinRuntime.Services;

/// <summary>PLC 连接配置</summary>
public sealed class PlcConfig
{
    public string Name { get; set; } = string.Empty;
    public string Ip { get; set; } = "192.168.1.10";
    public int Port { get; set; } = 102;
    public int Rack { get; set; } = 0;
    public int Slot { get; set; } = 1;
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
    public int DbNumber { get; set; }
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
}

public sealed class PlcNameRequest
{
    public string Name { get; set; } = string.Empty;
}

public sealed class ListDbGroupsRequest
{
    public string PlcName { get; set; } = string.Empty;
}

public sealed class CreateDbGroupRequest
{
    public string PlcName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int DbNumber { get; set; }
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

public sealed class TagIdRequest
{
    public string TagId { get; set; } = string.Empty;
}

public sealed class WriteTagRequest
{
    public string TagId { get; set; } = string.Empty;
    public object? Value { get; set; }
}
