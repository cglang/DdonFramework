using System.Text;

namespace Ddon.OpcUaServer.Nodes;

/// <summary>
/// 节点路径构建工具，用于构建和解析 OPC UA 地址空间中的节点路径。
/// 路径格式：RootName/ChildName/GrandChildName
/// </summary>
public static class NodePathBuilder
{
    /// <summary>路径分隔符。</summary>
    public const char Separator = '/';

    /// <summary>构建节点路径。</summary>
    public static string Build(params string[] segments)
    {
        if (segments == null || segments.Length == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment)) continue;
            if (sb.Length > 0)
                sb.Append(Separator);
            sb.Append(segment.Trim());
        }
        return sb.ToString();
    }

    /// <summary>从路径中提取父路径。</summary>
    public static string GetParentPath(string nodePath)
    {
        if (string.IsNullOrWhiteSpace(nodePath))
            return string.Empty;

        var lastIndex = nodePath.LastIndexOf(Separator);
        return lastIndex > 0 ? nodePath[..lastIndex] : string.Empty;
    }

    /// <summary>从路径中提取节点名称。</summary>
    public static string GetNodeName(string nodePath)
    {
        if (string.IsNullOrWhiteSpace(nodePath))
            return string.Empty;

        var lastIndex = nodePath.LastIndexOf(Separator);
        return lastIndex >= 0 ? nodePath[(lastIndex + 1)..] : nodePath;
    }

    /// <summary>将路径分割为段数组。</summary>
    public static string[] Split(string nodePath)
    {
        if (string.IsNullOrWhiteSpace(nodePath))
            return [];

        return nodePath.Split(Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
