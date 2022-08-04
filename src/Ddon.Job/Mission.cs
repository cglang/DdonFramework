using Ddon.Core.Services.LazyService.Static;
using Ddon.Core.Use;
using Ddon.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ddon.Job
{
    public class Mission
    {
        private DdonTimer? timer;

        public Mission() { }

        public Mission(MissionRule rule, string? name = default)
        {
            Id = Guid.NewGuid();
            Rule = rule;
            Name = name ?? string.Empty;
        }

        public Guid Id { get; set; }

        public string? Name { get; set; }

        public MissionState State { get; set; }

        public MissionRule? Rule { get; set; }

        public void Start()
        {
            if (timer is null) InitTimer();
            timer!.Start();
            State = MissionState.Started;
        }

        public void Stop()
        {
            if (timer is null) InitTimer();
            timer!.Stop();
            State = MissionState.Stoped;
        }

        private void InitTimer()
        {
            timer = new(Rule!.StartDate, Rule.Interval)
            {
                Elapsed = async () =>
                {
                    using var serviceScope = LazyServiceProvider.LazyServicePrivider.ServiceProvider.CreateScope();
                    var eventBus = serviceScope.ServiceProvider.GetRequiredService<IEventBus>();

                    await eventBus.PublishAsync(new JobEventData(Id));
                }
            };
        }
    }
}
