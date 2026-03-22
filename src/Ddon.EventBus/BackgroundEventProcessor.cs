using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Ddon.EventBus.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ddon.EventBus
{
    // 单例注册到DI容器中
    public class BackgroundEventProcessor
    {
        private readonly Channel<IEventData> _eventDataQueue = Channel.CreateUnbounded<IEventData>();
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        private bool _processing;

        public BackgroundEventProcessor(IMediator mediator, ILogger<BackgroundEventProcessor> logger)
        {
            _mediator = mediator;
            _logger = logger;

            // 启动后台消费任务
            Task.Run(() => ProcessQueueAsync());
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public async Task EnqueueAsync(IEventData eventData, CancellationToken cancellationToken = default)
        {
            await _eventDataQueue.Writer.WriteAsync(eventData, cancellationToken);
        }

        /// <summary>
        /// 持续消费队列
        /// </summary>
        private async Task ProcessQueueAsync()
        {
            lock (this)
            {
                if (_processing) return;

                _processing = true;
            }

            while (await _eventDataQueue.Reader.WaitToReadAsync())
            {
                while (_eventDataQueue.Reader.TryRead(out var eventData))
                {
                    try
                    {
                        await _mediator.Publish(eventData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while publishing event: {EventData}", eventData);
                    }
                }
            }
        }
    }
}
