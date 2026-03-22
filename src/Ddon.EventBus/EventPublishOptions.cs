namespace Ddon.EventBus
{
    public class EventPublishOptions
    {
        public Mode Mode { get; private set; }


        public static EventPublishOptions Default { get; } = new EventPublishOptions()
        {
            Mode = Mode.Default
        };

        public static EventPublishOptions Background { get; } = new EventPublishOptions()
        {
            Mode = Mode.RunInBackground
        };

        public static EventPublishOptions RunInThread { get; } = new EventPublishOptions()
        {
            Mode = Mode.RunInThread
        };
    }

    public enum Mode
    {
        Default = 0,
        /// <summary>
        /// 后台模式，采用队列的形式实现
        /// </summary>
        RunInBackground = 1,
        /// <summary>
        /// 线程模式，采用线程的形式实现
        /// </summary>
        RunInThread = 2
    }
}
