using Ddon.EventBus.Contracts;
using VitrinRuntime.Desktop.Services;
using VitrinRuntime.Desktop.Stores;

namespace VitrinRuntime.Desktop.Handlers
{
    /// <summary>
    /// 处理 <see cref="TagValueChangedEvent"/>，用于记录点位变化历史。
    /// </summary>
    public sealed class TagValueChangedHistoryHandler : IEventHandler<TagValueChangedEvent>
    {
        private readonly ITagHistoryStore _store;

        public TagValueChangedHistoryHandler(ITagHistoryStore store)
        {
            _store = store;
        }

        public Task HandleAsync(TagValueChangedEvent eventData, CancellationToken cancellationToken = default)
        {
            var record = new TagHistoryRecord
            {
                TagName = eventData.TagName,
                Address = eventData.Address,
                DataType = eventData.DataType,
                OldValue = eventData.OldValue,
                NewValue = eventData.NewValue,
                Timestamp = DateTime.UtcNow
            };

            _store.AddRecord(record);

            return Task.CompletedTask;
        }
    }
}
