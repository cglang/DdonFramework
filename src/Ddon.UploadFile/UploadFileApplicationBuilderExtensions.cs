using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.UploadFile
{
    public static class UploadFileApplicationBuilderExtensions
    {
        /// <summary>
        /// 启用文件上传中间件
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static void UseUploadFile(this IApplicationBuilder app)
        {
            app.Map(new PathString("/Api/UploadFile"), map =>
            map.Run(async (context) =>
            {
                var uploadFileService = context.RequestServices.GetService<IUploadFileService>();

                //var dtos = await uploadFileService!.UploadFiles(context.Request.Form.Files.Select(x => x));

                await context.Response.WriteAsJsonAsync(new { Name = "cglang" });
            }));
        }
    }
}
