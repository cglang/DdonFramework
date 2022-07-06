using Ddon.Core.Services.LazyService.Static;
using Ddon.Core.System.Timers;
using Ddon.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Job
{
    public class Job
    {
        private readonly DdonTimer Timer;

        public Job(string name, DdonJobRule rule, int jobType, string businessId)
        {
            double interval = rule.Interval == null ? 0 : (rule.Interval.Value * 60 * 60);
            Timer = new DdonTimer(rule.DateTime, interval);

            Id = Guid.NewGuid();
            Name = name;
            Rule = rule;
            JobType = jobType;
            BusinessId = businessId;

            StartDate = DateTime.UtcNow;
            EndDate = DateTime.UtcNow;

            InitTimer();
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Job 触发的Type类型
        /// </summary>
        public int JobType { get; set; }

        /// <summary>
        /// 业务主键
        /// </summary>
        public string BusinessId { get; set; }

        /// <summary>
        /// 执行规则
        /// </summary>
        public DdonJobRule Rule { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Job是否完成
        /// </summary>
        public bool Finish { get; set; }

        public void Start() => Timer.Start();

        public void Stop() => Timer.Stop();

        private Action<Guid>? _completed;
        public void SetCompleted(Action<Guid> action) => _completed = action;

        private Action<Guid>? _ivnvoked;
        public void SetIvnvoked(Action<Guid> action) => _ivnvoked = action;

        private void InitTimer()
        {
            Timer.Elapsed = async () =>
            {
                Console.WriteLine($"{DateTime.UtcNow}-----{Rule.EndTime}");
                if (_ivnvoked != null)
                {
                    _ivnvoked(Id);
                }
                if (_completed != null && (Rule.Interval == null || Rule.Interval == 0 || (Rule.EndTime is not null && DateTime.UtcNow > Rule.EndTime)))
                {
                    _completed(Id);
                }

                using var serviceScope = LazyServiceProvider.LazyServicePrivider.ServiceProvider.CreateScope();
                var eventBus = serviceScope.ServiceProvider.GetService<IEventBus>();

                await eventBus!.PublishAsync(new JobEvent<Guid>(Id, JobType, BusinessId));
            };
        }
    }
}
