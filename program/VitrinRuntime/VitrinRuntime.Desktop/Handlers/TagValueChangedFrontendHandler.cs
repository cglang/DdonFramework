using Avalonia.Threading;
using Ddon.Desktop.Core.Bridge;
using Ddon.EventBus.Contracts;
using Microsoft.Extensions.Logging;

namespace VitrinRuntime.Desktop.Handlers;

/// <summary>
/// 处理 <see cref="TagValueChangedEvent"/>，将点位变化推送到前端。
/// </summary>
public sealed class TagValueChangedFrontendHandler : IEventHandler<TagValueChangedEvent>
{
    private readonly IUiBridge _bridge;
    private readonly ILogger<TagValueChangedFrontendHandler> _logger;

    public TagValueChangedFrontendHandler(IUiBridge bridge, ILogger<TagValueChangedFrontendHandler> logger)
    {
        _bridge = bridge;
        _logger = logger;
    }

    public async Task HandleAsync(TagValueChangedEvent eventData, CancellationToken cancellationToken = default)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                // 全名格式为"PLC名称.分组名称.点位名称"，前端按短名匹配
                var shortName = eventData.TagName;
                var lastDot = eventData.TagName.LastIndexOf('.');
                if (lastDot >= 0)
                    shortName = eventData.TagName.Substring(lastDot + 1);

                _bridge.PublishAsync(new TagValueChanged
                {
                    TagName = shortName,
                    Address = eventData.Address,
                    DataType = eventData.DataType,
                    OldValue = eventData.OldValue,
                    NewValue = eventData.NewValue
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "推送点位 '{Tag}' 变化事件到前端失败", eventData.TagName);
            }
        });
    }
}

/// <summary>点位值变化事件，通过 IUiBridge 推送到前端</summary>
public sealed class TagValueChanged
{
    public string TagName { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
}
