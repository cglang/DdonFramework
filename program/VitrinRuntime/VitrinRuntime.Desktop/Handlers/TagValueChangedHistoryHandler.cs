using Ddon.EventBus.Contracts;

namespace VitrinRuntime.Desktop.Handlers
{
    /// <summary>
    /// 处理 <see cref="TagValueChangedEvent"/>，用于记录点位变化历史。
    /// </summary>
    public sealed class TagValueChangedHistoryHandler : IEventHandler<TagValueChangedEvent>
    {
        public Task HandleAsync(TagValueChangedEvent eventData, CancellationToken cancellationToken = default)
        {
            // TODO: 创建点位历史记录Store接口，暂时使用json文件进行实现记录点位变化历史。
            // json实现中每个点位记录100个历史数据，超过100个则删除最早的记录。
            // 记录点位名称、地址、数据类型、旧值、新值、时间戳等信息。
            // 在点位列表的操作列中，加一个“历史”按钮，点击后弹出一个窗口，显示该点位的历史数据列表。

            return Task.CompletedTask;
        }
    }
}
