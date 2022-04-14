using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Ddon.UploadFile
{
    public class UploadFileMiddleware
    {
        private readonly RequestDelegate _next;
        public UploadFileMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var t = httpContext.RequestServices.GetService(typeof(string));


            Console.WriteLine("CustomMiddleware (in)");
            await _next.Invoke(httpContext);
            Console.WriteLine("CustomMiddleware (out)");
        }
    }
}
