using MediatR;

namespace Ddon.Job
{
    public class JobEvent<TKey> : INotification
    {
        public JobEvent(TKey jobId, int jobEventId,string businessId)
        {
            Id = jobId;
            JobEventId = jobEventId;
            BusinessId = businessId;
        }

        /// <summary>
        /// Job任务Id
        /// </summary>
        public TKey Id { get; set; }

        /// <summary>
        /// 事件对象Id
        /// </summary>
        public int JobEventId { get; set; }

        /// <summary>
        /// 业务主键
        /// </summary>
        public string BusinessId { get; set; }
    }
}
