using Avalonia.Threading;
using Ddon.Desktop.Core.Bridge;
using Ddon.EventBus.Contracts;
using Microsoft.Extensions.Logging;

namespace VitrinRuntime.Services;

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
                _bridge.PublishAsync(new TagValueChanged
                {
                    TagName = eventData.TagName,
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
