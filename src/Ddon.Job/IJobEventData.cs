using MediatR;
using System;

namespace Ddon.Job
{
    public interface IJobEventData : INotification
    {
        Guid Id { get; set; }
    }
}
