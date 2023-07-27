using System.IO;
using System.Text;
using System;
using System.Threading.Tasks;
using Ddon.ConvenientSocket.Exceptions;
using Ddon.Core.Use.Pipeline;
using Ddon.Socket.Serialize;
using Ddon.Socket.Session.Model;
using Ddon.Socket.Session.Pipeline;
using Ddon.Socket.Session.Route;
using Ddon.Socket.Utility;

namespace Ddon.Socket.Session.Middleware
{
    public class EndPointMiddleware : ISocketMiddleware
    {
        private readonly SocketInvoke _caller;
        private readonly ISocketSerialize _socketSerialize;

        public EndPointMiddleware(SocketInvoke caller, ISocketSerialize socketSerialize)
        {
            _caller = caller;
            _socketSerialize = socketSerialize;
        }

        public async Task InvokeAsync(SocketContext context, MiddlewareDelegate<SocketContext> next)
        {
            await (context.Head.Mode switch
            {
                SocketMode.String => ModeOfStringAsync(context),
                SocketMode.Byte => ModeOfByteAsync(context),
                SocketMode.File => ModeOfFileAsync(context),
                SocketMode.Request => ModeOfRequestAsync(context),
                SocketMode.Response => ModeOfResponse(context),
                _ => Task.CompletedTask,
            });

            await next(context);


            Task ModeOfStringAsync(SocketContext context)
            {
                var textdata = Encoding.UTF8.GetString(context.SourceData.Span);
                return _caller.InvokeAsync(context, textdata);
            }

            Task ModeOfByteAsync(SocketContext context)
            {
                var textdata = Encoding.UTF8.GetString(context.SourceData.Span);
                return _caller.InvokeAsync(context, textdata);
            }

            Task ModeOfFileAsync(SocketContext context)
            {
                // TODO: File 需要有一个抽象层接口

                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Socket", "File", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                Directory.CreateDirectory(filePath);

                var fullName = Path.Combine(filePath, $"{context.Session.SessionId}.{DateTime.UtcNow:mmssffff}.{context.Head.FileName ?? string.Empty}");
                using var fileStream = new FileStream(fullName, FileMode.CreateNew);
                fileStream.Write(context.SourceData.Span);
                fileStream.Close();

                return _caller.InvokeAsync(context, fullName);
            }

            async Task ModeOfRequestAsync(SocketContext context)
            {
                var jsonData = Encoding.UTF8.GetString(context.SourceData.Span);
                var methodReturn = await _caller.InvokeAsync(context, jsonData);

                var responseData = new SocketResponse<object>(SocketResponseCode.OK, methodReturn);
                var methodReturnJsonBytes = _socketSerialize.SerializeOfByte(responseData);

                var responseHeadBytes = _socketSerialize.SerializeOfByte(context.Head.Response());

                await context.Session.SendBytesAsync(BitConverter.GetBytes(responseHeadBytes.Length), responseHeadBytes, methodReturnJsonBytes);
            }

            Task ModeOfResponse(SocketContext context)
            {
                if (!TimeoutRecordProcessor.ContainsKey(context.Head.Id))
                    return Task.CompletedTask;

                var request = TimeoutRecordProcessor.Get(context.Head.Id);
                if (request.IsCompleted)
                    return Task.CompletedTask;

                var res = _socketSerialize.Deserialize<SocketResponse<object>>(context.SourceData);

                if (res != null)
                {
                    switch (res.Code)
                    {
                        case SocketResponseCode.OK:
                            request.ActionHandler(_socketSerialize.SerializeOfString(res.Data));
                            break;
                        case SocketResponseCode.Error:
                            var ex = new DdonSocketRequestException(_socketSerialize.SerializeOfString(res.Data));
                            request.ExceptionHandler(ex);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return Task.CompletedTask;
            }
        }
    }
}
