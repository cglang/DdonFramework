using MediatR;

namespace Ddon.Job.Event
{
    internal class JobInvokeEventData : INotification
    {
        public JobInvokeEventData(string jobClassName, string jobMethodName)
        {
            JobClassName = jobClassName;
            JobMethodName = jobMethodName;
        }

        public string JobClassName { get; set; }

        public string JobMethodName { get; set; }
    }
}
