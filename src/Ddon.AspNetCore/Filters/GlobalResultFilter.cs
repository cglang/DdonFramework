using Ddon.Domain.Exceptions;
using Ddon.Identity.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Ddon.AspNetCore.Filters
{
    /// <summary>
    /// 全局异常捕获
    /// </summary>
    public class GlobalResultFilter : IAsyncExceptionFilter, IAsyncResultFilter
    {
        private readonly ILogger<GlobalResultFilter> _logger;

        public GlobalResultFilter(ILogger<GlobalResultFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.ExceptionHandled == false)
            {
                if (context.Exception is UnauthorizedException)
                {
                    context.Result = await UniteHandleAsync(false, context.Exception.Message, StatusCodes.Status401Unauthorized);
                }
                else if (context.Exception is UnauthenticationException)
                {
                    context.Result = await UniteHandleAsync(false, context.Exception.Message, StatusCodes.Status403Forbidden);
                }
                else if (context.Exception is ApplicationServiceException)
                {
                    context.Result = await UniteHandleAsync(false, context.Exception.Message, StatusCodes.Status200OK);
                }
                else
                {
                    context.Result = UniteHandleAsync(false, $"未知错误:{context.Exception.Message}", StatusCodes.Status500InternalServerError).Result;
                    _logger.LogError(context.Exception, context.Exception.Message);
                }
            }
            context.ExceptionHandled = true;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult)
            {
                context.Result = await UniteHandleAsync(true, string.Empty, StatusCodes.Status200OK, objectResult.Value);
            }
            await next();
        }

        /// <summary>
        /// 统一处理
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static async Task<ContentResult> UniteHandleAsync(bool success, string message, int statusCode, object? data = default)
        {
            await Task.CompletedTask;

            if (data is null)
            {
                return new ContentResult
                {
                    Content = JsonSerializer.Serialize(new GlobalResult(success, statusCode, message), options),
                    StatusCode = statusCode,
                    ContentType = "application/json;charset=utf-8"
                };
            }
            else
            {
                return new ContentResult
                {
                    Content = JsonSerializer.Serialize(new GlobalResult<object>(success, statusCode, message, data)),
                    StatusCode = statusCode,
                    ContentType = "application/json;charset=utf-8"
                };
            }
        }

        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };
    }
}
