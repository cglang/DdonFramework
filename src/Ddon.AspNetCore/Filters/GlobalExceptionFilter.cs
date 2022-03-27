using Ddon.Core.Exceptions;
using Ddon.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ddon.AspNetCore.Filters
{
    /// <summary>
    /// 全局异常捕获
    /// </summary>
    public class GlobalExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.ExceptionHandled == false)
            {
                if (context.Exception is UnauthorizedException)
                {
                    context.Result = UniteHandleAsync(context.Exception.Message, StatusCodes.Status401Unauthorized).Result;
                }
                else if (context.Exception is ApplicationServiceException)
                {
                    context.Result = UniteHandleAsync(context.Exception.Message, StatusCodes.Status200OK).Result;
                }
                else
                {
                    context.Result = UniteHandleAsync(context.Exception.Message, StatusCodes.Status500InternalServerError).Result;
                    _logger.LogError(context.Exception, context.Exception.Message);
                }
            }
            context.ExceptionHandled = true;    // 标记异常为已处理
            return Task.CompletedTask;
        }

        /// <summary>
        /// 统一处理
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static async Task<ContentResult> UniteHandleAsync(string message, int statusCode)
        {
            await Task.CompletedTask;

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(new GlobalExceptionResult(false, statusCode, message)),
                StatusCode = statusCode,
                ContentType = "application/json;charset=utf-8"
            };
        }
    }
}
