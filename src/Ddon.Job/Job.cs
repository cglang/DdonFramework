using Ddon.Core.System.Timers;

namespace Ddon.Job
{
    public class Job
    {
        private readonly DdonTimer Timer;

        public Job(string name, DdonJobRule rule, string className, string methodName, string? parameterText)
        {
            Timer = new DdonTimer(rule.DateTime, rule.Interval == null ? 10 : rule.DateTime.Millisecond);

            Id = Guid.NewGuid();
            Name = name;
            ClassName = className;
            MethodName = methodName;
            ParameterText = parameterText;
            Rule = rule;
            CreateDate = DateTime.Now;

            InitTimer();
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ClassName { get; set; }

        public string MethodName { get; set; }

        public string? ParameterText { get; set; }

        public DdonJobRule Rule { get; set; }

        public DateTime CreateDate { get; set; }

        public bool Finish { get; set; }

        public void Start() => Timer.Start();

        public void Stop() => Timer.Stop();

        private Action<Guid>? _completed;
        public void SetCompleted(Action<Guid> action) => _completed = action;

        private Action<Guid>? _ivnvoked;
        public void SetIvnvoked(Action<Guid> action) => _ivnvoked = action;

        public void Modify(string name, DdonJobRule rule, string className, string methodName, string? parameterText)
        {
            Name = name;
            ClassName = className;
            MethodName = methodName;
            ParameterText = parameterText;
            Rule = rule;
        }

        private void InitTimer()
        {
            Timer.Elapsed = async () =>
            {
                await DdonJobInvoke.IvnvokeAsync(ClassName, MethodName, ParameterText);

                if (_ivnvoked != null)
                    _ivnvoked(Id);

                if (_completed != null && Rule.Interval == null)
                    _completed(Id);
            };
        }
    }
}
