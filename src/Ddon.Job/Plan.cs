using Ddon.Core;
using Ddon.Core.System.Timers;
using Ddon.KeyValueStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Job
{
    public class Plan
    {
        private readonly DdonTimer Timer;

        public Plan(Guid id, string name, string className, string methodName, string parameterText, DdonTimerRule rule)
        {
            Timer = new DdonTimer().InitAction(async () =>
            {
                await DdonJobInvoke.IvnvokeAsync(className, methodName, parameterText);
            }).FinishAction(async () =>
            {
                Finish = true;
                var man = DdonServiceProvider.Services.GetService<IDdonKeyValueManager<Plan, DdonJobOptions>>();
                await man!.SaveAsync();
            }).SetRule(rule);

            Id = id;
            Name = name;
            ClassName = className;
            MethodName = methodName;
            ParameterText = parameterText;
            CreateDate = DateTime.UtcNow;
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ClassName { get; set; }

        public string MethodName { get; set; }

        public string ParameterText { get; set; }

        public DateTime CreateDate { get; set; }

        public bool Finish { get; set; }

        public void Start() => Timer.Start();

        public void Stop() => Timer.Stop();
    }
}
