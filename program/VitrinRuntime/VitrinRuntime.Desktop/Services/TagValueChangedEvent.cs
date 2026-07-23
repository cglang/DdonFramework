using Ddon.EventBus.Contracts;

namespace VitrinRuntime.Services;

/// <summary>
/// 点位值变化事件，通过 Ddon.EventBus 在进程内发布。
/// 多个 IEventHandler 可以同时处理此事件（如推送到前端、日志记录等）。
/// </summary>
public sealed class TagValueChangedEvent : IEventData
{
    public string TagName { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
}
