using Ddon.Desktop.Core.Annotations;
using Ddon.Desktop.Core.Bridge;
using Ddon.OpcUaServer.NodeManager;
using Ddon.OpcUaServer.Server;
using Microsoft.Extensions.Logging;
using Opc.Ua;

namespace VitrinRuntime.Desktop.Services;

// ── DTOs ──────────────────────────────────────

/// <summary>OPC UA Server 状态信息（Bridge API 返回）。</summary>
public sealed class ServerStatusDto
{
    public bool IsRunning { get; set; }
    public string EndpointUrl { get; set; } = "";
    public string ServerName { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public int SessionCount { get; set; }
}

/// <summary>Server 状态推送事件（通过 IUiBridge.PublishAsync 推到前端）。</summary>
public sealed class ServerStatusChangedEvent
{
    public bool IsRunning { get; set; }
    public string EndpointUrl { get; set; } = "";
    public string ServerName { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public int SessionCount { get; set; }
}

/// <summary>节点浏览信息。</summary>
public sealed class NodeInfoDto
{
    public string NodePath { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string NodeClass { get; set; } = "";   // "Object" / "Variable" / "Method"
    public string DataType { get; set; } = "";
    public bool HasChildren { get; set; }
}

/// <summary>节点详细信息。</summary>
public sealed class NodeDetailDto
{
    public string NodePath { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string NodeClass { get; set; } = "";
    public string DataType { get; set; } = "";
    public string? Value { get; set; }
    public string? SourceType { get; set; }
    public string? PlcName { get; set; }
    public string? TagName { get; set; }
}

/// <summary>写入节点值请求。</summary>
public sealed class WriteNodeValueRequest
{
    public string NodePath { get; set; } = "";
    public object? Value { get; set; }
}

// ── Bridge Service ────────────────────────────

[BridgeService(Name = "OpcUaServer")]
public sealed class OpcUaServerService
{
    private readonly IVitrinUaServer _server;
    private readonly IUiBridge _bridge;
    private readonly ILogger<OpcUaServerService> _logger;
    private DateTime _startedAt;

    public OpcUaServerService(IVitrinUaServer server, IUiBridge bridge, ILogger<OpcUaServerService> logger)
    {
        _server = server;
        _bridge = bridge;
        _logger = logger;

        // 订阅 Server 状态变化，推送到前端
        _server.StatusChanged += OnServerStatusChanged;
    }

