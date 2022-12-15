﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Use.Reflection;
using Ddon.Job.Event;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Job.Handler
{
    internal class JobInvokeHandle : INotificationHandler<JobInvokeEventData>
    {
        private readonly IServiceProvider _serviceProvider;

        public JobInvokeHandle(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(JobInvokeEventData data, CancellationToken cancellationToken)
        {
            var classType = DdonType.GetTypeByName<IJob>(data.JobClassName);
            var instance = _serviceProvider.GetRequiredService(classType) ??
                throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var method = DdonType.GetMothodByName(classType, data.JobMethodName);
            await DdonInvoke.InvokeAsync(instance, method);
        }
    }
}