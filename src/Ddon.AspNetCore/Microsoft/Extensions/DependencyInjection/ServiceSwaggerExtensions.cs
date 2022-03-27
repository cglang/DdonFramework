using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceSwaggerExtensions
    {
        public static void AddDefaultSwagger<THostName>(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
              {
                  c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gardener.HttpApi.Host", Version = "v1" });

                  // 添加项目xml注释
                  var commentsFileName = $"{typeof(THostName).Assembly.GetName().Name}.xml";
                  c.IncludeXmlComments(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, commentsFileName));

                  // 开启权限小锁
                  c.OperationFilter<AddResponseHeadersFilter>();
                  c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                  // 在header中添加token，传递到后台
                  c.OperationFilter<SecurityRequirementsOperationFilter>();
                  c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                  {
                      Description = "JWT授权(数据将在请求头中进行传递)直接在下面框中输入Bearer {token}(注意两者之间是一个空格) \"",
                      Name = "Authorization",           // jwt默认的参数名称
                      In = ParameterLocation.Header,    // jwt默认存放Authorization信息的位置(请求头中)
                      Type = SecuritySchemeType.ApiKey
                  });
              });
        }
    }
}
