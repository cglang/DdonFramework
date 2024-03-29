﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Ddon.Schedule
{
    public interface ISchedules { }

    public interface ISchedule : INotification
    {
        public Task InvokeAsync(CancellationToken cancellationToken);
    }
}