    private void OnServerStatusChanged(object? sender, ServerStatusChangedEventArgs args)
    {
        try
        {
            if (args.IsRunning)
                _startedAt = DateTime.UtcNow;

            _bridge.PublishAsync(new ServerStatusChangedEvent
            {
                IsRunning = args.IsRunning,
                EndpointUrl = _server.EndpointUrl,
                ServerName = "VitrinRuntime",
                StartedAt = _startedAt,
                SessionCount = _server.IsRunning ? 1 : 0,
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "推送 Server 状态变化事件失败");
        }
    }

    [BridgeMethod(Name = "GetServerStatus")]
    public ServerStatusDto GetServerStatus()
    {
        return new ServerStatusDto
        {
            IsRunning = _server.IsRunning,
            EndpointUrl = _server.EndpointUrl,
            ServerName = "VitrinRuntime",
            StartedAt = _startedAt,
            SessionCount = _server.IsRunning ? 1 : 0,
        };
    }

    [BridgeMethod(Name = "StartServer")]
    public async Task StartServer()
    {
        await _server.StartAsync();
        _startedAt = DateTime.UtcNow;
    }

    [BridgeMethod(Name = "StopServer")]
    public async Task StopServer()
    {
        await _server.StopAsync();
    }

    [BridgeMethod(Name = "RestartServer")]
    public async Task RestartServer()
    {
        await _server.StopAsync();
        await _server.StartAsync();
        _startedAt = DateTime.UtcNow;
    }

    [BridgeMethod(Name = "BrowseChildren")]
    public List<NodeInfoDto> BrowseChildren(string? nodePath)
    {
        var children = _server.NodeManager.GetChildren(nodePath);
        var result = new List<NodeInfoDto>();

        foreach (var child in children)
        {
            var info = new NodeInfoDto
            {
                NodePath = GetNodePath(child),
                DisplayName = child.DisplayName?.Text ?? child.BrowseName?.Name ?? "",
                NodeClass = GetNodeClass(child),
                DataType = GetDataType(child),
                HasChildren = HasChildNodes(child),
            };
            result.Add(info);
        }

        return result;
    }

    [BridgeMethod(Name = "GetNodeDetail")]
    public NodeDetailDto? GetNodeDetail(string nodePath)
    {
        var node = _server.NodeManager.FindNode(nodePath);
        if (node == null) return null;
        var varState = node as BaseVariableState;

        return new NodeDetailDto
        {
            NodePath = nodePath,
            DisplayName = node.DisplayName?.Text ?? node.BrowseName?.Name ?? "",
            NodeClass = GetNodeClass(node),
            DataType = GetDataType(node),
            Value = varState?.Value?.ToString(),
            SourceType = "SIMULATION",
        };
    }

    [BridgeMethod(Name = "ReadNodeValue")]
    public object? ReadNodeValue(string nodePath)
    {
        var node = _server.NodeManager.FindNode(nodePath);
        return (node as BaseVariableState)?.Value;
    }

    [BridgeMethod(Name = "WriteNodeValue")]
    public async Task WriteNodeValue(WriteNodeValueRequest req)
    {
        var node = _server.NodeManager.FindNode(req.NodePath);
        if (node is BaseVariableState varState)
        {
            varState.Value = req.Value;
            // 触发 SDK 发布变更通知
            _server.NodeManager.ApplyChanges();

            _logger.LogInformation("节点 '{Path}' 值已写入: {Value}", req.NodePath, req.Value);
        }
        else
        {
            throw new InvalidOperationException($"节点 '{req.NodePath}' 不是 Variable 类型或不存在。");
        }

        await Task.CompletedTask;
    }

    // ── 辅助方法 ──────────────────────────────

    private static string GetNodePath(NodeState node)
    {
        return node.BrowseName?.Name ?? node.NodeId?.ToString() ?? "";
    }

    private static string GetNodeClass(NodeState node)
    {
        if (node is FolderState) return "Object";
        if (node is MethodState) return "Method";
        if (node is BaseVariableState) return "Variable";
        return "Object";
    }

    private static string GetDataType(NodeState node)
    {
        if (node is BaseVariableState varState)
        {
            try
            {
                if (varState.DataType == DataTypeIds.Boolean) return "Boolean";
                if (varState.DataType == DataTypeIds.SByte) return "SByte";
                if (varState.DataType == DataTypeIds.Byte) return "Byte";
                if (varState.DataType == DataTypeIds.Int16) return "Int16";
                if (varState.DataType == DataTypeIds.UInt16) return "UInt16";
                if (varState.DataType == DataTypeIds.Int32) return "Int32";
                if (varState.DataType == DataTypeIds.UInt32) return "UInt32";
                if (varState.DataType == DataTypeIds.Int64) return "Int64";
                if (varState.DataType == DataTypeIds.UInt64) return "UInt64";
                if (varState.DataType == DataTypeIds.Float) return "Float";
                if (varState.DataType == DataTypeIds.Double) return "Double";
                if (varState.DataType == DataTypeIds.String) return "String";
                if (varState.DataType == DataTypeIds.DateTime) return "DateTime";
            }
            catch { }
            return varState.DataType?.ToString() ?? "";
        }
        return "";
    }

    private bool HasChildNodes(NodeState node)
    {
        var children = _server.NodeManager.GetChildren(GetNodePath(node));
        return children.Count > 0;
    }
}
